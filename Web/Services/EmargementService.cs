using Application.Common.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Web.Data;

namespace Web.Services;

public sealed class EmargementService(ApplicationDbContext dbContext, IEmailService emailService) : IEmargementService
{
    public async Task<(bool Success, string? ErrorMessage)> EnvoyerEmargementsEtapeCouranteAsync(int cohorteId, Func<int, string> construireLienSignature)
    {
        var cohorte = await dbContext.Cohortes.Include(c => c.Challenge).FirstOrDefaultAsync(c => c.Id == cohorteId);
        if (cohorte is null)
        {
            return (false, "Cohorte introuvable.");
        }

        if (cohorte.Statut != StatutCohorte.Active)
        {
            return (false, "Seule une Cohorte active a une étape courante.");
        }

        var etape = await dbContext.ChallengeEtapes
            .FirstOrDefaultAsync(e => e.ChallengeId == cohorte.ChallengeId && e.NumeroEtape == cohorte.EtapeCourante);
        if (etape is null)
        {
            return (false, "Étape introuvable.");
        }

        var attributions = await dbContext.CarteAttributions
            .Include(a => a.CarteCompetence)
            .Include(a => a.Utilisateur)
            .Where(a => a.CohorteId == cohorteId && a.ChallengeEtapeId == etape.Id && a.EstActif)
            .ToListAsync();

        if (attributions.Count == 0)
        {
            return (false, "Aucune carte attribuée pour l'étape en cours.");
        }

        var attributionIds = attributions.Select(a => a.Id).ToList();
        var emargementsExistants = await dbContext.Emargements
            .Where(e => attributionIds.Contains(e.CarteAttributionId))
            .ToListAsync();

        var maintenant = DateTime.UtcNow;
        foreach (var attribution in attributions)
        {
            var emargement = emargementsExistants.FirstOrDefault(e => e.CarteAttributionId == attribution.Id);
            if (emargement is null)
            {
                emargement = new Emargement { CarteAttributionId = attribution.Id };
                dbContext.Emargements.Add(emargement);
            }

            emargement.EnvoyeLe = maintenant;
        }
        await dbContext.SaveChangesAsync();

        foreach (var utilisateurId in attributions.Select(a => a.UtilisateurId).Distinct())
        {
            var utilisateur = attributions.First(a => a.UtilisateurId == utilisateurId).Utilisateur;
            if (string.IsNullOrWhiteSpace(utilisateur.Email))
            {
                continue;
            }

            var carteTitres = attributions
                .Where(a => a.UtilisateurId == utilisateurId)
                .Select(a => a.CarteCompetence.TitreTheorie)
                .ToList();

            var lien = construireLienSignature(cohorteId);
            var (sujet, corps) = ChallengeEmailTemplates.DemandeEmargement(cohorte.Challenge.Titre, etape.TitreEtape, carteTitres, lien);

            await emailService.EnvoyerAsync(utilisateur.Email, sujet, corps);
        }

        return (true, null);
    }

    public async Task<List<EmargementMembreInfo>> GetSuiviEtapeCouranteAsync(int cohorteId)
    {
        var cohorte = await dbContext.Cohortes.FirstOrDefaultAsync(c => c.Id == cohorteId);
        if (cohorte is null || cohorte.Statut != StatutCohorte.Active)
        {
            return [];
        }

        var etape = await dbContext.ChallengeEtapes
            .FirstOrDefaultAsync(e => e.ChallengeId == cohorte.ChallengeId && e.NumeroEtape == cohorte.EtapeCourante);
        if (etape is null)
        {
            return [];
        }

        var attributions = await dbContext.CarteAttributions
            .Include(a => a.CarteCompetence)
            .Include(a => a.Utilisateur)
            .Where(a => a.CohorteId == cohorteId && a.ChallengeEtapeId == etape.Id && a.EstActif)
            .ToListAsync();

        if (attributions.Count == 0)
        {
            return [];
        }

        var attributionIds = attributions.Select(a => a.Id).ToList();
        var emargements = await dbContext.Emargements
            .Where(e => attributionIds.Contains(e.CarteAttributionId))
            .ToListAsync();

        return attributions
            .GroupBy(a => a.UtilisateurId)
            .Select(g => new EmargementMembreInfo
            {
                UtilisateurId = g.Key,
                NomComplet = NomComplet(g.First().Utilisateur),
                Cartes = g.Select(a =>
                {
                    var emargement = emargements.FirstOrDefault(e => e.CarteAttributionId == a.Id);
                    return new EmargementCarteInfo
                    {
                        EmargementId = emargement?.Id ?? 0,
                        CarteCompetenceId = a.CarteCompetenceId,
                        CarteTitre = a.CarteCompetence.TitreTheorie,
                        Signe = emargement?.SigneLe is not null,
                    };
                }).ToList(),
            })
            .ToList();
    }

    public async Task<EmargementPourSignatureInfo?> GetPourSignatureAsync(int cohorteId, string utilisateurId)
    {
        var cohorte = await dbContext.Cohortes.Include(c => c.Challenge).FirstOrDefaultAsync(c => c.Id == cohorteId);
        if (cohorte is null || cohorte.Statut != StatutCohorte.Active)
        {
            return null;
        }

        var etape = await dbContext.ChallengeEtapes
            .FirstOrDefaultAsync(e => e.ChallengeId == cohorte.ChallengeId && e.NumeroEtape == cohorte.EtapeCourante);
        if (etape is null)
        {
            return null;
        }

        var attributions = await dbContext.CarteAttributions
            .Include(a => a.CarteCompetence)
            .Where(a => a.CohorteId == cohorteId && a.ChallengeEtapeId == etape.Id && a.UtilisateurId == utilisateurId && a.EstActif)
            .ToListAsync();

        if (attributions.Count == 0)
        {
            return null;
        }

        var attributionIds = attributions.Select(a => a.Id).ToList();
        var emargements = await dbContext.Emargements
            .Where(e => attributionIds.Contains(e.CarteAttributionId) && e.EnvoyeLe != null)
            .ToListAsync();

        // Rien n'a encore ete envoye pour cette etape (bouton "Envoyer les emargements"
        // pas encore utilise cote Gestionnaire) : pas de fausse demande de signature.
        if (emargements.Count == 0)
        {
            return null;
        }

        return new EmargementPourSignatureInfo
        {
            NumeroEtape = cohorte.EtapeCourante,
            ChallengeTitre = cohorte.Challenge.Titre,
            HeuresPresence = emargements.Select(e => e.HeuresPresence).FirstOrDefault(v => v is not null),
            HeuresTravailPersonnel = emargements.Select(e => e.HeuresTravailPersonnel).FirstOrDefault(v => v is not null),
            Cartes = attributions
                .Where(a => emargements.Any(e => e.CarteAttributionId == a.Id))
                .Select(a =>
                {
                    var emargement = emargements.First(e => e.CarteAttributionId == a.Id);
                    return new EmargementCarteInfo
                    {
                        EmargementId = emargement.Id,
                        CarteCompetenceId = a.CarteCompetenceId,
                        CarteTitre = a.CarteCompetence.TitreTheorie,
                        Signe = emargement.SigneLe is not null,
                    };
                }).ToList(),
        };
    }

    public async Task<(bool Success, string? ErrorMessage)> SignerAsync(int cohorteId, string utilisateurId, List<int> emargementIdsConfirmes, decimal? heuresPresence, decimal? heuresTravailPersonnel)
    {
        var cohorte = await dbContext.Cohortes.FirstOrDefaultAsync(c => c.Id == cohorteId);
        if (cohorte is null || cohorte.Statut != StatutCohorte.Active)
        {
            return (false, "Cohorte introuvable ou non active.");
        }

        var etape = await dbContext.ChallengeEtapes
            .FirstOrDefaultAsync(e => e.ChallengeId == cohorte.ChallengeId && e.NumeroEtape == cohorte.EtapeCourante);
        if (etape is null)
        {
            return (false, "Étape introuvable.");
        }

        // Restreint aux propres cartes du membre : impossible de signer l'emargement de
        // quelqu'un d'autre meme en forgeant les identifiants postes.
        var attributionIds = await dbContext.CarteAttributions
            .Where(a => a.CohorteId == cohorteId && a.ChallengeEtapeId == etape.Id && a.UtilisateurId == utilisateurId && a.EstActif)
            .Select(a => a.Id)
            .ToListAsync();

        var emargements = await dbContext.Emargements
            .Where(e => attributionIds.Contains(e.CarteAttributionId) && e.EnvoyeLe != null)
            .ToListAsync();

        if (emargements.Count == 0)
        {
            return (false, "Aucun émargement à signer pour cette étape.");
        }

        var maintenant = DateTime.UtcNow;
        foreach (var emargement in emargements)
        {
            if (emargement.SigneLe is null && emargementIdsConfirmes.Contains(emargement.Id))
            {
                emargement.SigneLe = maintenant;
            }

            emargement.HeuresPresence = heuresPresence;
            emargement.HeuresTravailPersonnel = heuresTravailPersonnel;
        }

        await dbContext.SaveChangesAsync();

        return (true, null);
    }

    private static string NomComplet(ApplicationUser utilisateur)
    {
        var nomComplet = $"{utilisateur.Prenom} {utilisateur.Nom}".Trim();
        return nomComplet.Length > 0 ? nomComplet : (utilisateur.Email ?? utilisateur.UserName ?? utilisateur.Id);
    }
}

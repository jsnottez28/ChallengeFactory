using Application.Common.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Web.Data;

namespace Web.Services;

public sealed class EmargementService(
    ApplicationDbContext dbContext,
    IEmailService emailService,
    IPreuveFichierStockageService stockageService) : IEmargementService
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

        // La date de seance emargee est toujours celle de la visio planifiee pour l'etape
        // (cf. Emargement.DateSeance) : impossible d'envoyer des emargements tant qu'aucune
        // date de visio n'a ete fixee, pour ne jamais tracer une seance a une date inventee.
        var visio = await dbContext.EtapesVisio
            .FirstOrDefaultAsync(v => v.CohorteId == cohorteId && v.NumeroEtape == cohorte.EtapeCourante);
        if (visio?.DateVisio is null)
        {
            return (false, "Planifiez d'abord une date de visio pour cette étape avant d'envoyer les émargements.");
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
            emargement.DateSeance = visio.DateVisio;
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
                        ASignature = !string.IsNullOrWhiteSpace(emargement?.SignatureCheminStockage),
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
            DateSeance = emargements.Select(e => e.DateSeance).FirstOrDefault(v => v is not null),
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
                        ASignature = !string.IsNullOrWhiteSpace(emargement.SignatureCheminStockage),
                    };
                }).ToList(),
        };
    }

    public async Task<(bool Success, string? ErrorMessage)> SignerAsync(int cohorteId, string utilisateurId, List<int> emargementIdsConfirmes, decimal? heuresPresence, decimal? heuresTravailPersonnel, byte[]? signaturePng)
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

        var aSignerMaintenant = emargements.Where(e => e.SigneLe is null && emargementIdsConfirmes.Contains(e.Id)).ToList();

        // La signature dessinee certifie la participation - jamais une simple case cochee
        // sans trace graphique (cf. Emargement.SignatureCheminStockage).
        if (aSignerMaintenant.Count > 0 && (signaturePng is null || signaturePng.Length == 0))
        {
            return (false, "Votre signature est obligatoire pour valider cet émargement.");
        }

        string? cheminSignature = null;
        if (aSignerMaintenant.Count > 0)
        {
            using var contenu = new MemoryStream(signaturePng!);
            cheminSignature = await stockageService.EnregistrerAsync(contenu, $"signature-cohorte{cohorteId}-etape{etape.NumeroEtape}-{utilisateurId}.png");
        }

        var maintenant = DateTime.UtcNow;
        foreach (var emargement in emargements)
        {
            if (emargement.SigneLe is null && emargementIdsConfirmes.Contains(emargement.Id))
            {
                emargement.SigneLe = maintenant;
                emargement.SignatureCheminStockage = cheminSignature;
            }

            emargement.HeuresPresence = heuresPresence;
            emargement.HeuresTravailPersonnel = heuresTravailPersonnel;
        }

        await dbContext.SaveChangesAsync();

        return (true, null);
    }

    public async Task<(Stream Contenu, string NomFichier)?> TelechargerSignatureAsync(int emargementId, string utilisateurId, bool estGestionnaire)
    {
        var emargement = await dbContext.Emargements
            .Include(e => e.CarteAttribution)
            .FirstOrDefaultAsync(e => e.Id == emargementId);

        if (emargement is null || string.IsNullOrWhiteSpace(emargement.SignatureCheminStockage))
        {
            return null;
        }

        if (!estGestionnaire && emargement.CarteAttribution.UtilisateurId != utilisateurId)
        {
            return null;
        }

        var contenu = await stockageService.TelechargerAsync(emargement.SignatureCheminStockage);
        return contenu is null ? null : (contenu, $"signature-{emargementId}.png");
    }

    private static string NomComplet(ApplicationUser utilisateur)
    {
        var nomComplet = $"{utilisateur.Prenom} {utilisateur.Nom}".Trim();
        return nomComplet.Length > 0 ? nomComplet : (utilisateur.Email ?? utilisateur.UserName ?? utilisateur.Id);
    }
}

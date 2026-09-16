using Application.Common.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Web.Data;

namespace Web.Services;

public sealed class TestPositionnementService(ApplicationDbContext dbContext, IEmailService emailService) : ITestPositionnementService
{
    public async Task<(bool Success, string? ErrorMessage)> EnvoyerAsync(int cohorteId, TypeTestPositionnement type, string gestionnaireId, Func<int, string> construireLienReponse)
    {
        var cohorte = await dbContext.Cohortes
            .Include(c => c.Challenge)
            .Include(c => c.Membres).ThenInclude(m => m.Utilisateur)
            .FirstOrDefaultAsync(c => c.Id == cohorteId);

        if (cohorte is null)
        {
            return (false, "Cohorte introuvable.");
        }

        if (cohorte.Statut != StatutCohorte.Active)
        {
            return (false, "Seule une Cohorte active peut recevoir un test de connaissances.");
        }

        if (type == TypeTestPositionnement.Amont && cohorte.EtapeCourante != 1)
        {
            return (false, "Le test de connaissances en amont ne peut être envoyé qu'à l'étape 1.");
        }

        if (type == TypeTestPositionnement.Aval && cohorte.EtapeCourante != cohorte.Challenge.NombreEtapes)
        {
            return (false, "Le test de connaissances en aval ne peut être envoyé qu'à la dernière étape.");
        }

        if (cohorte.Membres.Count == 0)
        {
            return (false, "Aucun membre dans cette Cohorte.");
        }

        var carteIds = await GetCarteIdsDuChallengeAsync(cohorte.ChallengeId);
        if (carteIds.Count == 0)
        {
            return (false, "Ce Challenge n'a aucune carte de compétences à évaluer.");
        }

        var test = await dbContext.TestsPositionnement.FirstOrDefaultAsync(t => t.CohorteId == cohorteId && t.Type == type);
        if (test is null)
        {
            test = new TestPositionnement
            {
                CohorteId = cohorteId,
                Type = type,
                EnvoyeParId = gestionnaireId,
            };
            dbContext.TestsPositionnement.Add(test);
        }

        test.EnvoyeLe = DateTime.UtcNow;
        await dbContext.SaveChangesAsync();

        var lien = construireLienReponse(cohorteId);
        var (sujet, corps) = ChallengeEmailTemplates.DemandeTestPositionnement(cohorte.Challenge.Titre, type, lien);

        foreach (var membre in cohorte.Membres)
        {
            if (!string.IsNullOrWhiteSpace(membre.Utilisateur.Email))
            {
                await emailService.EnvoyerAsync(membre.Utilisateur.Email, sujet, corps);
            }
        }

        return (true, null);
    }

    public async Task<TestPositionnementInfo?> GetPourReponseAsync(int cohorteId, TypeTestPositionnement type, string utilisateurId)
    {
        var test = await dbContext.TestsPositionnement
            .Include(t => t.Cohorte).ThenInclude(c => c.Challenge)
            .FirstOrDefaultAsync(t => t.CohorteId == cohorteId && t.Type == type);

        if (test is null)
        {
            return null;
        }

        var estMembre = await dbContext.CohorteMembres.AnyAsync(m => m.CohorteId == cohorteId && m.UtilisateurId == utilisateurId);
        if (!estMembre)
        {
            return null;
        }

        var cartes = await dbContext.ChallengeEtapeCartes
            .Include(ec => ec.CarteCompetence)
            .Where(ec => ec.ChallengeEtape.ChallengeId == test.Cohorte.ChallengeId)
            .Select(ec => ec.CarteCompetence)
            .Distinct()
            .ToListAsync();

        var reponses = await dbContext.TestsPositionnementReponses
            .Where(r => r.TestPositionnementId == test.Id && r.UtilisateurId == utilisateurId)
            .ToListAsync();

        return new TestPositionnementInfo
        {
            Type = type,
            ChallengeTitre = test.Cohorte.Challenge.Titre,
            Cartes = cartes
                .OrderBy(c => c.TitreTheorie)
                .Select(c => new TestPositionnementCarteInfo
                {
                    CarteCompetenceId = c.Id,
                    CarteTitre = c.TitreTheorie,
                    NiveauDejaRepondu = reponses.FirstOrDefault(r => r.CarteCompetenceId == c.Id)?.NiveauAutoEvalue,
                })
                .ToList(),
        };
    }

    public async Task<bool> ADejaReponduAsync(int cohorteId, TypeTestPositionnement type, string utilisateurId)
    {
        var testId = await dbContext.TestsPositionnement
            .Where(t => t.CohorteId == cohorteId && t.Type == type)
            .Select(t => (int?)t.Id)
            .FirstOrDefaultAsync();

        if (testId is null)
        {
            return false;
        }

        return await dbContext.TestsPositionnementReponses.AnyAsync(r => r.TestPositionnementId == testId && r.UtilisateurId == utilisateurId);
    }

    public async Task<(bool Success, string? ErrorMessage)> RepondreAsync(int cohorteId, TypeTestPositionnement type, string utilisateurId, Dictionary<int, int> niveauxParCarte)
    {
        var test = await dbContext.TestsPositionnement
            .Include(t => t.Cohorte)
            .FirstOrDefaultAsync(t => t.CohorteId == cohorteId && t.Type == type);

        if (test is null)
        {
            return (false, "Aucun test de connaissances à répondre pour le moment.");
        }

        var estMembre = await dbContext.CohorteMembres.AnyAsync(m => m.CohorteId == cohorteId && m.UtilisateurId == utilisateurId);
        if (!estMembre)
        {
            return (false, "Vous n'êtes pas membre de cette Cohorte.");
        }

        if (await ADejaReponduAsync(cohorteId, type, utilisateurId))
        {
            return (false, "Vous avez déjà répondu à ce test.");
        }

        var carteIds = await GetCarteIdsDuChallengeAsync(test.Cohorte.ChallengeId);
        if (carteIds.Count == 0)
        {
            return (false, "Ce Challenge n'a aucune carte de compétences à évaluer.");
        }

        foreach (var carteId in carteIds)
        {
            if (!niveauxParCarte.TryGetValue(carteId, out var niveau) || niveau < 0 || niveau > 10)
            {
                return (false, "Merci d'évaluer votre niveau (0 à 10) pour chaque carte du parcours.");
            }
        }

        var maintenant = DateTime.UtcNow;
        foreach (var carteId in carteIds)
        {
            dbContext.TestsPositionnementReponses.Add(new TestPositionnementReponse
            {
                TestPositionnementId = test.Id,
                UtilisateurId = utilisateurId,
                CarteCompetenceId = carteId,
                NiveauAutoEvalue = niveauxParCarte[carteId],
                RepondueLe = maintenant,
            });
        }
        await dbContext.SaveChangesAsync();

        return (true, null);
    }

    public async Task<TestPositionnementStats> GetStatsAsync(int cohorteId, TypeTestPositionnement type)
    {
        var nombreMembres = await dbContext.CohorteMembres.CountAsync(m => m.CohorteId == cohorteId);

        var test = await dbContext.TestsPositionnement.FirstOrDefaultAsync(t => t.CohorteId == cohorteId && t.Type == type);
        if (test is null)
        {
            return new TestPositionnementStats { Envoye = false, NombreMembresTotal = nombreMembres };
        }

        var reponses = await dbContext.TestsPositionnementReponses
            .Include(r => r.CarteCompetence)
            .Where(r => r.TestPositionnementId == test.Id)
            .ToListAsync();

        return new TestPositionnementStats
        {
            Envoye = true,
            NombreRepondants = reponses.Select(r => r.UtilisateurId).Distinct().Count(),
            NombreMembresTotal = nombreMembres,
            NiveauMoyenGlobal = reponses.Count > 0 ? reponses.Average(r => r.NiveauAutoEvalue) : null,
            ParCarte = reponses
                .GroupBy(r => r.CarteCompetence.TitreTheorie)
                .Select(g => new TestPositionnementCarteStat { CarteTitre = g.Key, NiveauMoyen = g.Average(r => r.NiveauAutoEvalue) })
                .OrderBy(c => c.CarteTitre)
                .ToList(),
        };
    }

    private async Task<List<int>> GetCarteIdsDuChallengeAsync(int challengeId) =>
        await dbContext.ChallengeEtapeCartes
            .Where(ec => ec.ChallengeEtape.ChallengeId == challengeId)
            .Select(ec => ec.CarteCompetenceId)
            .Distinct()
            .ToListAsync();
}

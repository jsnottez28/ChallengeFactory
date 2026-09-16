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

        var etapes = await GetEtapesDuChallengeAsync(cohorte.ChallengeId);
        if (etapes.Count == 0)
        {
            return (false, "Ce Challenge n'a aucune étape à évaluer.");
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

        var etapes = await GetEtapesDuChallengeAsync(test.Cohorte.ChallengeId);

        var reponses = await dbContext.TestsPositionnementReponses
            .Where(r => r.TestPositionnementId == test.Id && r.UtilisateurId == utilisateurId)
            .ToListAsync();

        return new TestPositionnementInfo
        {
            Type = type,
            ChallengeTitre = test.Cohorte.Challenge.Titre,
            Etapes = etapes
                .Select(e => new TestPositionnementEtapeInfo
                {
                    ChallengeEtapeId = e.Id,
                    NumeroEtape = e.NumeroEtape,
                    Libelle = EtapeLibelle(e),
                    NiveauDejaRepondu = reponses.FirstOrDefault(r => r.ChallengeEtapeId == e.Id)?.NiveauAutoEvalue,
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

    public async Task<(bool Success, string? ErrorMessage)> RepondreAsync(int cohorteId, TypeTestPositionnement type, string utilisateurId, Dictionary<int, int> niveauxParEtape)
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

        var etapes = await GetEtapesDuChallengeAsync(test.Cohorte.ChallengeId);
        if (etapes.Count == 0)
        {
            return (false, "Ce Challenge n'a aucune étape à évaluer.");
        }

        foreach (var etape in etapes)
        {
            if (!niveauxParEtape.TryGetValue(etape.Id, out var niveau) || niveau < 0 || niveau > 10)
            {
                return (false, "Merci d'évaluer votre niveau (0 à 10) pour chaque étape du parcours.");
            }
        }

        var maintenant = DateTime.UtcNow;
        foreach (var etape in etapes)
        {
            dbContext.TestsPositionnementReponses.Add(new TestPositionnementReponse
            {
                TestPositionnementId = test.Id,
                UtilisateurId = utilisateurId,
                ChallengeEtapeId = etape.Id,
                NiveauAutoEvalue = niveauxParEtape[etape.Id],
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
            .Include(r => r.ChallengeEtape)
            .Where(r => r.TestPositionnementId == test.Id)
            .ToListAsync();

        return new TestPositionnementStats
        {
            Envoye = true,
            NombreRepondants = reponses.Select(r => r.UtilisateurId).Distinct().Count(),
            NombreMembresTotal = nombreMembres,
            NiveauMoyenGlobal = reponses.Count > 0 ? reponses.Average(r => r.NiveauAutoEvalue) : null,
            ParEtape = reponses
                .GroupBy(r => r.ChallengeEtape)
                .Select(g => new TestPositionnementEtapeStat { NumeroEtape = g.Key.NumeroEtape, Libelle = EtapeLibelle(g.Key), NiveauMoyen = g.Average(r => r.NiveauAutoEvalue) })
                .OrderBy(e => e.NumeroEtape)
                .ToList(),
        };
    }

    private async Task<List<ChallengeEtape>> GetEtapesDuChallengeAsync(int challengeId) =>
        await dbContext.ChallengeEtapes
            .Where(e => e.ChallengeId == challengeId)
            .OrderBy(e => e.NumeroEtape)
            .ToListAsync();

    // L'objectif pedagogique est plus parlant qu'un titre d'etape ou de carte pour un
    // stagiaire qui s'auto-evalue - repli sur le titre si l'objectif n'a pas ete renseigne.
    private static string EtapeLibelle(ChallengeEtape etape) =>
        !string.IsNullOrWhiteSpace(etape.ObjectifPedagogique) ? etape.ObjectifPedagogique! : etape.TitreEtape;
}

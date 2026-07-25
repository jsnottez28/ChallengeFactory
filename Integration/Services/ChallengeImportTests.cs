using Application.Common.Interfaces;
using ClosedXML.Excel;
using Domain.Entities;
using Integration.TestSupport;
using Microsoft.EntityFrameworkCore;
using Web.Services;

namespace Integration.Services;

public class ChallengeImportTests
{
    private static MemoryStream ConstruireClasseur(
        (string ChallengeCode, string Titre, string? Slogan, int NombreEtapes, string Mode, string? Statut)[] challenges,
        (string ChallengeCode, int NumeroEtape, string TitreEtape, string? CodesCartes)[] etapes)
    {
        using var classeur = new XLWorkbook();

        var feuilleChallenge = classeur.AddWorksheet("challenge");
        feuilleChallenge.Cell(1, 1).Value = "challenge_code";
        feuilleChallenge.Cell(1, 2).Value = "titre";
        feuilleChallenge.Cell(1, 3).Value = "slogan";
        feuilleChallenge.Cell(1, 4).Value = "nombre_etapes";
        feuilleChallenge.Cell(1, 5).Value = "mode";
        feuilleChallenge.Cell(1, 6).Value = "statut";
        for (var i = 0; i < challenges.Length; i++)
        {
            var ligne = i + 2;
            var c = challenges[i];
            feuilleChallenge.Cell(ligne, 1).Value = c.ChallengeCode;
            feuilleChallenge.Cell(ligne, 2).Value = c.Titre;
            feuilleChallenge.Cell(ligne, 3).Value = c.Slogan ?? string.Empty;
            feuilleChallenge.Cell(ligne, 4).Value = c.NombreEtapes;
            feuilleChallenge.Cell(ligne, 5).Value = c.Mode;
            feuilleChallenge.Cell(ligne, 6).Value = c.Statut ?? string.Empty;
        }

        var feuilleEtapes = classeur.AddWorksheet("etapes");
        feuilleEtapes.Cell(1, 1).Value = "challenge_code";
        feuilleEtapes.Cell(1, 2).Value = "numero_etape";
        feuilleEtapes.Cell(1, 3).Value = "titre_etape";
        feuilleEtapes.Cell(1, 4).Value = "objectif_pedagogique";
        feuilleEtapes.Cell(1, 5).Value = "competence_cible";
        feuilleEtapes.Cell(1, 6).Value = "defi_individuel";
        feuilleEtapes.Cell(1, 7).Value = "codes_cartes";
        for (var i = 0; i < etapes.Length; i++)
        {
            var ligne = i + 2;
            var e = etapes[i];
            feuilleEtapes.Cell(ligne, 1).Value = e.ChallengeCode;
            feuilleEtapes.Cell(ligne, 2).Value = e.NumeroEtape;
            feuilleEtapes.Cell(ligne, 3).Value = e.TitreEtape;
            feuilleEtapes.Cell(ligne, 7).Value = e.CodesCartes ?? string.Empty;
        }

        var flux = new MemoryStream();
        classeur.SaveAs(flux);
        flux.Position = 0;
        return flux;
    }

    [Fact]
    public async Task ImporterAsync_CreeLeChallengeSesEtapesEtRattacheLesCartesExistantes_PuisPublie()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var challengeService = new ChallengeService(dbContext);
        var carteService = new CarteCompetenceService(dbContext);

        var (_, _, carte) = await carteService.CreateAsync(new CarteCompetenceInput { Code = "TPE-001", Niveau = NiveauCarte.Debutant, TitreTheorie = "Diagnostic" });

        await using var flux = ConstruireClasseur(
            [("CHAL-TEST", "Engager son équipe", "Un slogan", 2, "BtoB", "Publie")],
            [
                ("CHAL-TEST", 1, "Diagnostiquer", "TPE-001"),
                ("CHAL-TEST", 2, "Cartographier", null),
            ]);

        var rapport = await challengeService.ImporterAsync(flux);

        Assert.Empty(rapport.Erreurs);
        Assert.Equal(1, rapport.ChallengesCrees);
        Assert.Equal(2, rapport.EtapesCreees);
        Assert.Equal(1, rapport.CartesRattachees);
        Assert.Equal(1, rapport.ChallengesPublies);

        var challenge = await dbContext.Challenges
            .Include(c => c.Etapes)
                .ThenInclude(e => e.Cartes)
            .SingleAsync(c => c.Code == "CHAL-TEST");

        Assert.Equal("Engager son équipe", challenge.Titre);
        Assert.Equal(StatutChallenge.Publie, challenge.Statut);
        Assert.Equal(2, challenge.Etapes.Count);

        var etape1 = challenge.Etapes.Single(e => e.NumeroEtape == 1);
        var carteRattachee = Assert.Single(etape1.Cartes);
        Assert.Equal(carte!.Id, carteRattachee.CarteCompetenceId);
    }

    [Fact]
    public async Task ImporterAsync_ReimportDuMemeFichier_MetAJourAuLieuDeDupliquer()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var challengeService = new ChallengeService(dbContext);

        var challengesFichier = new[] { ("CHAL-TEST", "Titre initial", (string?)null, 1, "BtoC", (string?)null) };
        var etapesFichier = new[] { ("CHAL-TEST", 1, "Étape 1", (string?)null) };

        await using (var premierFlux = ConstruireClasseur(challengesFichier, etapesFichier))
        {
            await challengeService.ImporterAsync(premierFlux);
        }

        await using var deuxiemeFlux = ConstruireClasseur(
            [("CHAL-TEST", "Titre corrigé", null, 1, "BtoC", null)],
            etapesFichier);

        var rapport = await challengeService.ImporterAsync(deuxiemeFlux);

        Assert.Equal(0, rapport.ChallengesCrees);
        Assert.Equal(1, rapport.ChallengesMisAJour);
        Assert.Equal(0, rapport.EtapesCreees);
        Assert.Equal(1, rapport.EtapesMisesAJour);

        var nombreChallenges = await dbContext.Challenges.CountAsync(c => c.Code == "CHAL-TEST");
        Assert.Equal(1, nombreChallenges);

        var challenge = await dbContext.Challenges.SingleAsync(c => c.Code == "CHAL-TEST");
        Assert.Equal("Titre corrigé", challenge.Titre);
    }

    [Fact]
    public async Task ImporterAsync_SignaleUneCarteInconnue_SansBloquerLeRestant()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var challengeService = new ChallengeService(dbContext);

        await using var flux = ConstruireClasseur(
            [("CHAL-TEST", "Titre", null, 1, "BtoC", null)],
            [("CHAL-TEST", 1, "Étape 1", "CODE-INEXISTANT")]);

        var rapport = await challengeService.ImporterAsync(flux);

        Assert.Equal(1, rapport.ChallengesCrees);
        Assert.Equal(1, rapport.EtapesCreees);
        Assert.Equal(0, rapport.CartesRattachees);

        var erreur = Assert.Single(rapport.Erreurs);
        Assert.Equal("codes_cartes", erreur.Champ);
        Assert.Contains("CODE-INEXISTANT", erreur.Raison);
    }

    [Fact]
    public async Task ImporterAsync_NePublieQueSiAuMoinsUneEtapeAEteImportee()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var challengeService = new ChallengeService(dbContext);

        await using var flux = ConstruireClasseur(
            [("CHAL-TEST", "Titre", null, 3, "BtoC", "Publie")],
            []);

        var rapport = await challengeService.ImporterAsync(flux);

        Assert.Equal(0, rapport.ChallengesPublies);
        Assert.Contains(rapport.Erreurs, e => e.Champ == "statut");

        var challenge = await dbContext.Challenges.SingleAsync(c => c.Code == "CHAL-TEST");
        Assert.Equal(StatutChallenge.Brouillon, challenge.Statut);
    }
}

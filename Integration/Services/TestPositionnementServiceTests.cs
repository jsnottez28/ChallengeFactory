using Application.Common.Interfaces;
using Domain.Entities;
using Integration.TestSupport;
using Web.Data;
using Web.Services;

namespace Integration.Services;

public class TestPositionnementServiceTests
{
    private static async Task<(TestPositionnementService TestPositionnementService, int CohorteId, ApplicationUser Gestionnaire, ApplicationUser Apprenant, int EtapeId)> PreparerAsync(
        ApplicationDbContext dbContext, string? objectifPedagogique)
    {
        var userManager = TestUserManagerFactory.Create(dbContext);
        var emailService = new FakeEmailService();
        var cohorteService = new CohorteService(dbContext, userManager, emailService, new PreuveService(dbContext, userManager, new FakePreuveFichierStockageService(), new NotificationService(dbContext), new FakeEmailService()), new NotificationService(dbContext));
        var challengeService = new ChallengeService(dbContext);
        var carteService = new CarteCompetenceService(dbContext);
        var testPositionnementService = new TestPositionnementService(dbContext, emailService);

        var (_, _, challenge) = await challengeService.CreateAsync(new ChallengeInput { Titre = "Challenge Test", NombreEtapes = 1, Mode = ModePlateforme.BtoC });
        var (_, _, etape) = await challengeService.CreerEtapeAsync(challenge!.Id, new ChallengeEtapeInput { TitreEtape = "Étape 1 — Titre carte", ObjectifPedagogique = objectifPedagogique });
        var (_, _, carte) = await carteService.CreateAsync(new CarteCompetenceInput { Code = "CODE-1", Niveau = NiveauCarte.Debutant, TitreTheorie = "Titre carte peu clair" });
        await challengeService.DefinirCartesEtapeAsync(etape!.Id, [carte!.Id]);
        await challengeService.PublierAsync(challenge.Id);

        var gestionnaire = new ApplicationUser { UserName = "coach@test.local", Email = "coach@test.local" };
        var apprenant = new ApplicationUser { UserName = "apprenant@test.local", Email = "apprenant@test.local" };
        dbContext.Users.AddRange(gestionnaire, apprenant);
        await dbContext.SaveChangesAsync();

        var (_, _, cohorteId) = await cohorteService.CreateAsync(new CohorteInput { ChallengeId = challenge.Id, Nom = "Cohorte Test" });
        await cohorteService.AjouterMembreManuelAsync(cohorteId!.Value, apprenant.Id);
        await cohorteService.LancerAsync(cohorteId.Value, gestionnaire.Id, "https://test.local/parcours", "https://test.local/mi-parcours");

        return (testPositionnementService, cohorteId.Value, gestionnaire, apprenant, etape.Id);
    }

    [Fact]
    public async Task GetPourReponseAsync_UtiliseLObjectifPedagogiqueDeLEtapeCommeLibelle()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var (testPositionnementService, cohorteId, gestionnaire, apprenant, etapeId) = await PreparerAsync(dbContext, "Savoir donner un feedback constructif");

        await testPositionnementService.EnvoyerAsync(cohorteId, TypeTestPositionnement.Amont, gestionnaire.Id, _ => "https://test.local/test");

        var info = await testPositionnementService.GetPourReponseAsync(cohorteId, TypeTestPositionnement.Amont, apprenant.Id);

        Assert.NotNull(info);
        var etape = Assert.Single(info!.Etapes);
        Assert.Equal(etapeId, etape.ChallengeEtapeId);
        Assert.Equal("Savoir donner un feedback constructif", etape.Libelle);
    }

    [Fact]
    public async Task GetPourReponseAsync_ReplieSurLeTitreDeLEtape_SiAucunObjectifRenseigne()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var (testPositionnementService, cohorteId, gestionnaire, apprenant, _) = await PreparerAsync(dbContext, objectifPedagogique: null);

        await testPositionnementService.EnvoyerAsync(cohorteId, TypeTestPositionnement.Amont, gestionnaire.Id, _ => "https://test.local/test");

        var info = await testPositionnementService.GetPourReponseAsync(cohorteId, TypeTestPositionnement.Amont, apprenant.Id);

        var etape = Assert.Single(info!.Etapes);
        Assert.Equal("Étape 1 — Titre carte", etape.Libelle);
    }

    [Fact]
    public async Task RepondreAsync_ExigeUnNiveauParEtape_PasParCarte()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var (testPositionnementService, cohorteId, gestionnaire, apprenant, etapeId) = await PreparerAsync(dbContext, "Objectif clair");

        await testPositionnementService.EnvoyerAsync(cohorteId, TypeTestPositionnement.Amont, gestionnaire.Id, _ => "https://test.local/test");

        var (success, errorMessage) = await testPositionnementService.RepondreAsync(cohorteId, TypeTestPositionnement.Amont, apprenant.Id, new Dictionary<int, int> { [etapeId] = 7 });

        Assert.True(success, errorMessage);
        Assert.True(await testPositionnementService.ADejaReponduAsync(cohorteId, TypeTestPositionnement.Amont, apprenant.Id));
    }

    [Fact]
    public async Task GetStatsAsync_RegroupeParEtapeAvecSonLibelle()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var (testPositionnementService, cohorteId, gestionnaire, apprenant, etapeId) = await PreparerAsync(dbContext, "Objectif mesurable");

        await testPositionnementService.EnvoyerAsync(cohorteId, TypeTestPositionnement.Amont, gestionnaire.Id, _ => "https://test.local/test");
        await testPositionnementService.RepondreAsync(cohorteId, TypeTestPositionnement.Amont, apprenant.Id, new Dictionary<int, int> { [etapeId] = 4 });

        var stats = await testPositionnementService.GetStatsAsync(cohorteId, TypeTestPositionnement.Amont);

        Assert.True(stats.Envoye);
        Assert.Equal(1, stats.NombreRepondants);
        var etapeStat = Assert.Single(stats.ParEtape);
        Assert.Equal("Objectif mesurable", etapeStat.Libelle);
        Assert.Equal(4, etapeStat.NiveauMoyen);
    }
}

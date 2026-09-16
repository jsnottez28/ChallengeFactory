using Application.Common.Interfaces;
using Domain.Entities;
using Integration.TestSupport;
using Web.Data;
using Web.Services;

namespace Integration.Services;

public class VisioServiceTests
{
    private static async Task<(VisioService VisioService, int CohorteId, ApplicationUser Gestionnaire)> PreparerCohorteActiveAsync(ApplicationDbContext dbContext)
    {
        var userManager = TestUserManagerFactory.Create(dbContext);
        var emailService = new FakeEmailService();
        var cohorteService = new CohorteService(dbContext, userManager, emailService, new PreuveService(dbContext, userManager, new FakePreuveFichierStockageService(), new NotificationService(dbContext), new FakeEmailService()), new NotificationService(dbContext));
        var visioService = new VisioService(dbContext, emailService);
        var challengeService = new ChallengeService(dbContext);

        var (_, _, challenge) = await challengeService.CreateAsync(new ChallengeInput
        {
            Titre = "Challenge Test",
            NombreEtapes = 1,
            Mode = ModePlateforme.BtoC,
        });
        await challengeService.CreerEtapeAsync(challenge!.Id, new ChallengeEtapeInput { TitreEtape = "Étape 1" });
        await challengeService.PublierAsync(challenge.Id);

        var gestionnaire = new ApplicationUser { UserName = "coach@test.local", Email = "coach@test.local" };
        dbContext.Users.Add(gestionnaire);
        await dbContext.SaveChangesAsync();

        var (_, _, cohorteId) = await cohorteService.CreateAsync(new CohorteInput { ChallengeId = challenge.Id, Nom = "Cohorte Test" });
        await cohorteService.LancerAsync(cohorteId!.Value, gestionnaire.Id, "https://test.local/parcours", "https://test.local/mi-parcours");

        return (visioService, cohorteId.Value, gestionnaire);
    }

    [Fact]
    public async Task PlanifierAsync_Echoue_SiAucuneDateFournie()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var (visioService, cohorteId, gestionnaire) = await PreparerCohorteActiveAsync(dbContext);

        var (success, errorMessage) = await visioService.PlanifierAsync(cohorteId, gestionnaire.Id, dateVisio: null, "https://meet.test.local/seance");

        Assert.False(success);
        Assert.Contains("obligatoires", errorMessage);
    }

    [Fact]
    public async Task PlanifierAsync_Echoue_SiDateTropAncienne()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var (visioService, cohorteId, gestionnaire) = await PreparerCohorteActiveAsync(dbContext);

        var dateTropAncienne = DateTime.UtcNow.AddDays(-5);
        var (success, errorMessage) = await visioService.PlanifierAsync(cohorteId, gestionnaire.Id, dateTropAncienne, "https://meet.test.local/seance");

        Assert.False(success);
        Assert.Contains("passé", errorMessage);
    }

    [Fact]
    public async Task PlanifierAsync_Echoue_SiDateTropEloigneeDansLeFutur()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var (visioService, cohorteId, gestionnaire) = await PreparerCohorteActiveAsync(dbContext);

        var dateAberrante = DateTime.UtcNow.AddYears(5);
        var (success, errorMessage) = await visioService.PlanifierAsync(cohorteId, gestionnaire.Id, dateAberrante, "https://meet.test.local/seance");

        Assert.False(success);
        Assert.Contains("éloignée", errorMessage);
    }

    [Fact]
    public async Task PlanifierAsync_Reussit_AvecUneDateValide()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var (visioService, cohorteId, gestionnaire) = await PreparerCohorteActiveAsync(dbContext);

        var dateValide = DateTime.UtcNow.AddDays(7);
        var (success, errorMessage) = await visioService.PlanifierAsync(cohorteId, gestionnaire.Id, dateValide, "https://meet.test.local/seance");

        Assert.True(success, errorMessage);
    }
}

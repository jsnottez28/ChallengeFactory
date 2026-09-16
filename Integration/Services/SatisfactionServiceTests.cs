using Application.Common.Interfaces;
using Domain.Entities;
using Integration.TestSupport;
using Microsoft.EntityFrameworkCore;
using Web.Data;
using Web.Services;

namespace Integration.Services;

public class SatisfactionServiceTests
{
    private static async Task<(int CohorteId, ApplicationUser Apprenant)> PreparerCohorteActiveAsync(ApplicationDbContext dbContext)
    {
        var challengeService = new ChallengeService(dbContext);
        var userManager = TestUserManagerFactory.Create(dbContext);
        var cohorteService = new CohorteService(dbContext, userManager, new FakeEmailService(), new PreuveService(dbContext, userManager, new FakePreuveFichierStockageService(), new NotificationService(dbContext), new FakeEmailService()), new NotificationService(dbContext));

        var (_, _, challenge) = await challengeService.CreateAsync(new ChallengeInput { Titre = "Challenge Test", NombreEtapes = 1, Mode = ModePlateforme.BtoC });
        await challengeService.CreerEtapeAsync(challenge!.Id, new ChallengeEtapeInput { TitreEtape = "Étape 1" });
        await challengeService.PublierAsync(challenge.Id);

        var gestionnaire = new ApplicationUser { UserName = "coach@test.local", Email = "coach@test.local" };
        var apprenant = new ApplicationUser { UserName = "apprenant@test.local", Email = "apprenant@test.local" };
        dbContext.Users.AddRange(gestionnaire, apprenant);
        await dbContext.SaveChangesAsync();

        var (_, _, cohorteId) = await cohorteService.CreateAsync(new CohorteInput { ChallengeId = challenge.Id, Nom = "Cohorte Test" });
        await cohorteService.AjouterMembreManuelAsync(cohorteId!.Value, apprenant.Id);
        await cohorteService.LancerAsync(cohorteId.Value, gestionnaire.Id, "https://test.local/parcours", "https://test.local/mi-parcours");

        return (cohorteId.Value, apprenant);
    }

    [Fact]
    public async Task EnregistrerReponseAsync_EnregistreLes4NotesEtLeCommentaire()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var satisfactionService = new SatisfactionService(dbContext);
        var (cohorteId, apprenant) = await PreparerCohorteActiveAsync(dbContext);

        var (success, errorMessage) = await satisfactionService.EnregistrerReponseAsync(
            cohorteId, apprenant.Id, score: 9, noteContenus: 8, noteAccompagnement: 10, noteAdequationAttentes: 7, "Très bon parcours.");

        Assert.True(success, errorMessage);

        var reponse = await dbContext.SatisfactionReponses.SingleAsync();
        Assert.Equal(9, reponse.Score);
        Assert.Equal(8, reponse.NoteContenus);
        Assert.Equal(10, reponse.NoteAccompagnement);
        Assert.Equal(7, reponse.NoteAdequationAttentes);
        Assert.Equal("Très bon parcours.", reponse.Commentaire);
    }

    [Theory]
    [InlineData(-1, 5, 5, 5)]
    [InlineData(11, 5, 5, 5)]
    [InlineData(5, -1, 5, 5)]
    [InlineData(5, 5, 11, 5)]
    [InlineData(5, 5, 5, -1)]
    public async Task EnregistrerReponseAsync_Echoue_SiUneNoteEstHorsPlage(int score, int noteContenus, int noteAccompagnement, int noteAdequationAttentes)
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var satisfactionService = new SatisfactionService(dbContext);
        var (cohorteId, apprenant) = await PreparerCohorteActiveAsync(dbContext);

        var (success, errorMessage) = await satisfactionService.EnregistrerReponseAsync(
            cohorteId, apprenant.Id, score, noteContenus, noteAccompagnement, noteAdequationAttentes, null);

        Assert.False(success);
        Assert.NotNull(errorMessage);
        Assert.False(await dbContext.SatisfactionReponses.AnyAsync());
    }

    [Fact]
    public async Task GetStatsAsync_AgregeUneMoyenneParDimension()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var satisfactionService = new SatisfactionService(dbContext);
        var (cohorteId, premierMembre) = await PreparerCohorteActiveAsync(dbContext);

        var secondMembre = new ApplicationUser { UserName = "second@test.local", Email = "second@test.local" };
        dbContext.Users.Add(secondMembre);
        await dbContext.SaveChangesAsync();

        var userManager = TestUserManagerFactory.Create(dbContext);
        var cohorteService = new CohorteService(dbContext, userManager, new FakeEmailService(), new PreuveService(dbContext, userManager, new FakePreuveFichierStockageService(), new NotificationService(dbContext), new FakeEmailService()), new NotificationService(dbContext));
        await cohorteService.AjouterMembreManuelAsync(cohorteId, secondMembre.Id);

        await satisfactionService.EnregistrerReponseAsync(cohorteId, premierMembre.Id, 10, 10, 10, 10, null);
        await satisfactionService.EnregistrerReponseAsync(cohorteId, secondMembre.Id, 4, 6, 8, 2, null);

        var stats = await satisfactionService.GetStatsAsync(cohorteId);

        Assert.Equal(2, stats.NombreReponses);
        Assert.Equal(7d, stats.ScoreMoyen);
        Assert.Equal(8d, stats.NoteContenusMoyenne);
        Assert.Equal(9d, stats.NoteAccompagnementMoyenne);
        Assert.Equal(6d, stats.NoteAdequationAttentesMoyenne);
    }
}

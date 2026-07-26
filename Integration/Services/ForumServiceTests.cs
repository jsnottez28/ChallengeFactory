using Application.Common;
using Application.Common.Interfaces;
using Domain.Entities;
using Integration.TestSupport;
using Microsoft.EntityFrameworkCore;
using Web.Data;
using Web.Services;

namespace Integration.Services;

public class ForumServiceTests
{
    private static async Task<(int CohorteId, int ChallengeEtapeId, List<ApplicationUser> Membres)> CreerCohorteActiveAsync(
        ApplicationDbContext dbContext, ICohorteService cohorteService, int nombreMembres = 2)
    {
        var challengeService = new ChallengeService(dbContext);
        var (_, _, challenge) = await challengeService.CreateAsync(new ChallengeInput { Titre = "Challenge Forum", NombreEtapes = 1, Mode = ModePlateforme.BtoC });
        var (_, _, etape) = await challengeService.CreerEtapeAsync(challenge!.Id, new ChallengeEtapeInput { TitreEtape = "Étape 1", DefiIndividuel = "Défi" });
        await challengeService.PublierAsync(challenge.Id);

        var gestionnaire = new ApplicationUser { UserName = "coach@test.local", Email = "coach@test.local" };
        dbContext.Users.Add(gestionnaire);

        var membres = new List<ApplicationUser>();
        for (var i = 0; i < nombreMembres; i++)
        {
            var membre = new ApplicationUser { UserName = $"membre{i}@test.local", Email = $"membre{i}@test.local", Statut = StatutUtilisateur.Actif };
            dbContext.Users.Add(membre);
            membres.Add(membre);
        }
        await dbContext.SaveChangesAsync();

        var (_, _, cohorteId) = await cohorteService.CreateAsync(new CohorteInput { ChallengeId = challenge.Id, Nom = "Cohorte Forum" });
        foreach (var membre in membres)
        {
            await cohorteService.AjouterMembreManuelAsync(cohorteId!.Value, membre.Id);
        }
        await cohorteService.LancerAsync(cohorteId!.Value, gestionnaire.Id, "https://test.local/parcours");

        return (cohorteId.Value, etape!.Id, membres);
    }

    private static (ApplicationDbContext DbContext, ICohorteService CohorteService, IForumService ForumService) CreerServices()
    {
        var dbContext = InMemoryDbContextFactory.Create();
        var userManager = TestUserManagerFactory.Create(dbContext);
        var preuveService = new PreuveService(dbContext, userManager, new FakePreuveFichierStockageService(), new NotificationService(dbContext), new FakeEmailService());
        var cohorteService = new CohorteService(dbContext, userManager, new FakeEmailService(), preuveService);
        var forumService = new ForumService(dbContext, new NotificationService(dbContext));
        return (dbContext, cohorteService, forumService);
    }

    [Fact]
    public async Task PosterMessageAsync_PublieUnMessageEtUneReponseEnFil()
    {
        var (dbContext, cohorteService, forumService) = CreerServices();
        await using var _ = dbContext;

        var (cohorteId, etapeId, membres) = await CreerCohorteActiveAsync(dbContext, cohorteService);
        var auteur = membres[0];
        var repondant = membres[1];

        var (success, errorMessage) = await forumService.PosterMessageAsync(auteur.Id, cohorteId, etapeId, "Message racine", null, "https://test.local/forum");
        Assert.True(success, errorMessage);

        var racine = await dbContext.ForumMessages.SingleAsync(m => m.AuteurId == auteur.Id);
        await forumService.PosterMessageAsync(repondant.Id, cohorteId, etapeId, "Une réponse", racine.Id, "https://test.local/forum");

        var messages = await forumService.GetMessagesEtapeAsync(etapeId, cohorteId, auteur.Id);
        var messageRacine = Assert.Single(messages);
        var reponse = Assert.Single(messageRacine.Reponses);
        Assert.Equal("Une réponse", reponse.Contenu);
    }

    [Fact]
    public async Task MarquerUtileAsync_Echoue_SiAuteurMarqueSonProprePropreMessage()
    {
        var (dbContext, cohorteService, forumService) = CreerServices();
        await using var _ = dbContext;

        var (cohorteId, etapeId, membres) = await CreerCohorteActiveAsync(dbContext, cohorteService);
        var auteur = membres[0];

        await forumService.PosterMessageAsync(auteur.Id, cohorteId, etapeId, "Mon message", null, "https://test.local/forum");
        var message = await dbContext.ForumMessages.SingleAsync();

        var (success, errorMessage) = await forumService.MarquerUtileAsync(message.Id, auteur.Id, "https://test.local/forum");

        Assert.False(success);
        Assert.NotNull(errorMessage);
    }

    [Fact]
    public async Task MarquerUtileAsync_UniciteParMessageEtUtilisateur_EtGenerePointsKarmaUneSeuleFois()
    {
        var (dbContext, cohorteService, forumService) = CreerServices();
        await using var _ = dbContext;

        var (cohorteId, etapeId, membres) = await CreerCohorteActiveAsync(dbContext, cohorteService);
        var auteur = membres[0];
        var marqueur = membres[1];

        await forumService.PosterMessageAsync(auteur.Id, cohorteId, etapeId, "Message utile", null, "https://test.local/forum");
        var message = await dbContext.ForumMessages.SingleAsync();

        var (premierSuccess, _) = await forumService.MarquerUtileAsync(message.Id, marqueur.Id, "https://test.local/forum");
        Assert.True(premierSuccess);

        var (deuxiemeSuccess, deuxiemeErreur) = await forumService.MarquerUtileAsync(message.Id, marqueur.Id, "https://test.local/forum");
        Assert.False(deuxiemeSuccess);
        Assert.NotNull(deuxiemeErreur);

        var nombreMarquages = await dbContext.ForumMessagesUtiles.CountAsync(u => u.MessageId == message.Id && u.MarqueParId == marqueur.Id);
        Assert.Equal(1, nombreMarquages);

        var pointsAuteur = await dbContext.PointsEvenements
            .Where(e => e.UtilisateurId == auteur.Id && e.TypePoints == TypePoints.PointsKarma)
            .SumAsync(e => e.Montant);
        Assert.Equal(PointsConfig.PointsKarmaMessageUtile, pointsAuteur);
    }

    [Fact]
    public async Task PosterMessageAsync_Echoue_SurUneEtapeQuiNestPlusLEtapeCourante()
    {
        var (dbContext, cohorteService, forumService) = CreerServices();
        await using var _ = dbContext;

        var (cohorteId, etapeId, membres) = await CreerCohorteActiveAsync(dbContext, cohorteService, nombreMembres: 1);
        var membre = membres[0];

        var gestionnaire = await dbContext.Users.FirstAsync(u => u.Email == "coach@test.local");
        await cohorteService.ValiderEtapeAsync(cohorteId, gestionnaire.Id, "https://test.local/parcours", "https://test.local/bibliotheque");

        var (success, errorMessage) = await forumService.PosterMessageAsync(membre.Id, cohorteId, etapeId, "Trop tard", null, "https://test.local/forum");

        Assert.False(success);
        Assert.NotNull(errorMessage);
    }

    [Fact]
    public async Task GetMessagesEtapeAsync_Echoue_PourUnNonMembre_SaufEnModeAdmin()
    {
        var (dbContext, cohorteService, forumService) = CreerServices();
        await using var _ = dbContext;

        var (cohorteId, etapeId, membres) = await CreerCohorteActiveAsync(dbContext, cohorteService, nombreMembres: 1);
        await forumService.PosterMessageAsync(membres[0].Id, cohorteId, etapeId, "Message", null, "https://test.local/forum");

        var nonMembre = new ApplicationUser { UserName = "exterieur@test.local", Email = "exterieur@test.local" };
        dbContext.Users.Add(nonMembre);
        await dbContext.SaveChangesAsync();

        var messagesNonMembre = await forumService.GetMessagesEtapeAsync(etapeId, cohorteId, nonMembre.Id);
        Assert.Empty(messagesNonMembre);

        var messagesAdmin = await forumService.GetMessagesEtapeAsync(etapeId, cohorteId, nonMembre.Id, estAdmin: true);
        Assert.Single(messagesAdmin);
    }

    [Fact]
    public async Task SupprimerMessageAsync_SupprimeAussiLesReponsesEnFil()
    {
        var (dbContext, cohorteService, forumService) = CreerServices();
        await using var _ = dbContext;

        var (cohorteId, etapeId, membres) = await CreerCohorteActiveAsync(dbContext, cohorteService);
        var auteur = membres[0];
        var repondant = membres[1];

        await forumService.PosterMessageAsync(auteur.Id, cohorteId, etapeId, "Racine", null, "https://test.local/forum");
        var racine = await dbContext.ForumMessages.SingleAsync(m => m.AuteurId == auteur.Id);
        await forumService.PosterMessageAsync(repondant.Id, cohorteId, etapeId, "Réponse", racine.Id, "https://test.local/forum");

        var (success, errorMessage) = await forumService.SupprimerMessageAsync(racine.Id);
        Assert.True(success, errorMessage);

        var nombreMessagesRestants = await dbContext.ForumMessages.CountAsync(m => m.CohorteId == cohorteId);
        Assert.Equal(0, nombreMessagesRestants);
    }
}

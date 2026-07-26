using Application.Common.Interfaces;
using Domain.Entities;
using Integration.TestSupport;
using Microsoft.EntityFrameworkCore;
using Web.Data;
using Web.Services;

namespace Integration.Services;

// Couvre le flux "Demander un embarquement" (prompt section H) : une premiere demande cree
// une Cohorte Proposee, une seconde demande sur le meme Challenge rejoint la MEME Cohorte
// Proposee (jamais une nouvelle), une Cohorte Proposee ne doit jamais apparaitre dans le
// catalogue public ni permettre de deposer une preuve, et la validation Gestionnaire
// transitionne correctement Proposee -> EnPreparation.
public class EmbarquementTests
{
    private static (ApplicationDbContext DbContext, ICohorteService CohorteService, IPreuveService PreuveService, INotificationService NotificationService, FakeEmailService EmailService) CreerServices()
    {
        var dbContext = InMemoryDbContextFactory.Create();
        var userManager = TestUserManagerFactory.Create(dbContext);
        var notificationService = new NotificationService(dbContext);
        var emailService = new FakeEmailService();
        var preuveService = new PreuveService(dbContext, userManager, new FakePreuveFichierStockageService(), notificationService, new FakeEmailService());
        var cohorteService = new CohorteService(dbContext, userManager, emailService, preuveService, notificationService);
        return (dbContext, cohorteService, preuveService, notificationService, emailService);
    }

    private static async Task<Challenge> CreerChallengeBtoCPublieAsync(ApplicationDbContext dbContext, ModePlateforme mode = ModePlateforme.BtoC)
    {
        var challengeService = new ChallengeService(dbContext);
        var (_, _, challenge) = await challengeService.CreateAsync(new ChallengeInput { Titre = "Challenge Embarquement", NombreEtapes = 1, Mode = mode });
        var (_, _, etape) = await challengeService.CreerEtapeAsync(challenge!.Id, new ChallengeEtapeInput { TitreEtape = "Étape 1", DefiIndividuel = "Défi" });
        await challengeService.PublierAsync(challenge.Id);
        return challenge;
    }

    [Fact]
    public async Task DemanderEmbarquementAsync_PremiereDemande_CreeUneNouvelleCohorteProposee()
    {
        var (dbContext, cohorteService, _, _, _) = CreerServices();
        await using var _ = dbContext;

        var challenge = await CreerChallengeBtoCPublieAsync(dbContext);
        var apprenant = new ApplicationUser { UserName = "apprenant@test.local", Email = "apprenant@test.local" };
        dbContext.Users.Add(apprenant);
        await dbContext.SaveChangesAsync();

        var (success, errorMessage, cohorteId) = await cohorteService.DemanderEmbarquementAsync(challenge.Id, apprenant.Id);

        Assert.True(success, errorMessage);
        Assert.NotNull(cohorteId);

        var cohorte = await dbContext.Cohortes.FindAsync(cohorteId!.Value);
        Assert.NotNull(cohorte);
        Assert.Equal(StatutCohorte.Proposee, cohorte!.Statut);
        Assert.True(await dbContext.CohorteMembres.AnyAsync(m => m.CohorteId == cohorteId.Value && m.UtilisateurId == apprenant.Id));
    }

    [Fact]
    public async Task DemanderEmbarquementAsync_SecondeDemandeSurLeMemeChallenge_RejointLaMemeCohorteProposee()
    {
        var (dbContext, cohorteService, _, _, _) = CreerServices();
        await using var _ = dbContext;

        var challenge = await CreerChallengeBtoCPublieAsync(dbContext);
        var premier = new ApplicationUser { UserName = "premier@test.local", Email = "premier@test.local" };
        var second = new ApplicationUser { UserName = "second@test.local", Email = "second@test.local" };
        dbContext.Users.AddRange(premier, second);
        await dbContext.SaveChangesAsync();

        var (_, _, cohorteId1) = await cohorteService.DemanderEmbarquementAsync(challenge.Id, premier.Id);
        var (success2, errorMessage2, cohorteId2) = await cohorteService.DemanderEmbarquementAsync(challenge.Id, second.Id);

        Assert.True(success2, errorMessage2);
        Assert.Equal(cohorteId1, cohorteId2);

        // Une seule Cohorte Proposee pour ce Challenge, avec les 2 demandeurs dedans.
        var cohortesProposees = await dbContext.Cohortes.Where(c => c.ChallengeId == challenge.Id && c.Statut == StatutCohorte.Proposee).ToListAsync();
        var cohorteProposee = Assert.Single(cohortesProposees);
        var membres = await dbContext.CohorteMembres.Where(m => m.CohorteId == cohorteProposee.Id).ToListAsync();
        Assert.Equal(2, membres.Count);
    }

    [Fact]
    public async Task CohorteProposee_NApparaitJamaisDansLeCatalogueEtNePermetPasDeDeposerUnePreuve()
    {
        var (dbContext, cohorteService, preuveService, _, _) = CreerServices();
        await using var _ = dbContext;

        var challenge = await CreerChallengeBtoCPublieAsync(dbContext);
        var apprenant = new ApplicationUser { UserName = "apprenant@test.local", Email = "apprenant@test.local", Statut = StatutUtilisateur.Actif };
        dbContext.Users.Add(apprenant);
        await dbContext.SaveChangesAsync();

        var (_, _, cohorteId) = await cohorteService.DemanderEmbarquementAsync(challenge.Id, apprenant.Id);

        // Invisible du catalogue (GetAllAsync exclut Proposee - cf. HomeController.Formations).
        var toutesLesCohortes = await cohorteService.GetAllAsync();
        Assert.DoesNotContain(toutesLesCohortes, c => c.Id == cohorteId!.Value);

        // Aucun depot de preuve possible : la Cohorte n'est pas Active.
        var etapeId = await dbContext.ChallengeEtapes.Where(e => e.ChallengeId == challenge.Id).Select(e => e.Id).FirstAsync();
        var (success, errorMessage, _) = await preuveService.DeposerOuModifierAsync(apprenant.Id, cohorteId!.Value, etapeId, "Description", [], null);
        Assert.False(success);
        Assert.NotNull(errorMessage);
    }

    [Fact]
    public async Task ValiderEmbarquementAsync_TransitionneProposeeVersEnPreparation_EtDevientVisibleDansLeCatalogue()
    {
        var (dbContext, cohorteService, _, notificationService, emailService) = CreerServices();
        await using var _ = dbContext;

        var challenge = await CreerChallengeBtoCPublieAsync(dbContext);
        var premier = new ApplicationUser { UserName = "premier@test.local", Email = "premier@test.local" };
        var second = new ApplicationUser { UserName = "second@test.local", Email = "second@test.local" };
        dbContext.Users.AddRange(premier, second);
        await dbContext.SaveChangesAsync();

        var (_, _, cohorteId) = await cohorteService.DemanderEmbarquementAsync(challenge.Id, premier.Id);
        await cohorteService.DemanderEmbarquementAsync(challenge.Id, second.Id);

        var dateLancement = DateTime.UtcNow.Date.AddDays(14);
        var (success, errorMessage) = await cohorteService.ValiderEmbarquementAsync(
            cohorteId!.Value, "Session validée", dateLancement, "https://test.local/formations");

        Assert.True(success, errorMessage);

        var cohorte = await dbContext.Cohortes.FindAsync(cohorteId.Value);
        Assert.Equal(StatutCohorte.EnPreparation, cohorte!.Statut);
        Assert.Equal("Session validée", cohorte.Nom);
        Assert.Equal(dateLancement, cohorte.DateLancement);

        var toutesLesCohortes = await cohorteService.GetAllAsync();
        Assert.Contains(toutesLesCohortes, c => c.Id == cohorteId.Value && c.Statut == StatutCohorte.EnPreparation);

        // Chaque demandeur (deja membre depuis DemanderEmbarquementAsync) recoit un email
        // ET une notification in-app annoncant la confirmation de sa session.
        Assert.Equal(2, emailService.Envois.Count);
        Assert.Contains(emailService.Envois, e => e.Destinataire == premier.Email && e.Sujet.Contains("confirmée"));
        Assert.Contains(emailService.Envois, e => e.Destinataire == second.Email && e.Sujet.Contains("confirmée"));

        var notifsPremier = await notificationService.GetMesNotificationsAsync(premier.Id);
        Assert.Contains(notifsPremier, n => n.Type == TypeNotification.DemandeEmbarquementValidee);
        var notifsSecond = await notificationService.GetMesNotificationsAsync(second.Id);
        Assert.Contains(notifsSecond, n => n.Type == TypeNotification.DemandeEmbarquementValidee);
    }

    [Fact]
    public async Task RefuserEmbarquementAsync_SupprimeLaCohorteEtNotifieChaqueDemandeur()
    {
        var (dbContext, cohorteService, _, notificationService, _) = CreerServices();
        await using var _ = dbContext;

        var challenge = await CreerChallengeBtoCPublieAsync(dbContext);
        var premier = new ApplicationUser { UserName = "premier@test.local", Email = "premier@test.local" };
        var second = new ApplicationUser { UserName = "second@test.local", Email = "second@test.local" };
        dbContext.Users.AddRange(premier, second);
        await dbContext.SaveChangesAsync();

        var (_, _, cohorteId) = await cohorteService.DemanderEmbarquementAsync(challenge.Id, premier.Id);
        await cohorteService.DemanderEmbarquementAsync(challenge.Id, second.Id);

        var (success, errorMessage) = await cohorteService.RefuserEmbarquementAsync(cohorteId!.Value, "https://test.local/formations");

        Assert.True(success, errorMessage);
        Assert.Null(await dbContext.Cohortes.FindAsync(cohorteId.Value));

        var notifsPremier = await notificationService.GetMesNotificationsAsync(premier.Id);
        Assert.Contains(notifsPremier, n => n.Type == TypeNotification.DemandeEmbarquementRefusee);
        var notifsSecond = await notificationService.GetMesNotificationsAsync(second.Id);
        Assert.Contains(notifsSecond, n => n.Type == TypeNotification.DemandeEmbarquementRefusee);
    }
}

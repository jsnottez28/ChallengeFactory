using Application.Common.Interfaces;
using Domain.Entities;
using Integration.TestSupport;
using Web.Data;
using Web.Services;

namespace Integration.Services;

// Couvre les 5 declencheurs de notifications in-app (prompt section B) et l'email
// "preuve validee par les pairs" envoye une seule fois par franchissement de seuil
// (prompt section C) - tests minimums explicitement demandes dans les contraintes
// transverses du prompt.
public class NotificationTriggersTests
{
    private static async Task<(ApplicationDbContext DbContext, IPreuveService PreuveService, IForumService ForumService, ICohorteService CohorteService, INotificationService NotificationService, FakeEmailService EmailService, int CohorteId, int ChallengeEtapeId, List<ApplicationUser> Membres, string GestionnaireId)> CreerContexteAsync(int nombreMembres = 3)
    {
        var dbContext = InMemoryDbContextFactory.Create();
        var userManager = TestUserManagerFactory.Create(dbContext);
        var notificationService = new NotificationService(dbContext);
        var emailService = new FakeEmailService();
        var preuveService = new PreuveService(dbContext, userManager, new FakePreuveFichierStockageService(), notificationService, emailService);
        var forumService = new ForumService(dbContext, notificationService);
        var cohorteService = new CohorteService(dbContext, userManager, new FakeEmailService(), preuveService, new NotificationService(dbContext));

        var challengeService = new ChallengeService(dbContext);
        var (_, _, challenge) = await challengeService.CreateAsync(new ChallengeInput { Titre = "Challenge Notifications", NombreEtapes = 1, Mode = ModePlateforme.BtoC });
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

        var (_, _, cohorteId) = await cohorteService.CreateAsync(new CohorteInput { ChallengeId = challenge.Id, Nom = "Cohorte Notifications" });
        foreach (var membre in membres)
        {
            await cohorteService.AjouterMembreManuelAsync(cohorteId!.Value, membre.Id);
        }
        await cohorteService.LancerAsync(cohorteId!.Value, gestionnaire.Id, "https://test.local/parcours", DateTime.UtcNow.AddDays(1), "https://test.local/visio", null);

        return (dbContext, preuveService, forumService, cohorteService, notificationService, emailService, cohorteId.Value, etape!.Id, membres, gestionnaire.Id);
    }

    [Fact]
    public async Task PosterMessageAsync_NouveauMessageRacine_NotifieTousLesMembresSaufLAuteur()
    {
        var ctx = await CreerContexteAsync(nombreMembres: 3);
        await using var _ = ctx.DbContext;
        var auteur = ctx.Membres[0];

        await ctx.ForumService.PosterMessageAsync(auteur.Id, ctx.CohorteId, ctx.ChallengeEtapeId, "Message racine", null, "https://test.local/forum");

        var notifsAuteur = await ctx.NotificationService.GetMesNotificationsAsync(auteur.Id);
        Assert.Empty(notifsAuteur);

        foreach (var autre in ctx.Membres.Skip(1))
        {
            var notifs = await ctx.NotificationService.GetMesNotificationsAsync(autre.Id);
            var notif = Assert.Single(notifs);
            Assert.Equal(TypeNotification.NouveauMessageForum, notif.Type);
            Assert.Equal("https://test.local/forum", notif.Lien);
        }
    }

    [Fact]
    public async Task PosterMessageAsync_Reponse_NotifieUniquementLAuteurDuMessageParent()
    {
        var ctx = await CreerContexteAsync(nombreMembres: 3);
        await using var _ = ctx.DbContext;
        var auteurRacine = ctx.Membres[0];
        var repondant = ctx.Membres[1];
        var temoin = ctx.Membres[2];

        await ctx.ForumService.PosterMessageAsync(auteurRacine.Id, ctx.CohorteId, ctx.ChallengeEtapeId, "Racine", null, "https://test.local/forum");
        var racine = ctx.DbContext.ForumMessages.Single();

        // Le message racine a deja notifie repondant et temoin (NouveauMessageForum) : on
        // capture leur nombre de notifications AVANT la reponse pour isoler, par delta,
        // l'effet propre de la reponse (et non repartir d'un etat vide).
        var nombreAvantRepondant = (await ctx.NotificationService.GetMesNotificationsAsync(repondant.Id)).Count;
        var nombreAvantTemoin = (await ctx.NotificationService.GetMesNotificationsAsync(temoin.Id)).Count;

        await ctx.ForumService.PosterMessageAsync(repondant.Id, ctx.CohorteId, ctx.ChallengeEtapeId, "Une réponse", racine.Id, "https://test.local/forum");

        // L'auteur du message racine recoit la notification specifique de reponse...
        var notifsAuteurRacine = await ctx.NotificationService.GetMesNotificationsAsync(auteurRacine.Id);
        var notifReponse = Assert.Single(notifsAuteurRacine);
        Assert.Equal(TypeNotification.ReponseAMonMessage, notifReponse.Type);

        // ...mais un simple temoin (autre membre de la cohorte, ni auteur ni repondant) ne
        // recoit RIEN de plus pour cette reponse : le declencheur generique "nouveau message
        // forum" est volontairement desactive pour les reponses (cf. resume de livraison).
        var notifsTemoinApres = await ctx.NotificationService.GetMesNotificationsAsync(temoin.Id);
        Assert.Equal(nombreAvantTemoin, notifsTemoinApres.Count);

        // Et le repondant ne se notifie pas lui-meme.
        var notifsRepondantApres = await ctx.NotificationService.GetMesNotificationsAsync(repondant.Id);
        Assert.Equal(nombreAvantRepondant, notifsRepondantApres.Count);
    }

    [Fact]
    public async Task MarquerUtileAsync_NotifieLAuteurDuMessage_PasLeMarqueur()
    {
        var ctx = await CreerContexteAsync(nombreMembres: 2);
        await using var _ = ctx.DbContext;
        var auteur = ctx.Membres[0];
        var marqueur = ctx.Membres[1];

        await ctx.ForumService.PosterMessageAsync(auteur.Id, ctx.CohorteId, ctx.ChallengeEtapeId, "Message utile", null, "https://test.local/forum");
        var message = ctx.DbContext.ForumMessages.Single();

        // Le message racine a deja notifie le marqueur (NouveauMessageForum, il fait partie
        // de la cohorte) : on capture son nombre de notifications AVANT le marquage utile
        // pour isoler par delta l'effet propre du marquage.
        var nombreAvantMarqueur = (await ctx.NotificationService.GetMesNotificationsAsync(marqueur.Id)).Count;

        await ctx.ForumService.MarquerUtileAsync(message.Id, marqueur.Id, "https://test.local/forum");

        var notifsAuteur = await ctx.NotificationService.GetMesNotificationsAsync(auteur.Id);
        Assert.Contains(notifsAuteur, n => n.Type == TypeNotification.MessageMarqueUtile);

        var notifsMarqueurApres = await ctx.NotificationService.GetMesNotificationsAsync(marqueur.Id);
        Assert.Equal(nombreAvantMarqueur, notifsMarqueurApres.Count);
    }

    [Theory]
    [InlineData(DecisionValidationPair.Valide)]
    [InlineData(DecisionValidationPair.ARevoir)]
    public async Task ValiderParPairAsync_NotifieLAuteurDeLaPreuve_QuelleQueSoitLaDecision(DecisionValidationPair decision)
    {
        var ctx = await CreerContexteAsync(nombreMembres: 3);
        await using var _ = ctx.DbContext;
        var auteur = ctx.Membres[0];
        var pair = ctx.Membres[1];

        var (_, _, preuveId) = await ctx.PreuveService.DeposerOuModifierAsync(auteur.Id, ctx.CohorteId, ctx.ChallengeEtapeId, "Description", [], null);

        // Un commentaire est obligatoire pour "À revoir" (regle metier de ValiderParPairAsync,
        // sans lien avec ce qui est teste ici).
        var commentaire = decision == DecisionValidationPair.ARevoir ? "À préciser" : null;
        var (success, errorMessage) = await ctx.PreuveService.ValiderParPairAsync(preuveId!.Value, pair.Id, decision, commentaire, "https://test.local/suivi-preuve");
        Assert.True(success, errorMessage);

        var notifsAuteur = await ctx.NotificationService.GetMesNotificationsAsync(auteur.Id);
        Assert.Contains(notifsAuteur, n => n.Type == TypeNotification.DecisionSurMaPreuve);

        // Le pair qui valide ne se notifie pas lui-meme.
        var notifsPair = await ctx.NotificationService.GetMesNotificationsAsync(pair.Id);
        Assert.Empty(notifsPair);
    }

    [Fact]
    public async Task ValiderParPairAsync_PreuveValideeParLesPairs_NotifieEtEnvoieLEmailUneSeuleFoisParFranchissementDeSeuil()
    {
        var ctx = await CreerContexteAsync(nombreMembres: 3);
        await using var _ = ctx.DbContext;
        var auteur = ctx.Membres[0];
        var pair1 = ctx.Membres[1];
        var pair2 = ctx.Membres[2];

        var (_, _, preuveId) = await ctx.PreuveService.DeposerOuModifierAsync(auteur.Id, ctx.CohorteId, ctx.ChallengeEtapeId, "Description", [], null);

        // pair1 valide : ratio 1/1 = 100% >= seuil (50%) -> franchissement du seuil,
        // premiere fois -> notification + email attendus.
        await ctx.PreuveService.ValiderParPairAsync(preuveId!.Value, pair1.Id, DecisionValidationPair.Valide, null, "https://test.local/suivi-preuve");

        var notifsApresPremierVote = await ctx.NotificationService.GetMesNotificationsAsync(auteur.Id);
        Assert.Single(notifsApresPremierVote, n => n.Type == TypeNotification.PreuveValideeParLesPairs);
        Assert.Single(ctx.EmailService.Envois, e => e.Sujet.Contains("validée par tes pairs"));

        // pair2 valide aussi : ratio reste 2/2 = 100%, TOUJOURS ValideeParLesPairs -> simple
        // recalcul au meme statut, pas un nouveau franchissement -> ni notification ni email
        // supplementaires (c'est exactement le point demande : "pas a chaque recalcul").
        await ctx.PreuveService.ValiderParPairAsync(preuveId.Value, pair2.Id, DecisionValidationPair.Valide, null, "https://test.local/suivi-preuve");

        var notifsApresSecondVote = await ctx.NotificationService.GetMesNotificationsAsync(auteur.Id);
        Assert.Single(notifsApresSecondVote, n => n.Type == TypeNotification.PreuveValideeParLesPairs);
        Assert.Single(ctx.EmailService.Envois, e => e.Sujet.Contains("validée par tes pairs"));
        Assert.Equal(auteur.Email, ctx.EmailService.Envois.Single(e => e.Sujet.Contains("validée par tes pairs")).Destinataire);
    }
}

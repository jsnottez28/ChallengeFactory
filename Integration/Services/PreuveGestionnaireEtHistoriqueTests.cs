using Application.Common.Interfaces;
using Domain.Entities;
using Integration.TestSupport;
using Web.Data;
using Web.Services;

namespace Integration.Services;

// Couvre les corrections A.1 (le Gestionnaire doit pouvoir consulter/telecharger les
// fichiers d'une Preuve de sa cohorte, pas seulement voir un compteur agrege) et A.2
// (l'ecran "Suivi de ma preuve" doit afficher TOUT le fil de retours, pairs et
// Gestionnaire, meme depuis une requete/DbContext totalement separee du depot initial -
// cf. prompt "Corrections de bugs").
public class PreuveGestionnaireEtHistoriqueTests
{
    private static async Task<(int CohorteId, int ChallengeEtapeId, List<ApplicationUser> Membres, string GestionnaireId)> CreerCohorteActiveAsync(
        ApplicationDbContext dbContext, ICohorteService cohorteService, int nombreMembres = 3)
    {
        var challengeService = new ChallengeService(dbContext);
        var (_, _, challenge) = await challengeService.CreateAsync(new ChallengeInput { Titre = "Challenge Historique", NombreEtapes = 1, Mode = ModePlateforme.BtoC });
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

        var (_, _, cohorteId) = await cohorteService.CreateAsync(new CohorteInput { ChallengeId = challenge.Id, Nom = "Cohorte Historique" });
        foreach (var membre in membres)
        {
            await cohorteService.AjouterMembreManuelAsync(cohorteId!.Value, membre.Id);
        }
        await cohorteService.LancerAsync(cohorteId!.Value, gestionnaire.Id, "https://test.local/parcours", DateTime.UtcNow.AddDays(1), "https://test.local/visio", null);

        return (cohorteId.Value, etape!.Id, membres, gestionnaire.Id);
    }

    [Fact]
    public async Task GetDetailPourGestionnaireAsync_RetourneFichiersDescriptionEtHistoriqueComplet_PasSeulementUnCompteur()
    {
        var nomBase = Guid.NewGuid().ToString();
        await using var dbContext = InMemoryDbContextFactory.Create(nomBase);
        var userManager = TestUserManagerFactory.Create(dbContext);
        var preuveService = new PreuveService(dbContext, userManager, new FakePreuveFichierStockageService(), new NotificationService(dbContext), new FakeEmailService());
        var cohorteService = new CohorteService(dbContext, userManager, new FakeEmailService(), preuveService, new NotificationService(dbContext));

        var (cohorteId, etapeId, membres, gestionnaireId) = await CreerCohorteActiveAsync(dbContext, cohorteService);
        var auteur = membres[0];
        var pair = membres[1];

        var fichier = new FichierPreuveInput { NomFichier = "capture.png", Contenu = new MemoryStream([1, 2, 3]), TailleOctets = 3 };
        var (_, _, preuveId) = await preuveService.DeposerOuModifierAsync(auteur.Id, cohorteId, etapeId, "Ma description détaillée", [fichier], null);

        await preuveService.ValiderParPairAsync(preuveId!.Value, pair.Id, DecisionValidationPair.ARevoir, "À préciser", "https://test.local/suivi-preuve");

        // Le Gestionnaire n'est PAS l'auteur : GetDetailPourGestionnaireAsync ne doit pas
        // filtrer par auteur (contrairement a GetMaPreuveAsync), seul l'appelant
        // (controleur, droit PREUVE.CONSULTER) restreint l'acces.
        var detail = await preuveService.GetDetailPourGestionnaireAsync(preuveId.Value);

        Assert.NotNull(detail);
        Assert.Single(detail!.Fichiers);
        Assert.Equal("capture.png", detail.Fichiers[0].NomFichier);
        Assert.Equal("Ma description détaillée", detail.Description);
        var retour = Assert.Single(detail.Retours);
        Assert.False(retour.EstGestionnaire);
        Assert.Equal("À revoir", retour.Decision);
        Assert.Equal("À préciser", retour.Commentaire);
    }

    [Fact]
    public async Task TelechargerFichierAsync_AutoriseUnGestionnaireEnLectureSeule_SansLeDroitValider()
    {
        var nomBase = Guid.NewGuid().ToString();
        await using var dbContext = InMemoryDbContextFactory.Create(nomBase);
        var userManager = TestUserManagerFactory.Create(dbContext);
        var preuveService = new PreuveService(dbContext, userManager, new FakePreuveFichierStockageService(), new NotificationService(dbContext), new FakeEmailService());
        var cohorteService = new CohorteService(dbContext, userManager, new FakeEmailService(), preuveService, new NotificationService(dbContext));

        var (cohorteId, etapeId, membres, _) = await CreerCohorteActiveAsync(dbContext, cohorteService);
        var auteur = membres[0];

        var fichier = new FichierPreuveInput { NomFichier = "rapport.pdf", Contenu = new MemoryStream([9, 9, 9]), TailleOctets = 3 };
        await preuveService.DeposerOuModifierAsync(auteur.Id, cohorteId, etapeId, null, [fichier], null);

        var fichierId = dbContext.PreuveFichiers.Single().Id;

        // Un utilisateur externe (ni auteur, ni membre de la cohorte) avec le droit
        // PREUVE.CONSULTER (pas VALIDER) simule via aLeDroitAdmin=true : doit pouvoir
        // telecharger (avant la correction A.1, seul PREUVE.VALIDER donnait acces).
        var gestionnaireLectureSeule = new ApplicationUser { UserName = "lecture-seule@test.local", Email = "lecture-seule@test.local" };
        dbContext.Users.Add(gestionnaireLectureSeule);
        await dbContext.SaveChangesAsync();

        var resultat = await preuveService.TelechargerFichierAsync(fichierId, gestionnaireLectureSeule.Id, aLeDroitAdmin: true);

        Assert.NotNull(resultat);
        Assert.Equal("rapport.pdf", resultat!.Value.NomFichier);
    }

    [Fact]
    public async Task TelechargerFichierAsync_RefuseUnExterieurSansDroitEtSansLienAvecLaCohorte()
    {
        var nomBase = Guid.NewGuid().ToString();
        await using var dbContext = InMemoryDbContextFactory.Create(nomBase);
        var userManager = TestUserManagerFactory.Create(dbContext);
        var preuveService = new PreuveService(dbContext, userManager, new FakePreuveFichierStockageService(), new NotificationService(dbContext), new FakeEmailService());
        var cohorteService = new CohorteService(dbContext, userManager, new FakeEmailService(), preuveService, new NotificationService(dbContext));

        var (cohorteId, etapeId, membres, _) = await CreerCohorteActiveAsync(dbContext, cohorteService);
        var auteur = membres[0];

        var fichier = new FichierPreuveInput { NomFichier = "prive.pdf", Contenu = new MemoryStream([1]), TailleOctets = 1 };
        await preuveService.DeposerOuModifierAsync(auteur.Id, cohorteId, etapeId, null, [fichier], null);
        var fichierId = dbContext.PreuveFichiers.Single().Id;

        var exterieur = new ApplicationUser { UserName = "exterieur@test.local", Email = "exterieur@test.local" };
        dbContext.Users.Add(exterieur);
        await dbContext.SaveChangesAsync();

        var resultat = await preuveService.TelechargerFichierAsync(fichierId, exterieur.Id, aLeDroitAdmin: false);

        Assert.Null(resultat);
    }

    [Fact]
    public async Task GetMaPreuveAsync_RetourneToutLHistoriqueDesRetours_MemeDepuisUneRequeteSeparee()
    {
        var nomBase = Guid.NewGuid().ToString();

        // "Requete 1" : depot + votes, tout via un premier DbContext (comme un premier
        // ensemble d'appels HTTP).
        int preuveId;
        int cohorteId;
        int etapeId;
        string auteurId, gestionnaireId, pair1Id, pair2Id;
        await using (var dbContext1 = InMemoryDbContextFactory.Create(nomBase))
        {
            var userManager1 = TestUserManagerFactory.Create(dbContext1);
            var preuveService1 = new PreuveService(dbContext1, userManager1, new FakePreuveFichierStockageService(), new NotificationService(dbContext1), new FakeEmailService());
            var cohorteService1 = new CohorteService(dbContext1, userManager1, new FakeEmailService(), preuveService1, new NotificationService(dbContext1));

            var (cId, eId, membres, gId) = await CreerCohorteActiveAsync(dbContext1, cohorteService1, nombreMembres: 3);
            cohorteId = cId;
            etapeId = eId;
            gestionnaireId = gId;
            var auteur = membres[0];
            var pair1 = membres[1];
            var pair2 = membres[2];
            auteurId = auteur.Id;
            pair1Id = pair1.Id;
            pair2Id = pair2.Id;

            var (_, _, pId) = await preuveService1.DeposerOuModifierAsync(auteur.Id, cohorteId, etapeId, "Description", [], null);
            preuveId = pId!.Value;

            await preuveService1.ValiderParPairAsync(preuveId, pair1.Id, DecisionValidationPair.ARevoir, "Manque de détails", "https://test.local/suivi-preuve");
            await preuveService1.ValiderParPairAsync(preuveId, pair2.Id, DecisionValidationPair.Valide, null, "https://test.local/suivi-preuve");
            await preuveService1.ValiderParGestionnaireAsync(preuveId, gestionnaireId, DecisionValidationGestionnaire.Valide, null, "https://test.local/suivi-preuve");
        }

        // "Requete 2" : lecture de "Suivi de ma preuve" via un DbContext totalement
        // independant (identity map vide) pointant sur la meme base - reproduit fidelement
        // un nouveau chargement de page.
        await using var dbContext2 = InMemoryDbContextFactory.Create(nomBase);
        var userManager2 = TestUserManagerFactory.Create(dbContext2);
        var preuveService2 = new PreuveService(dbContext2, userManager2, new FakePreuveFichierStockageService(), new NotificationService(dbContext2), new FakeEmailService());

        var detail = await preuveService2.GetMaPreuveAsync(auteurId, etapeId);

        Assert.NotNull(detail);
        Assert.Equal(3, detail!.Retours.Count);
        Assert.Contains(detail.Retours, r => !r.EstGestionnaire && r.Decision == "À revoir" && r.Commentaire == "Manque de détails");
        Assert.Contains(detail.Retours, r => !r.EstGestionnaire && r.Decision == "Validé" && r.Commentaire is null);
        Assert.Contains(detail.Retours, r => r.EstGestionnaire && r.Decision == "Validé");

        // Trie chronologiquement (cf. VersDetail) - au moins verifier que les dates sont
        // dans l'ordre croissant.
        Assert.True(detail.Retours.SequenceEqual(detail.Retours.OrderBy(r => r.Date)));
    }
}

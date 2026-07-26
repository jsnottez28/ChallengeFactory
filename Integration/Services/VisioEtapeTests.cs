using Application.Common.Interfaces;
using Domain.Entities;
using Integration.TestSupport;
using Web.Data;
using Web.Services;

namespace Integration.Services;

// Couvre le prompt "Visio planifiee par etape" (section 1) - tests minimums explicitement
// demandes dans les contraintes transverses :
// - impossible de confirmer "Lancer la cohorte" / "Valider l'etape..." (hors derniere
//   etape) sans date/heure + lien de la visio ;
// - la derniere etape (cloture) n'exige pas de visio suivante ;
// - le descriptif auto-genere reprend le titre, le defi individuel et les cartes de
//   l'etape introduite.
public class VisioEtapeTests
{
    private static async Task<(Challenge Challenge, List<ChallengeEtape> Etapes, List<CarteCompetence> Cartes)> PreparerChallengePublieAsync(
        ApplicationDbContext dbContext, int nombreEtapes = 2)
    {
        var challengeService = new ChallengeService(dbContext);
        var carteService = new CarteCompetenceService(dbContext);

        var (_, _, challenge) = await challengeService.CreateAsync(new ChallengeInput
        {
            Titre = "Challenge Visio",
            NombreEtapes = nombreEtapes,
            Mode = ModePlateforme.BtoC,
        });

        var etapes = new List<ChallengeEtape>();
        var cartes = new List<CarteCompetence>();
        for (var i = 1; i <= nombreEtapes; i++)
        {
            var (_, _, etape) = await challengeService.CreerEtapeAsync(challenge!.Id, new ChallengeEtapeInput
            {
                TitreEtape = $"Étape {i}",
                DefiIndividuel = $"Défi terrain {i}",
            });
            var (_, _, carte) = await carteService.CreateAsync(new CarteCompetenceInput
            {
                Code = $"CODE-{i}",
                Niveau = NiveauCarte.Debutant,
                TitreTheorie = $"Carte {i}",
            });
            await challengeService.DefinirCartesEtapeAsync(etape!.Id, [carte!.Id]);
            etapes.Add(etape);
            cartes.Add(carte);
        }

        await challengeService.PublierAsync(challenge!.Id);

        return (challenge, etapes, cartes);
    }

    private static (ApplicationDbContext DbContext, ICohorteService CohorteService) CreerServices()
    {
        var dbContext = InMemoryDbContextFactory.Create();
        var userManager = TestUserManagerFactory.Create(dbContext);
        var notificationService = new NotificationService(dbContext);
        var preuveService = new PreuveService(dbContext, userManager, new FakePreuveFichierStockageService(), notificationService, new FakeEmailService());
        var cohorteService = new CohorteService(dbContext, userManager, new FakeEmailService(), preuveService, notificationService);
        return (dbContext, cohorteService);
    }

    [Fact]
    public async Task LancerAsync_Echoue_SansDateHeureNiLienDeLaVisio()
    {
        var (dbContext, cohorteService) = CreerServices();
        await using var _ = dbContext;

        var (challenge, _, _) = await PreparerChallengePublieAsync(dbContext);
        var gestionnaire = new ApplicationUser { UserName = "coach@test.local", Email = "coach@test.local" };
        dbContext.Users.Add(gestionnaire);
        await dbContext.SaveChangesAsync();

        var (_, _, cohorteId) = await cohorteService.CreateAsync(new CohorteInput { ChallengeId = challenge.Id, Nom = "Cohorte Test" });

        var (success, errorMessage) = await cohorteService.LancerAsync(
            cohorteId!.Value, gestionnaire.Id, "https://test.local/parcours",
            dateHeureVisio: null, lienConnexionVisio: null, descriptifVisio: null);

        Assert.False(success);
        Assert.NotNull(errorMessage);

        var cohorte = await dbContext.Cohortes.FindAsync(cohorteId.Value);
        Assert.Equal(StatutCohorte.EnPreparation, cohorte!.Statut);
        Assert.Empty(dbContext.VisiosEtape);
    }

    [Fact]
    public async Task LancerAsync_Reussit_AvecDateHeureEtLienDeLaVisio_EtCreeLaVisioEtape1()
    {
        var (dbContext, cohorteService) = CreerServices();
        await using var _ = dbContext;

        var (challenge, etapes, cartes) = await PreparerChallengePublieAsync(dbContext);
        var gestionnaire = new ApplicationUser { UserName = "coach@test.local", Email = "coach@test.local" };
        dbContext.Users.Add(gestionnaire);
        await dbContext.SaveChangesAsync();

        var (_, _, cohorteId) = await cohorteService.CreateAsync(new CohorteInput { ChallengeId = challenge.Id, Nom = "Cohorte Test" });

        var dateHeure = DateTime.UtcNow.AddDays(3);
        var (success, errorMessage) = await cohorteService.LancerAsync(
            cohorteId!.Value, gestionnaire.Id, "https://test.local/parcours",
            dateHeure, "https://visio.test/salle-1", null);

        Assert.True(success, errorMessage);

        var visio = Assert.Single(dbContext.VisiosEtape);
        Assert.Equal(cohorteId.Value, visio.CohorteId);
        Assert.Equal(etapes[0].Id, visio.ChallengeEtapeId);
        Assert.Equal(dateHeure, visio.DateHeure);
        Assert.Equal("https://visio.test/salle-1", visio.LienConnexion);
        Assert.Contains(etapes[0].TitreEtape, visio.Descriptif);
        Assert.Contains(etapes[0].DefiIndividuel!, visio.Descriptif);
        Assert.Contains(cartes[0].TitreTheorie, visio.Descriptif);
    }

    [Fact]
    public async Task ValiderEtapeAsync_Echoue_SansVisio_SiCeNEstPasLaDerniereEtape()
    {
        var (dbContext, cohorteService) = CreerServices();
        await using var _ = dbContext;

        var (challenge, _, _) = await PreparerChallengePublieAsync(dbContext, nombreEtapes: 2);
        var gestionnaire = new ApplicationUser { UserName = "coach@test.local", Email = "coach@test.local" };
        dbContext.Users.Add(gestionnaire);
        await dbContext.SaveChangesAsync();

        var (_, _, cohorteId) = await cohorteService.CreateAsync(new CohorteInput { ChallengeId = challenge.Id, Nom = "Cohorte Test" });
        await cohorteService.LancerAsync(cohorteId!.Value, gestionnaire.Id, "https://test.local/parcours",
            DateTime.UtcNow.AddDays(1), "https://visio.test/salle-1", null);

        var (success, errorMessage) = await cohorteService.ValiderEtapeAsync(
            cohorteId.Value, gestionnaire.Id, "https://test.local/parcours", "https://test.local/bibliotheque",
            dateHeureVisio: null, lienConnexionVisio: null, descriptifVisio: null);

        Assert.False(success);
        Assert.NotNull(errorMessage);

        // Toujours a l'etape 1 : la validation a bien ete refusee, pas juste "sans visio".
        var cohorte = await dbContext.Cohortes.FindAsync(cohorteId.Value);
        Assert.Equal(1, cohorte!.EtapeCourante);
        Assert.Single(dbContext.VisiosEtape); // uniquement celle de l'etape 1 (lancement)
    }

    [Fact]
    public async Task ValiderEtapeAsync_SurLaDerniereEtape_NeDemandePasDeVisio_EtClotureLaCohorte()
    {
        var (dbContext, cohorteService) = CreerServices();
        await using var _ = dbContext;

        var (challenge, _, _) = await PreparerChallengePublieAsync(dbContext, nombreEtapes: 1);
        var gestionnaire = new ApplicationUser { UserName = "coach@test.local", Email = "coach@test.local" };
        dbContext.Users.Add(gestionnaire);
        await dbContext.SaveChangesAsync();

        var (_, _, cohorteId) = await cohorteService.CreateAsync(new CohorteInput { ChallengeId = challenge.Id, Nom = "Cohorte Test" });
        await cohorteService.LancerAsync(cohorteId!.Value, gestionnaire.Id, "https://test.local/parcours",
            DateTime.UtcNow.AddDays(1), "https://visio.test/salle-1", null);

        // Aucun champ de visio fourni (null partout) : doit reussir puisque l'etape 1 est
        // ici la derniere etape (NombreEtapes = 1) - pas d'etape suivante a introduire.
        var (success, errorMessage) = await cohorteService.ValiderEtapeAsync(
            cohorteId.Value, gestionnaire.Id, "https://test.local/parcours", "https://test.local/bibliotheque",
            dateHeureVisio: null, lienConnexionVisio: null, descriptifVisio: null);

        Assert.True(success, errorMessage);

        var cohorte = await dbContext.Cohortes.FindAsync(cohorteId.Value);
        Assert.Equal(StatutCohorte.Terminee, cohorte!.Statut);

        // Toujours une seule visio (celle de l'etape 1 au lancement) : aucune visio
        // "fantome" creee pour une etape suivante qui n'existe pas.
        Assert.Single(dbContext.VisiosEtape);
    }

    [Fact]
    public async Task ValiderEtapeAsync_Reussit_AvecVisio_SiCeNEstPasLaDerniereEtape_EtCreeLaVisioDeLEtapeSuivante()
    {
        var (dbContext, cohorteService) = CreerServices();
        await using var _ = dbContext;

        var (challenge, etapes, cartes) = await PreparerChallengePublieAsync(dbContext, nombreEtapes: 2);
        var gestionnaire = new ApplicationUser { UserName = "coach@test.local", Email = "coach@test.local" };
        dbContext.Users.Add(gestionnaire);
        await dbContext.SaveChangesAsync();

        var (_, _, cohorteId) = await cohorteService.CreateAsync(new CohorteInput { ChallengeId = challenge.Id, Nom = "Cohorte Test" });
        await cohorteService.LancerAsync(cohorteId!.Value, gestionnaire.Id, "https://test.local/parcours",
            DateTime.UtcNow.AddDays(1), "https://visio.test/salle-1", null);

        var dateHeureEtape2 = DateTime.UtcNow.AddDays(8);
        var (success, errorMessage) = await cohorteService.ValiderEtapeAsync(
            cohorteId.Value, gestionnaire.Id, "https://test.local/parcours", "https://test.local/bibliotheque",
            dateHeureEtape2, "https://visio.test/salle-2", null);

        Assert.True(success, errorMessage);

        var visioEtape2 = Assert.Single(dbContext.VisiosEtape, v => v.ChallengeEtapeId == etapes[1].Id);
        Assert.Equal(dateHeureEtape2, visioEtape2.DateHeure);
        Assert.Equal("https://visio.test/salle-2", visioEtape2.LienConnexion);
        Assert.Contains(etapes[1].TitreEtape, visioEtape2.Descriptif);
        Assert.Contains(etapes[1].DefiIndividuel!, visioEtape2.Descriptif);
        Assert.Contains(cartes[1].TitreTheorie, visioEtape2.Descriptif);

        // 2 visios en tout : etape 1 (lancement) + etape 2 (validation).
        Assert.Equal(2, dbContext.VisiosEtape.Count());
    }

    [Fact]
    public async Task GenererDescriptifVisioAsync_RepredLeTitreLeDefiEtLesCartesDeLEtape()
    {
        var (dbContext, cohorteService) = CreerServices();
        await using var _ = dbContext;

        var (_, etapes, cartes) = await PreparerChallengePublieAsync(dbContext, nombreEtapes: 1);

        var descriptif = await cohorteService.GenererDescriptifVisioAsync(etapes[0].Id);

        Assert.Contains(etapes[0].TitreEtape, descriptif);
        Assert.Contains(etapes[0].DefiIndividuel!, descriptif);
        Assert.Contains(cartes[0].TitreTheorie, descriptif);
        // Agenda fixe en 3 temps (prompt section 1.3).
        Assert.Contains("difficultés rencontrées", descriptif);
        Assert.Contains("Atelier collectif", descriptif);
    }
}

using Application.Common.Interfaces;
using Domain.Entities;
using Integration.TestSupport;
using Web.Data;
using Web.Services;

namespace Integration.Services;

// Couvre la recuperation, depuis la bibliotheque de cartes, de la ou des preuves
// deposees lors des (precedents) Challenges qui ont attribue une carte a l'apprenant.
public class CarteApprenantPreuvesTests
{
    private static async Task<(Challenge Challenge, ChallengeEtape Etape, CarteCompetence Carte)> PreparerChallengeAvecCarteAsync(
        ApplicationDbContext dbContext, string titreChallenge, string codeCarte)
    {
        var challengeService = new ChallengeService(dbContext);
        var carteService = new CarteCompetenceService(dbContext);

        var (_, _, challenge) = await challengeService.CreateAsync(new ChallengeInput
        {
            Titre = titreChallenge,
            NombreEtapes = 1,
            Mode = ModePlateforme.BtoC,
        });
        var (_, _, etape) = await challengeService.CreerEtapeAsync(challenge!.Id, new ChallengeEtapeInput
        {
            TitreEtape = "Étape 1",
            DefiIndividuel = "Défi terrain",
        });
        var (_, _, carte) = await carteService.CreateAsync(new CarteCompetenceInput
        {
            Code = codeCarte,
            Niveau = NiveauCarte.Debutant,
            TitreTheorie = "Carte partagée",
        });
        await challengeService.DefinirCartesEtapeAsync(etape!.Id, [carte!.Id]);
        await challengeService.PublierAsync(challenge.Id);

        return (challenge, etape, carte);
    }

    private static (ApplicationDbContext DbContext, ICohorteService CohorteService, IPreuveService PreuveService, ICarteApprenantService ApprenantService) CreerServices()
    {
        var dbContext = InMemoryDbContextFactory.Create();
        var userManager = TestUserManagerFactory.Create(dbContext);
        var notificationService = new NotificationService(dbContext);
        var preuveService = new PreuveService(dbContext, userManager, new FakePreuveFichierStockageService(), notificationService, new FakeEmailService());
        var cohorteService = new CohorteService(dbContext, userManager, new FakeEmailService(), preuveService, notificationService);
        var apprenantService = new CarteApprenantService(dbContext, userManager, preuveService);
        return (dbContext, cohorteService, preuveService, apprenantService);
    }

    [Fact]
    public async Task GetMesPreuvesPourCarteAsync_RetourneLaPreuveDeposeePourLeChallengeQuiAAttribueLaCarte()
    {
        var (dbContext, cohorteService, preuveService, apprenantService) = CreerServices();
        await using var _ = dbContext;

        var (challenge, etape, carte) = await PreparerChallengeAvecCarteAsync(dbContext, "Challenge A", "CODE-A");
        var apprenant = new ApplicationUser { UserName = "apprenant@test.local", Email = "apprenant@test.local" };
        var gestionnaire = new ApplicationUser { UserName = "coach@test.local", Email = "coach@test.local" };
        dbContext.Users.AddRange(apprenant, gestionnaire);
        await dbContext.SaveChangesAsync();

        var (_, _, cohorteId) = await cohorteService.CreateAsync(new CohorteInput { ChallengeId = challenge.Id, Nom = "Cohorte A" });
        await cohorteService.AjouterMembreManuelAsync(cohorteId!.Value, apprenant.Id);
        await cohorteService.LancerAsync(cohorteId.Value, gestionnaire.Id, "https://test.local/parcours",
            DateTime.UtcNow.AddDays(1), "https://visio.test/salle", null);

        var fichier = new FichierPreuveInput { NomFichier = "capture.png", Contenu = new MemoryStream([1, 2, 3]), TailleOctets = 3 };
        await preuveService.DeposerOuModifierAsync(apprenant.Id, cohorteId.Value, etape.Id, "Mon défi rendu", [fichier], null);

        var preuvesCarte = await apprenantService.GetMesPreuvesPourCarteAsync(apprenant.Id, carte.Id);

        var entree = Assert.Single(preuvesCarte);
        Assert.Equal("Challenge A", entree.ChallengeTitre);
        Assert.Equal(1, entree.NumeroEtape);
        Assert.NotNull(entree.Preuve);
        Assert.Equal("Mon défi rendu", entree.Preuve!.Description);
        var fichierRecu = Assert.Single(entree.Preuve.Fichiers);
        Assert.Equal("capture.png", fichierRecu.NomFichier);
    }

    [Fact]
    public async Task GetMesPreuvesPourCarteAsync_UneEntreeParChallengeQuandLaMemeCarteEstAttribueeDeuxFois()
    {
        var (dbContext, cohorteService, preuveService, apprenantService) = CreerServices();
        await using var _ = dbContext;

        // Deux Challenges DIFFERENTS qui rattachent la MEME carte (meme Code) comme
        // Ressource Directrice - simule une carte reutilisee dans plusieurs parcours.
        var carteService = new CarteCompetenceService(dbContext);
        var (challengeA, etapeA, carte) = await PreparerChallengeAvecCarteAsync(dbContext, "Challenge A", "CODE-PARTAGE");

        var challengeService = new ChallengeService(dbContext);
        var (_, _, challengeB) = await challengeService.CreateAsync(new ChallengeInput { Titre = "Challenge B", NombreEtapes = 1, Mode = ModePlateforme.BtoC });
        var (_, _, etapeB) = await challengeService.CreerEtapeAsync(challengeB!.Id, new ChallengeEtapeInput { TitreEtape = "Étape 1", DefiIndividuel = "Autre défi" });
        await challengeService.DefinirCartesEtapeAsync(etapeB!.Id, [carte.Id]);
        await challengeService.PublierAsync(challengeB.Id);

        var apprenant = new ApplicationUser { UserName = "apprenant@test.local", Email = "apprenant@test.local" };
        var gestionnaire = new ApplicationUser { UserName = "coach@test.local", Email = "coach@test.local" };
        dbContext.Users.AddRange(apprenant, gestionnaire);
        await dbContext.SaveChangesAsync();

        var (_, _, cohorteIdA) = await cohorteService.CreateAsync(new CohorteInput { ChallengeId = challengeA.Id, Nom = "Cohorte A" });
        await cohorteService.AjouterMembreManuelAsync(cohorteIdA!.Value, apprenant.Id);
        await cohorteService.LancerAsync(cohorteIdA.Value, gestionnaire.Id, "https://test.local/parcours", DateTime.UtcNow.AddDays(1), "https://visio.test/a", null);
        await preuveService.DeposerOuModifierAsync(apprenant.Id, cohorteIdA.Value, etapeA.Id, "Défi A rendu", [], null);

        var (_, _, cohorteIdB) = await cohorteService.CreateAsync(new CohorteInput { ChallengeId = challengeB.Id, Nom = "Cohorte B" });
        await cohorteService.AjouterMembreManuelAsync(cohorteIdB!.Value, apprenant.Id);
        await cohorteService.LancerAsync(cohorteIdB.Value, gestionnaire.Id, "https://test.local/parcours", DateTime.UtcNow.AddDays(1), "https://visio.test/b", null);
        await preuveService.DeposerOuModifierAsync(apprenant.Id, cohorteIdB.Value, etapeB.Id, "Défi B rendu", [], null);

        var preuvesCarte = await apprenantService.GetMesPreuvesPourCarteAsync(apprenant.Id, carte.Id);

        Assert.Equal(2, preuvesCarte.Count);
        Assert.Contains(preuvesCarte, p => p.ChallengeTitre == "Challenge A" && p.Preuve!.Description == "Défi A rendu");
        Assert.Contains(preuvesCarte, p => p.ChallengeTitre == "Challenge B" && p.Preuve!.Description == "Défi B rendu");
    }

    [Fact]
    public async Task GetMesPreuvesPourCarteAsync_VideSurUneCarteAttribueeUniquementEnLibre()
    {
        var (dbContext, _, _, apprenantService) = CreerServices();
        await using var _ = dbContext;

        var carteService = new CarteCompetenceService(dbContext);
        var (_, _, carte) = await carteService.CreateAsync(new CarteCompetenceInput { Code = "LIBRE-1", Niveau = NiveauCarte.Debutant, TitreTheorie = "Carte libre" });

        var apprenant = new ApplicationUser { UserName = "apprenant@test.local", Email = "apprenant@test.local" };
        var coach = new ApplicationUser { UserName = "coach@test.local", Email = "coach@test.local" };
        dbContext.Users.AddRange(apprenant, coach);
        await dbContext.SaveChangesAsync();

        await carteService.AttribuerAsync([carte!.Id], [apprenant.Id], coach.Id, null);

        var preuvesCarte = await apprenantService.GetMesPreuvesPourCarteAsync(apprenant.Id, carte.Id);

        Assert.Empty(preuvesCarte);
    }
}

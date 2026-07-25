using Application.Common.Interfaces;
using Domain.Entities;
using Integration.TestSupport;
using Microsoft.EntityFrameworkCore;
using Web.Data;
using Web.Services;

namespace Integration.Services;

// Couvre les exigences transverses du moteur de Challenges : separation stricte entre
// "Mon parcours en cours" et "Ma bibliotheque", et blocage serveur de tout contenu
// apprenant pour un compte dont le statut_acces_plateforme n'est pas Actif - meme avec
// des cartes/cohortes deja attribuees par ailleurs.
public class AccesApprenantChallengeTests
{
    private static async Task<(int CohorteId, string GestionnaireId)> CreerEtLancerCohorteAsync(
        ApplicationDbContext dbContext,
        CohorteService cohorteService,
        ApplicationUser membre,
        string nomCohorte,
        ModePlateforme mode = ModePlateforme.BtoC)
    {
        var challengeService = new ChallengeService(dbContext);
        var carteService = new CarteCompetenceService(dbContext);

        var (_, _, challenge) = await challengeService.CreateAsync(new ChallengeInput { Titre = nomCohorte, NombreEtapes = 1, Mode = mode });
        var (_, _, etape) = await challengeService.CreerEtapeAsync(challenge!.Id, new ChallengeEtapeInput { TitreEtape = "Étape 1", DefiIndividuel = "Défi" });
        var (_, _, carte) = await carteService.CreateAsync(new CarteCompetenceInput { Code = $"CODE-{nomCohorte}", Niveau = NiveauCarte.Debutant, TitreTheorie = $"Carte {nomCohorte}" });
        await challengeService.DefinirCartesEtapeAsync(etape!.Id, [carte!.Id]);
        await challengeService.PublierAsync(challenge.Id);

        var gestionnaire = new ApplicationUser { UserName = $"coach-{nomCohorte}@test.local", Email = $"coach-{nomCohorte}@test.local" };
        dbContext.Users.Add(gestionnaire);
        await dbContext.SaveChangesAsync();

        var (_, _, cohorteId) = await cohorteService.CreateAsync(new CohorteInput { ChallengeId = challenge.Id, Nom = nomCohorte });
        await cohorteService.AjouterMembreManuelAsync(cohorteId!.Value, membre.Id);
        await cohorteService.LancerAsync(cohorteId.Value, gestionnaire.Id, "https://test.local/parcours");

        return (cohorteId.Value, gestionnaire.Id);
    }

    [Fact]
    public async Task CompteEnAttenteDeValidation_NaAccesAAucunContenu_MemeAvecCarteEtCohorteAttribuees()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var userManager = TestUserManagerFactory.Create(dbContext);
        var cohorteService = new CohorteService(dbContext, userManager, new FakeEmailService(), new PreuveService(dbContext, userManager, new FakePreuveFichierStockageService()));
        var apprenantService = new CarteApprenantService(dbContext, userManager);

        var apprenant = new ApplicationUser { UserName = "apprenant@test.local", Email = "apprenant@test.local", Statut = StatutUtilisateur.Modere };
        dbContext.Users.Add(apprenant);
        await dbContext.SaveChangesAsync();

        await CreerEtLancerCohorteAsync(dbContext, cohorteService, apprenant, "ChallengeA");

        var parcours = await cohorteService.GetMesParcoursEnCoursAsync(apprenant.Id);
        var bibliotheque = await apprenantService.GetMesCartesAsync(apprenant.Id);

        Assert.Empty(parcours);
        Assert.Empty(bibliotheque);
    }

    [Fact]
    public async Task CompteSuspendu_NaAccesAAucunContenu()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var userManager = TestUserManagerFactory.Create(dbContext);
        var cohorteService = new CohorteService(dbContext, userManager, new FakeEmailService(), new PreuveService(dbContext, userManager, new FakePreuveFichierStockageService()));
        var apprenantService = new CarteApprenantService(dbContext, userManager);

        var apprenant = new ApplicationUser { UserName = "apprenant@test.local", Email = "apprenant@test.local", Statut = StatutUtilisateur.Actif };
        dbContext.Users.Add(apprenant);
        await dbContext.SaveChangesAsync();

        await CreerEtLancerCohorteAsync(dbContext, cohorteService, apprenant, "ChallengeA");

        // L'acces fonctionnait tant que le compte etait Actif...
        Assert.NotEmpty(await cohorteService.GetMesParcoursEnCoursAsync(apprenant.Id));

        // ...puis est coupe des que le Gestionnaire suspend l'acces.
        apprenant.Statut = StatutUtilisateur.Inactif;
        await userManager.UpdateAsync(apprenant);

        Assert.Empty(await cohorteService.GetMesParcoursEnCoursAsync(apprenant.Id));
        Assert.Empty(await apprenantService.GetMesCartesAsync(apprenant.Id));
    }

    [Fact]
    public async Task ValidationAbonnement_DebloqueAccesATousLesChallengesBtoCEnCours_PasSeulementLeDernier()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var userManager = TestUserManagerFactory.Create(dbContext);
        var cohorteService = new CohorteService(dbContext, userManager, new FakeEmailService(), new PreuveService(dbContext, userManager, new FakePreuveFichierStockageService()));

        var apprenant = new ApplicationUser { UserName = "apprenant@test.local", Email = "apprenant@test.local", Statut = StatutUtilisateur.Modere };
        dbContext.Users.Add(apprenant);
        await dbContext.SaveChangesAsync();

        // Deux Challenges BtoC distincts, rejoints alors que le compte est encore En
        // attente de validation.
        await CreerEtLancerCohorteAsync(dbContext, cohorteService, apprenant, "ChallengeA");
        await CreerEtLancerCohorteAsync(dbContext, cohorteService, apprenant, "ChallengeB");

        Assert.Empty(await cohorteService.GetMesParcoursEnCoursAsync(apprenant.Id));

        // Le Gestionnaire valide l'abonnement une seule fois, au niveau du compte.
        apprenant.Statut = StatutUtilisateur.Actif;
        await userManager.UpdateAsync(apprenant);

        var parcours = await cohorteService.GetMesParcoursEnCoursAsync(apprenant.Id);

        Assert.Equal(2, parcours.Count);
        Assert.Contains(parcours, p => p.ChallengeTitre == "ChallengeA");
        Assert.Contains(parcours, p => p.ChallengeTitre == "ChallengeB");
    }

    [Fact]
    public async Task CohorteTerminee_DisparaitDeMonParcours_MaisResteDansLaBibliotheque()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var userManager = TestUserManagerFactory.Create(dbContext);
        var cohorteService = new CohorteService(dbContext, userManager, new FakeEmailService(), new PreuveService(dbContext, userManager, new FakePreuveFichierStockageService()));
        var apprenantService = new CarteApprenantService(dbContext, userManager);

        var apprenant = new ApplicationUser { UserName = "apprenant@test.local", Email = "apprenant@test.local", Statut = StatutUtilisateur.Actif };
        dbContext.Users.Add(apprenant);
        await dbContext.SaveChangesAsync();

        var (cohorteId, gestionnaireId) = await CreerEtLancerCohorteAsync(dbContext, cohorteService, apprenant, "ChallengeUneEtape");

        Assert.Single(await cohorteService.GetMesParcoursEnCoursAsync(apprenant.Id));

        // Une seule etape : la valider cloture la Cohorte.
        await cohorteService.ValiderEtapeAsync(cohorteId, gestionnaireId, "https://test.local/parcours", "https://test.local/bibliotheque");

        var parcours = await cohorteService.GetMesParcoursEnCoursAsync(apprenant.Id);
        var bibliotheque = await apprenantService.GetMesCartesAsync(apprenant.Id);

        Assert.Empty(parcours);
        var carteBibliotheque = Assert.Single(bibliotheque);
        Assert.Equal(OrigineAttribution.Challenge, carteBibliotheque.OrigineType);
        Assert.Equal("ChallengeUneEtape", carteBibliotheque.ChallengeTitre);
    }
}

using Application.Common.Interfaces;
using Domain.Entities;
using Integration.TestSupport;
using Microsoft.EntityFrameworkCore;
using Web.Data;
using Web.Services;
using static Integration.TestSupport.EmargementTestHelper;

namespace Integration.Services;

public class EmargementServiceTests
{
    private static async Task<(Challenge Challenge, List<ChallengeEtape> Etapes, List<CarteCompetence> Cartes)> PreparerChallengePublieAsync(
        ApplicationDbContext dbContext, int nombreEtapes = 1)
    {
        var challengeService = new ChallengeService(dbContext);
        var carteService = new CarteCompetenceService(dbContext);

        var (_, _, challenge) = await challengeService.CreateAsync(new ChallengeInput
        {
            Titre = "Challenge Test",
            NombreEtapes = nombreEtapes,
            Mode = ModePlateforme.BtoC,
        });

        var etapes = new List<ChallengeEtape>();
        var cartes = new List<CarteCompetence>();
        for (var i = 1; i <= nombreEtapes; i++)
        {
            var (_, _, etape) = await challengeService.CreerEtapeAsync(challenge!.Id, new ChallengeEtapeInput { TitreEtape = $"Étape {i}" });
            var (_, _, carte) = await carteService.CreateAsync(new CarteCompetenceInput { Code = $"CODE-{i}", Niveau = NiveauCarte.Debutant, TitreTheorie = $"Carte {i}" });
            await challengeService.DefinirCartesEtapeAsync(etape!.Id, [carte!.Id]);
            etapes.Add(etape);
            cartes.Add(carte);
        }

        await challengeService.PublierAsync(challenge!.Id);

        return (challenge, etapes, cartes);
    }

    private static async Task<(CohorteService CohorteService, EmargementService EmargementService, VisioService VisioService, int CohorteId, ApplicationUser Gestionnaire, ApplicationUser Apprenant)> PreparerCohorteActiveAsync(ApplicationDbContext dbContext)
    {
        var userManager = TestUserManagerFactory.Create(dbContext);
        var emailService = new FakeEmailService();
        var cohorteService = new CohorteService(dbContext, userManager, emailService, new PreuveService(dbContext, userManager, new FakePreuveFichierStockageService(), new NotificationService(dbContext), new FakeEmailService()), new NotificationService(dbContext));
        var emargementService = new EmargementService(dbContext, emailService, new FakePreuveFichierStockageService());
        var visioService = new VisioService(dbContext, emailService);

        var (challenge, _, _) = await PreparerChallengePublieAsync(dbContext);

        var gestionnaire = new ApplicationUser { UserName = "coach@test.local", Email = "coach@test.local" };
        var apprenant = new ApplicationUser { UserName = "apprenant@test.local", Email = "apprenant@test.local" };
        dbContext.Users.AddRange(gestionnaire, apprenant);
        await dbContext.SaveChangesAsync();

        var (_, _, cohorteId) = await cohorteService.CreateAsync(new CohorteInput { ChallengeId = challenge.Id, Nom = "Cohorte Test" });
        await cohorteService.AjouterMembreManuelAsync(cohorteId!.Value, apprenant.Id);
        await cohorteService.LancerAsync(cohorteId.Value, gestionnaire.Id, "https://test.local/parcours", "https://test.local/mi-parcours");

        return (cohorteService, emargementService, visioService, cohorteId.Value, gestionnaire, apprenant);
    }

    [Fact]
    public async Task EnvoyerEmargementsEtapeCouranteAsync_Echoue_SansDateDeVisioPlanifiee()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var (_, emargementService, _, cohorteId, _, _) = await PreparerCohorteActiveAsync(dbContext);

        var (success, errorMessage) = await emargementService.EnvoyerEmargementsEtapeCouranteAsync(cohorteId, _ => "https://test.local/emargement");

        Assert.False(success);
        Assert.Contains("visio", errorMessage);
        Assert.False(await dbContext.Emargements.AnyAsync());
    }

    [Fact]
    public async Task EnvoyerEmargementsEtapeCouranteAsync_Reussit_EtRepriseLaDateDeLaVisioSurChaqueEmargement()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var (_, emargementService, visioService, cohorteId, gestionnaire, _) = await PreparerCohorteActiveAsync(dbContext);

        var dateVisio = new DateTime(2026, 10, 1, 14, 0, 0, DateTimeKind.Utc);
        await visioService.PlanifierAsync(cohorteId, gestionnaire.Id, dateVisio, "https://meet.test.local/seance");

        var (success, errorMessage) = await emargementService.EnvoyerEmargementsEtapeCouranteAsync(cohorteId, _ => "https://test.local/emargement");

        Assert.True(success, errorMessage);
        var emargement = await dbContext.Emargements.SingleAsync();
        Assert.Equal(dateVisio, emargement.DateSeance);
    }

    [Fact]
    public async Task SignerAsync_Echoue_SiAucuneCarteCochee()
    {
        // Regression : signer sans avoir coche aucune carte (mais avec heures et signature
        // fournies) ne doit jamais renvoyer un faux succes silencieux - c'est le bug remonte
        // par le Gestionnaire ("enregistré" affiché alors que rien n'avait ete coche, sans
        // message pour dire de cocher la carte).
        await using var dbContext = InMemoryDbContextFactory.Create();
        var (_, emargementService, visioService, cohorteId, gestionnaire, apprenant) = await PreparerCohorteActiveAsync(dbContext);

        await visioService.PlanifierAsync(cohorteId, gestionnaire.Id, DateTime.UtcNow, "https://meet.test.local/seance");
        await emargementService.EnvoyerEmargementsEtapeCouranteAsync(cohorteId, _ => "https://test.local/emargement");

        var signaturePng = new byte[] { 137, 80, 78, 71 };
        var (success, errorMessage) = await emargementService.SignerAsync(cohorteId, apprenant.Id, [], 1m, 1m, signaturePng);

        Assert.False(success);
        Assert.Contains("cocher", errorMessage, StringComparison.OrdinalIgnoreCase);
        var emargement = await dbContext.Emargements.SingleAsync();
        Assert.Null(emargement.SigneLe);
    }

    [Fact]
    public async Task SignerAsync_Echoue_SansSignature()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var (_, emargementService, visioService, cohorteId, gestionnaire, apprenant) = await PreparerCohorteActiveAsync(dbContext);

        await visioService.PlanifierAsync(cohorteId, gestionnaire.Id, DateTime.UtcNow, "https://meet.test.local/seance");
        await emargementService.EnvoyerEmargementsEtapeCouranteAsync(cohorteId, _ => "https://test.local/emargement");

        var aSigner = await emargementService.GetPourSignatureAsync(cohorteId, apprenant.Id);
        var emargementIds = aSigner!.Cartes.Select(c => c.EmargementId).ToList();

        var (success, errorMessage) = await emargementService.SignerAsync(cohorteId, apprenant.Id, emargementIds, 1m, 1m, null);

        Assert.False(success);
        Assert.Contains("signature", errorMessage, StringComparison.OrdinalIgnoreCase);
        var emargement = await dbContext.Emargements.SingleAsync();
        Assert.Null(emargement.SigneLe);
    }

    [Fact]
    public async Task SignerAsync_Echoue_SansHeuresRenseignees()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var (_, emargementService, visioService, cohorteId, gestionnaire, apprenant) = await PreparerCohorteActiveAsync(dbContext);

        await visioService.PlanifierAsync(cohorteId, gestionnaire.Id, DateTime.UtcNow, "https://meet.test.local/seance");
        await emargementService.EnvoyerEmargementsEtapeCouranteAsync(cohorteId, _ => "https://test.local/emargement");

        var aSigner = await emargementService.GetPourSignatureAsync(cohorteId, apprenant.Id);
        var emargementIds = aSigner!.Cartes.Select(c => c.EmargementId).ToList();
        var signaturePng = new byte[] { 137, 80, 78, 71 };

        var (success, errorMessage) = await emargementService.SignerAsync(cohorteId, apprenant.Id, emargementIds, null, null, signaturePng);

        Assert.False(success);
        Assert.Contains("heures", errorMessage, StringComparison.OrdinalIgnoreCase);
        var emargement = await dbContext.Emargements.SingleAsync();
        Assert.Null(emargement.SigneLe);
    }

    [Fact]
    public async Task SignerAsync_AvecSignature_EnregistreLaSignatureEtPermetSonTelechargementParLAuteurOuUnGestionnaire()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var (_, emargementService, visioService, cohorteId, gestionnaire, apprenant) = await PreparerCohorteActiveAsync(dbContext);

        var autreUtilisateur = new ApplicationUser { UserName = "autre@test.local", Email = "autre@test.local" };
        dbContext.Users.Add(autreUtilisateur);
        await dbContext.SaveChangesAsync();

        await visioService.PlanifierAsync(cohorteId, gestionnaire.Id, DateTime.UtcNow, "https://meet.test.local/seance");
        await emargementService.EnvoyerEmargementsEtapeCouranteAsync(cohorteId, _ => "https://test.local/emargement");

        var aSigner = await emargementService.GetPourSignatureAsync(cohorteId, apprenant.Id);
        var emargementIds = aSigner!.Cartes.Select(c => c.EmargementId).ToList();
        var signaturePng = new byte[] { 137, 80, 78, 71 };

        var (success, errorMessage) = await emargementService.SignerAsync(cohorteId, apprenant.Id, emargementIds, 3m, 4m, signaturePng);
        Assert.True(success, errorMessage);

        var emargement = await dbContext.Emargements.SingleAsync();
        Assert.NotNull(emargement.SigneLe);
        Assert.False(string.IsNullOrWhiteSpace(emargement.SignatureCheminStockage));

        // L'auteur peut retelecharger sa propre signature.
        var telechargementAuteur = await emargementService.TelechargerSignatureAsync(emargement.Id, apprenant.Id, estGestionnaire: false);
        Assert.NotNull(telechargementAuteur);

        // Un Gestionnaire (droit COHORTE.CONSULTER) peut aussi la consulter.
        var telechargementGestionnaire = await emargementService.TelechargerSignatureAsync(emargement.Id, gestionnaire.Id, estGestionnaire: true);
        Assert.NotNull(telechargementGestionnaire);

        // Un autre membre sans le droit ne peut pas acceder a la signature d'autrui.
        var telechargementRefuse = await emargementService.TelechargerSignatureAsync(emargement.Id, autreUtilisateur.Id, estGestionnaire: false);
        Assert.Null(telechargementRefuse);
    }

    [Fact]
    public async Task GetRecapCohorteAsync_AgregeLesHeuresParSeanceSurToutesLesEtapesDejaEmargees()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var userManager = TestUserManagerFactory.Create(dbContext);
        var emailService = new FakeEmailService();
        var cohorteService = new CohorteService(dbContext, userManager, emailService, new PreuveService(dbContext, userManager, new FakePreuveFichierStockageService(), new NotificationService(dbContext), new FakeEmailService()), new NotificationService(dbContext));
        var emargementService = new EmargementService(dbContext, emailService, new FakePreuveFichierStockageService());
        var visioService = new VisioService(dbContext, emailService);

        var (challenge, _, _) = await PreparerChallengePublieAsync(dbContext, nombreEtapes: 2);

        var gestionnaire = new ApplicationUser { UserName = "coach@test.local", Email = "coach@test.local" };
        var apprenant = new ApplicationUser { UserName = "apprenant@test.local", Email = "apprenant@test.local", Prenom = "Jeanne", Nom = "Dupont" };
        dbContext.Users.AddRange(gestionnaire, apprenant);
        await dbContext.SaveChangesAsync();

        var (_, _, cohorteId) = await cohorteService.CreateAsync(new CohorteInput { ChallengeId = challenge.Id, Nom = "Cohorte Test" });
        await cohorteService.AjouterMembreManuelAsync(cohorteId!.Value, apprenant.Id);
        await cohorteService.LancerAsync(cohorteId.Value, gestionnaire.Id, "https://test.local/parcours", "https://test.local/mi-parcours");

        // Etape 1 : 2h de presence + 1h de travail personnel.
        await visioService.PlanifierAsync(cohorteId.Value, gestionnaire.Id, DateTime.UtcNow, "https://meet.test.local/seance1");
        await emargementService.EnvoyerEmargementsEtapeCouranteAsync(cohorteId.Value, _ => "https://test.local/emargement");
        var aSigner1 = await emargementService.GetPourSignatureAsync(cohorteId.Value, apprenant.Id);
        await emargementService.SignerAsync(cohorteId.Value, apprenant.Id, aSigner1!.Cartes.Select(c => c.EmargementId).ToList(), 2m, 1m, [1, 2, 3]);

        await RepondreTestPositionnementRequisAsync(dbContext, emailService, cohorteId.Value, gestionnaire.Id, apprenant);

        await cohorteService.ValiderEtapeAsync(cohorteId.Value, gestionnaire.Id, "https://test.local/parcours", "https://test.local/bibliotheque", "https://test.local/satisfaction", "https://test.local/mi-parcours", "https://test.local/attestation");

        // Etape 2 : 3h de presence + 2h de travail personnel.
        await visioService.PlanifierAsync(cohorteId.Value, gestionnaire.Id, DateTime.UtcNow, "https://meet.test.local/seance2");
        await emargementService.EnvoyerEmargementsEtapeCouranteAsync(cohorteId.Value, _ => "https://test.local/emargement");
        var aSigner2 = await emargementService.GetPourSignatureAsync(cohorteId.Value, apprenant.Id);
        await emargementService.SignerAsync(cohorteId.Value, apprenant.Id, aSigner2!.Cartes.Select(c => c.EmargementId).ToList(), 3m, 2m, [1, 2, 3]);

        var recap = await emargementService.GetRecapCohorteAsync(cohorteId.Value);

        var membreRecap = Assert.Single(recap);
        Assert.Equal("Jeanne Dupont", membreRecap.NomComplet);
        Assert.Equal(2, membreRecap.NombreSeancesSignees);
        Assert.Equal(2, membreRecap.NombreSeancesTotal);
        Assert.Equal(5m, membreRecap.TotalHeuresPresence);
        Assert.Equal(3m, membreRecap.TotalHeuresTravailPersonnel);
    }
}

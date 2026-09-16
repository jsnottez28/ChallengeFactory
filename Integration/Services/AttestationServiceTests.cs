using Application.Common.Interfaces;
using Domain.Entities;
using Integration.TestSupport;
using Microsoft.EntityFrameworkCore;
using Web.Data;
using Web.Services;

namespace Integration.Services;

public class AttestationServiceTests
{
    private static async Task<(Challenge Challenge, List<ChallengeEtape> Etapes, List<CarteCompetence> Cartes)> PreparerChallengePublieAsync(
        ApplicationDbContext dbContext, int nombreEtapes)
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

    [Fact]
    public async Task GetAttestationAsync_RenvoieNull_SiLaCohorteNEstPasTerminee()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var userManager = TestUserManagerFactory.Create(dbContext);
        var cohorteService = new CohorteService(dbContext, userManager, new FakeEmailService(), new PreuveService(dbContext, userManager, new FakePreuveFichierStockageService(), new NotificationService(dbContext), new FakeEmailService()), new NotificationService(dbContext));
        var attestationService = new AttestationService(dbContext);

        var (challenge, _, _) = await PreparerChallengePublieAsync(dbContext, nombreEtapes: 2);
        var apprenant = new ApplicationUser { UserName = "apprenant@test.local", Email = "apprenant@test.local" };
        dbContext.Users.Add(apprenant);
        await dbContext.SaveChangesAsync();

        var (_, _, cohorteId) = await cohorteService.CreateAsync(new CohorteInput { ChallengeId = challenge.Id, Nom = "Cohorte Test" });
        await cohorteService.AjouterMembreManuelAsync(cohorteId!.Value, apprenant.Id);

        var attestation = await attestationService.GetAttestationAsync(cohorteId.Value, apprenant.Id);

        Assert.Null(attestation);
    }

    [Fact]
    public async Task GetAttestationAsync_RenvoieNull_SiLUtilisateurNEstPasMembre()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var userManager = TestUserManagerFactory.Create(dbContext);
        var cohorteService = new CohorteService(dbContext, userManager, new FakeEmailService(), new PreuveService(dbContext, userManager, new FakePreuveFichierStockageService(), new NotificationService(dbContext), new FakeEmailService()), new NotificationService(dbContext));
        var attestationService = new AttestationService(dbContext);

        var (challenge, _, _) = await PreparerChallengePublieAsync(dbContext, nombreEtapes: 1);
        var etranger = new ApplicationUser { UserName = "etranger@test.local", Email = "etranger@test.local" };
        dbContext.Users.Add(etranger);
        await dbContext.SaveChangesAsync();

        var (_, _, cohorteId) = await cohorteService.CreateAsync(new CohorteInput { ChallengeId = challenge.Id, Nom = "Cohorte Test" });
        await cohorteService.LancerAsync(cohorteId!.Value, "gestionnaire-id", "https://test.local/parcours", "https://test.local/mi-parcours");
        await cohorteService.ValiderEtapeAsync(cohorteId.Value, "gestionnaire-id", "https://test.local/parcours", "https://test.local/bibliotheque", "https://test.local/satisfaction", "https://test.local/mi-parcours", "https://test.local/attestation");

        var attestation = await attestationService.GetAttestationAsync(cohorteId.Value, etranger.Id);

        Assert.Null(attestation);
    }

    [Fact]
    public async Task GetAttestationAsync_UneFoisTerminee_ListeLesCartesEtAgregeLesHeuresDesEmargementsSignes()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var userManager = TestUserManagerFactory.Create(dbContext);
        var emailService = new FakeEmailService();
        var cohorteService = new CohorteService(dbContext, userManager, emailService, new PreuveService(dbContext, userManager, new FakePreuveFichierStockageService(), new NotificationService(dbContext), new FakeEmailService()), new NotificationService(dbContext));
        var emargementService = new EmargementService(dbContext, emailService, new FakePreuveFichierStockageService());
        var visioService = new VisioService(dbContext, emailService);
        var attestationService = new AttestationService(dbContext);

        var (challenge, _, cartes) = await PreparerChallengePublieAsync(dbContext, nombreEtapes: 1);

        var gestionnaire = new ApplicationUser { UserName = "coach@test.local", Email = "coach@test.local", Prenom = "Coach", Nom = "Test" };
        var apprenant = new ApplicationUser { UserName = "apprenant@test.local", Email = "apprenant@test.local", Prenom = "Jeanne", Nom = "Dupont" };
        dbContext.Users.AddRange(gestionnaire, apprenant);
        await dbContext.SaveChangesAsync();

        var (_, _, cohorteId) = await cohorteService.CreateAsync(new CohorteInput { ChallengeId = challenge.Id, Nom = "Cohorte Test" });
        await cohorteService.AjouterMembreManuelAsync(cohorteId!.Value, apprenant.Id);
        await cohorteService.LancerAsync(cohorteId.Value, gestionnaire.Id, "https://test.local/parcours", "https://test.local/mi-parcours");

        // La date de seance emargee reprend celle de la visio planifiee pour l'etape :
        // impossible d'envoyer des emargements sans l'avoir fixee au prealable, cf.
        // EmargementService.EnvoyerEmargementsEtapeCouranteAsync.
        var (visioSuccess, visioError) = await visioService.PlanifierAsync(cohorteId.Value, gestionnaire.Id, DateTime.UtcNow, "https://meet.test.local/seance");
        Assert.True(visioSuccess, visioError);

        // Emargement envoye puis signe par l'apprenant avant la cloture de l'etape (le
        // bouton "Envoyer les emargements" peut etre utilise a tout moment pendant que
        // l'etape est active, cf. EmargementService).
        var (envoiSuccess, envoiError) = await emargementService.EnvoyerEmargementsEtapeCouranteAsync(cohorteId.Value, _ => "https://test.local/emargement");
        Assert.True(envoiSuccess, envoiError);

        var aSigner = await emargementService.GetPourSignatureAsync(cohorteId.Value, apprenant.Id);
        Assert.NotNull(aSigner);
        var emargementIds = aSigner!.Cartes.Select(c => c.EmargementId).ToList();
        var signaturePng = new byte[] { 1, 2, 3, 4 };
        var (signatureSuccess, signatureError) = await emargementService.SignerAsync(cohorteId.Value, apprenant.Id, emargementIds, heuresPresence: 1.5m, heuresTravailPersonnel: 2m, signaturePng);
        Assert.True(signatureSuccess, signatureError);

        await cohorteService.ValiderEtapeAsync(cohorteId.Value, gestionnaire.Id, "https://test.local/parcours", "https://test.local/bibliotheque", "https://test.local/satisfaction", "https://test.local/mi-parcours", "https://test.local/attestation");

        var attestation = await attestationService.GetAttestationAsync(cohorteId.Value, apprenant.Id);

        Assert.NotNull(attestation);
        Assert.Equal("Challenge Test", attestation!.ChallengeTitre);
        Assert.Equal("Jeanne Dupont", attestation.BeneficiaireNomComplet);
        Assert.Equal(1, attestation.NombreEtapesValidees);
        Assert.Equal(1, attestation.NombreEtapesTotal);
        Assert.Contains(cartes[0].TitreTheorie, attestation.CartesAcquises);
        Assert.True(attestation.HeuresDisponibles);
        Assert.Equal(1.5m, attestation.TotalHeuresPresence);
        Assert.Equal(2m, attestation.TotalHeuresTravailPersonnel);
    }

    [Fact]
    public async Task GetAttestationAsync_SigneSansHeuresRenseignees_ListeLesCartesMaisSansHeures()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var userManager = TestUserManagerFactory.Create(dbContext);
        var emailService = new FakeEmailService();
        var cohorteService = new CohorteService(dbContext, userManager, emailService, new PreuveService(dbContext, userManager, new FakePreuveFichierStockageService(), new NotificationService(dbContext), new FakeEmailService()), new NotificationService(dbContext));
        var emargementService = new EmargementService(dbContext, emailService, new FakePreuveFichierStockageService());
        var visioService = new VisioService(dbContext, emailService);
        var attestationService = new AttestationService(dbContext);

        var (challenge, _, cartes) = await PreparerChallengePublieAsync(dbContext, nombreEtapes: 1);

        var gestionnaire = new ApplicationUser { UserName = "coach@test.local", Email = "coach@test.local" };
        var apprenant = new ApplicationUser { UserName = "apprenant@test.local", Email = "apprenant@test.local" };
        dbContext.Users.AddRange(gestionnaire, apprenant);
        await dbContext.SaveChangesAsync();

        var (_, _, cohorteId) = await cohorteService.CreateAsync(new CohorteInput { ChallengeId = challenge.Id, Nom = "Cohorte Test" });
        await cohorteService.AjouterMembreManuelAsync(cohorteId!.Value, apprenant.Id);
        await cohorteService.LancerAsync(cohorteId.Value, gestionnaire.Id, "https://test.local/parcours", "https://test.local/mi-parcours");

        // Emargement signe, mais sans renseigner les heures (facultatives) - un membre peut
        // signer sans avoir de volume horaire a declarer, cf. IEmargementService.SignerAsync.
        await visioService.PlanifierAsync(cohorteId.Value, gestionnaire.Id, DateTime.UtcNow, "https://meet.test.local/seance");
        await emargementService.EnvoyerEmargementsEtapeCouranteAsync(cohorteId.Value, _ => "https://test.local/emargement");
        var aSigner = await emargementService.GetPourSignatureAsync(cohorteId.Value, apprenant.Id);
        var emargementIds = aSigner!.Cartes.Select(c => c.EmargementId).ToList();
        await emargementService.SignerAsync(cohorteId.Value, apprenant.Id, emargementIds, null, null, [1, 2, 3]);

        await cohorteService.ValiderEtapeAsync(cohorteId.Value, gestionnaire.Id, "https://test.local/parcours", "https://test.local/bibliotheque", "https://test.local/satisfaction", "https://test.local/mi-parcours", "https://test.local/attestation");

        var attestation = await attestationService.GetAttestationAsync(cohorteId.Value, apprenant.Id);

        Assert.NotNull(attestation);
        Assert.Contains(cartes[0].TitreTheorie, attestation!.CartesAcquises);
        Assert.False(attestation.HeuresDisponibles);
        Assert.Equal(0m, attestation.TotalHeuresPresence);
    }
}

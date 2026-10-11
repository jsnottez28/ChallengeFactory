using Application.Common.Interfaces;
using Domain.Entities;
using Integration.TestSupport;
using Microsoft.EntityFrameworkCore;
using Web.Data;
using Web.Services;
using static Integration.TestSupport.EmargementTestHelper;

namespace Integration.Services;

public class CohorteServiceTests
{
    private static async Task<(Challenge Challenge, List<ChallengeEtape> Etapes, List<CarteCompetence> Cartes)> PreparerChallengePublieAsync(
        ApplicationDbContext dbContext, int nombreEtapes = 2, ModePlateforme mode = ModePlateforme.BtoC, FormatChallenge format = FormatChallenge.Collectif, string codePrefix = "CODE")
    {
        var challengeService = new ChallengeService(dbContext);
        var carteService = new CarteCompetenceService(dbContext);

        var (_, _, challenge) = await challengeService.CreateAsync(new ChallengeInput
        {
            Titre = "Challenge Test",
            NombreEtapes = nombreEtapes,
            Mode = mode,
            Format = format,
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
                Code = $"{codePrefix}-{i}",
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

    [Fact]
    public async Task CreateAsync_Echoue_SiLeChallengeEstEnBrouillon()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var challengeService = new ChallengeService(dbContext);
        var cohorteService = new CohorteService(dbContext, TestUserManagerFactory.Create(dbContext), new FakeEmailService(), new PreuveService(dbContext, TestUserManagerFactory.Create(dbContext), new FakePreuveFichierStockageService(), new NotificationService(dbContext), new FakeEmailService()), new NotificationService(dbContext));

        var (_, _, challenge) = await challengeService.CreateAsync(new ChallengeInput { Titre = "Brouillon", Mode = ModePlateforme.BtoC });

        var (success, errorMessage, _) = await cohorteService.CreateAsync(new CohorteInput { ChallengeId = challenge!.Id, Nom = "Cohorte X" });

        Assert.False(success);
        Assert.NotNull(errorMessage);
    }

    [Fact]
    public async Task LancerAsync_FermeInscriptions_PasseActive_AttribueEtape1_EtNotifieLesMembres()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var userManager = TestUserManagerFactory.Create(dbContext);
        var emailService = new FakeEmailService();
        var cohorteService = new CohorteService(dbContext, userManager, emailService, new PreuveService(dbContext, userManager, new FakePreuveFichierStockageService(), new NotificationService(dbContext), new FakeEmailService()), new NotificationService(dbContext));

        var (challenge, etapes, cartes) = await PreparerChallengePublieAsync(dbContext);

        var gestionnaire = new ApplicationUser { UserName = "coach@test.local", Email = "coach@test.local" };
        var apprenant = new ApplicationUser { UserName = "apprenant@test.local", Email = "apprenant@test.local" };
        dbContext.Users.AddRange(gestionnaire, apprenant);
        await dbContext.SaveChangesAsync();

        var (_, _, cohorteId) = await cohorteService.CreateAsync(new CohorteInput { ChallengeId = challenge.Id, Nom = "Cohorte Test" });
        await cohorteService.AjouterMembreManuelAsync(cohorteId!.Value, apprenant.Id);

        var (success, errorMessage) = await cohorteService.LancerAsync(cohorteId.Value, gestionnaire.Id, "https://test.local/parcours", "https://test.local/mi-parcours");

        Assert.True(success, errorMessage);

        var cohorte = await dbContext.Cohortes.FirstAsync(c => c.Id == cohorteId.Value);
        Assert.Equal(StatutCohorte.Active, cohorte.Statut);
        Assert.Equal(1, cohorte.EtapeCourante);

        var attribution = await dbContext.CarteAttributions.SingleAsync(a =>
            a.CarteCompetenceId == cartes[0].Id && a.UtilisateurId == apprenant.Id);
        Assert.Equal(OrigineAttribution.Challenge, attribution.OrigineType);
        Assert.Equal(cohorteId.Value, attribution.CohorteId);
        Assert.Equal(etapes[0].Id, attribution.ChallengeEtapeId);

        // 2 emails : le lancement (bienvenue + explication du principe, distinct de
        // "Nouvelle étape" - cf. ChallengeEmailTemplates.LancementParcours), et le point
        // d'étape mi-parcours (le Challenge de test a 2 étapes, donc l'étape 1 est déjà
        // l'étape médiane - cf. CohorteService.EnvoyerQuestionnaireMiParcoursSiEtapeMedianeAsync).
        Assert.Equal(2, emailService.Envois.Count);
        Assert.All(emailService.Envois, e => Assert.Equal(apprenant.Email, e.Destinataire));
        Assert.Single(emailService.Envois, e => e.Sujet.Contains("démarre"));
        Assert.Single(emailService.Envois, e => e.Sujet.Contains("mi-parcours"));
    }

    [Fact]
    public async Task ValiderEtapeAsync_AvanceEtapeCouranteEtAttribueLaCarteSuivante_EtNotifie()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var userManager = TestUserManagerFactory.Create(dbContext);
        var emailService = new FakeEmailService();
        var cohorteService = new CohorteService(dbContext, userManager, emailService, new PreuveService(dbContext, userManager, new FakePreuveFichierStockageService(), new NotificationService(dbContext), new FakeEmailService()), new NotificationService(dbContext));

        var (challenge, etapes, cartes) = await PreparerChallengePublieAsync(dbContext, nombreEtapes: 2);

        var gestionnaire = new ApplicationUser { UserName = "coach@test.local", Email = "coach@test.local" };
        var apprenant = new ApplicationUser { UserName = "apprenant@test.local", Email = "apprenant@test.local" };
        dbContext.Users.AddRange(gestionnaire, apprenant);
        await dbContext.SaveChangesAsync();

        var (_, _, cohorteId) = await cohorteService.CreateAsync(new CohorteInput { ChallengeId = challenge.Id, Nom = "Cohorte Test" });
        await cohorteService.AjouterMembreManuelAsync(cohorteId!.Value, apprenant.Id);
        await cohorteService.LancerAsync(cohorteId.Value, gestionnaire.Id, "https://test.local/parcours", "https://test.local/mi-parcours");
        await SignerTousLesEmargementsEtapeCouranteAsync(dbContext, emailService, cohorteId.Value, gestionnaire.Id, apprenant);
        emailService.Envois.Clear();

        var (success, errorMessage) = await cohorteService.ValiderEtapeAsync(cohorteId.Value, gestionnaire.Id, "https://test.local/parcours", "https://test.local/bibliotheque", "https://test.local/satisfaction", "https://test.local/mi-parcours", "https://test.local/attestation");

        Assert.True(success, errorMessage);

        var cohorte = await dbContext.Cohortes.FirstAsync(c => c.Id == cohorteId.Value);
        Assert.Equal(2, cohorte.EtapeCourante);
        Assert.Equal(StatutCohorte.Active, cohorte.Statut);

        var validation = await dbContext.CohorteEtapeValidations.SingleAsync(v => v.CohorteId == cohorteId.Value);
        Assert.Equal(1, validation.NumeroEtape);
        Assert.Equal(gestionnaire.Id, validation.ValideParId);

        var attributionEtape2 = await dbContext.CarteAttributions.SingleAsync(a =>
            a.CarteCompetenceId == cartes[1].Id && a.UtilisateurId == apprenant.Id);
        Assert.Equal(etapes[1].Id, attributionEtape2.ChallengeEtapeId);

        var email = Assert.Single(emailService.Envois);
        Assert.Contains("Nouvelle étape", email.Sujet);
    }

    [Fact]
    public async Task ValiderEtapeAsync_Echoue_TantQueTousLesMembresNOntPasSigneLeurEmargement()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var userManager = TestUserManagerFactory.Create(dbContext);
        var emailService = new FakeEmailService();
        var cohorteService = new CohorteService(dbContext, userManager, emailService, new PreuveService(dbContext, userManager, new FakePreuveFichierStockageService(), new NotificationService(dbContext), new FakeEmailService()), new NotificationService(dbContext));

        var (challenge, _, _) = await PreparerChallengePublieAsync(dbContext, nombreEtapes: 2);

        var gestionnaire = new ApplicationUser { UserName = "coach@test.local", Email = "coach@test.local" };
        var premierMembre = new ApplicationUser { UserName = "premier@test.local", Email = "premier@test.local" };
        var secondMembre = new ApplicationUser { UserName = "second@test.local", Email = "second@test.local" };
        dbContext.Users.AddRange(gestionnaire, premierMembre, secondMembre);
        await dbContext.SaveChangesAsync();

        var (_, _, cohorteId) = await cohorteService.CreateAsync(new CohorteInput { ChallengeId = challenge.Id, Nom = "Cohorte Test" });
        await cohorteService.AjouterMembreManuelAsync(cohorteId!.Value, premierMembre.Id);
        await cohorteService.AjouterMembreManuelAsync(cohorteId.Value, secondMembre.Id);
        await cohorteService.LancerAsync(cohorteId.Value, gestionnaire.Id, "https://test.local/parcours", "https://test.local/mi-parcours");

        // Aucun emargement envoye du tout : refuse.
        var (successSansEnvoi, erreurSansEnvoi) = await cohorteService.ValiderEtapeAsync(cohorteId.Value, gestionnaire.Id, "https://test.local/parcours", "https://test.local/bibliotheque", "https://test.local/satisfaction", "https://test.local/mi-parcours", "https://test.local/attestation");
        Assert.False(successSansEnvoi);
        Assert.Contains("émargement", erreurSansEnvoi);

        // Un seul des deux membres a signe : toujours refuse.
        await SignerTousLesEmargementsEtapeCouranteAsync(dbContext, emailService, cohorteId.Value, gestionnaire.Id, premierMembre);
        var (successPartiel, erreurPartiel) = await cohorteService.ValiderEtapeAsync(cohorteId.Value, gestionnaire.Id, "https://test.local/parcours", "https://test.local/bibliotheque", "https://test.local/satisfaction", "https://test.local/mi-parcours", "https://test.local/attestation");
        Assert.False(successPartiel);
        Assert.Contains("émargement", erreurPartiel);

        var cohorteEncoreEtape1 = await dbContext.Cohortes.FirstAsync(c => c.Id == cohorteId.Value);
        Assert.Equal(1, cohorteEncoreEtape1.EtapeCourante);

        // Les deux ont signe : la validation reussit.
        await SignerTousLesEmargementsEtapeCouranteAsync(dbContext, emailService, cohorteId.Value, gestionnaire.Id, secondMembre);
        var (successComplet, erreurComplet) = await cohorteService.ValiderEtapeAsync(cohorteId.Value, gestionnaire.Id, "https://test.local/parcours", "https://test.local/bibliotheque", "https://test.local/satisfaction", "https://test.local/mi-parcours", "https://test.local/attestation");
        Assert.True(successComplet, erreurComplet);

        var cohorteAvancee = await dbContext.Cohortes.FirstAsync(c => c.Id == cohorteId.Value);
        Assert.Equal(2, cohorteAvancee.EtapeCourante);
    }

    [Fact]
    public async Task ValiderEtapeAsync_SurLaDerniereEtape_ClotureLaCohorteEtEnvoieUnEmailDeClotureDistinct()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var userManager = TestUserManagerFactory.Create(dbContext);
        var emailService = new FakeEmailService();
        var cohorteService = new CohorteService(dbContext, userManager, emailService, new PreuveService(dbContext, userManager, new FakePreuveFichierStockageService(), new NotificationService(dbContext), new FakeEmailService()), new NotificationService(dbContext));

        var (challenge, _, _) = await PreparerChallengePublieAsync(dbContext, nombreEtapes: 1);

        var gestionnaire = new ApplicationUser { UserName = "coach@test.local", Email = "coach@test.local" };
        var apprenant = new ApplicationUser { UserName = "apprenant@test.local", Email = "apprenant@test.local" };
        dbContext.Users.AddRange(gestionnaire, apprenant);
        await dbContext.SaveChangesAsync();

        var (_, _, cohorteId) = await cohorteService.CreateAsync(new CohorteInput { ChallengeId = challenge.Id, Nom = "Cohorte Test" });
        await cohorteService.AjouterMembreManuelAsync(cohorteId!.Value, apprenant.Id);
        await cohorteService.LancerAsync(cohorteId.Value, gestionnaire.Id, "https://test.local/parcours", "https://test.local/mi-parcours");
        await SignerTousLesEmargementsEtapeCouranteAsync(dbContext, emailService, cohorteId.Value, gestionnaire.Id, apprenant);
        emailService.Envois.Clear();

        var (success, errorMessage) = await cohorteService.ValiderEtapeAsync(cohorteId.Value, gestionnaire.Id, "https://test.local/parcours", "https://test.local/bibliotheque", "https://test.local/satisfaction", "https://test.local/mi-parcours", "https://test.local/attestation");

        Assert.True(success, errorMessage);

        var cohorte = await dbContext.Cohortes.FirstAsync(c => c.Id == cohorteId.Value);
        Assert.Equal(StatutCohorte.Terminee, cohorte.Statut);

        // La cloture envoie desormais 2 emails distincts : la felicitation de cloture, et
        // la demande de satisfaction (critere 7 Qualiopi) - cf. NotifierDemandeSatisfactionAsync.
        Assert.Equal(2, emailService.Envois.Count);
        Assert.All(emailService.Envois, email => Assert.Equal(apprenant.Email, email.Destinataire));

        var emailCloture = Assert.Single(emailService.Envois, e => e.Sujet.Contains("terminé"));
        Assert.DoesNotContain("Nouvelle étape", emailCloture.Sujet);
        Assert.Contains($"https://test.local/attestation?cohorteId={cohorteId.Value}", emailCloture.CorpsHtml);

        var emailSatisfaction = Assert.Single(emailService.Envois, e => e.Sujet.Contains("Votre avis compte"));
        Assert.Contains("https://test.local/satisfaction", emailSatisfaction.CorpsHtml);
    }

    [Fact]
    public async Task LancerAsync_NeDupliquePasUneAttributionDejaExistante()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var userManager = TestUserManagerFactory.Create(dbContext);
        var cohorteService = new CohorteService(dbContext, userManager, new FakeEmailService(), new PreuveService(dbContext, userManager, new FakePreuveFichierStockageService(), new NotificationService(dbContext), new FakeEmailService()), new NotificationService(dbContext));

        var (challenge, etapes, cartes) = await PreparerChallengePublieAsync(dbContext, nombreEtapes: 1);

        var gestionnaire = new ApplicationUser { UserName = "coach@test.local", Email = "coach@test.local" };
        var apprenant = new ApplicationUser { UserName = "apprenant@test.local", Email = "apprenant@test.local" };
        dbContext.Users.AddRange(gestionnaire, apprenant);
        await dbContext.SaveChangesAsync();

        var (_, _, cohorteId) = await cohorteService.CreateAsync(new CohorteInput { ChallengeId = challenge.Id, Nom = "Cohorte Test" });
        await cohorteService.AjouterMembreManuelAsync(cohorteId!.Value, apprenant.Id);

        // Simule une ligne d'attribution deja existante pour la meme (carte, utilisateur,
        // cohorte, etape) - par ex. issue d'une tentative precedente - avant meme que
        // LancerAsync ne s'execute.
        dbContext.CarteAttributions.Add(new CarteAttribution
        {
            CarteCompetenceId = cartes[0].Id,
            UtilisateurId = apprenant.Id,
            AttribueParId = gestionnaire.Id,
            AttribueLe = DateTime.UtcNow,
            EstActif = true,
            OrigineType = OrigineAttribution.Challenge,
            CohorteId = cohorteId.Value,
            ChallengeEtapeId = etapes[0].Id,
        });
        await dbContext.SaveChangesAsync();

        var (success, errorMessage) = await cohorteService.LancerAsync(cohorteId.Value, gestionnaire.Id, "https://test.local/parcours", "https://test.local/mi-parcours");
        Assert.True(success, errorMessage);

        var nombreAttributions = await dbContext.CarteAttributions.CountAsync(a =>
            a.CarteCompetenceId == cartes[0].Id && a.UtilisateurId == apprenant.Id
            && a.CohorteId == cohorteId.Value && a.ChallengeEtapeId == etapes[0].Id);

        Assert.Equal(1, nombreAttributions);
    }

    [Fact]
    public async Task MembreAjouteApresLancement_RecoitLesCartesDesEtapesDejaValideesJusquALaCourante()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var userManager = TestUserManagerFactory.Create(dbContext);
        var emailService = new FakeEmailService();
        var cohorteService = new CohorteService(dbContext, userManager, emailService, new PreuveService(dbContext, userManager, new FakePreuveFichierStockageService(), new NotificationService(dbContext), new FakeEmailService()), new NotificationService(dbContext));

        var (challenge, etapes, cartes) = await PreparerChallengePublieAsync(dbContext, nombreEtapes: 3);

        var gestionnaire = new ApplicationUser { UserName = "coach@test.local", Email = "coach@test.local" };
        var premierMembre = new ApplicationUser { UserName = "premier@test.local", Email = "premier@test.local" };
        var retardataire = new ApplicationUser { UserName = "retardataire@test.local", Email = "retardataire@test.local" };
        dbContext.Users.AddRange(gestionnaire, premierMembre, retardataire);
        await dbContext.SaveChangesAsync();

        var (_, _, cohorteId) = await cohorteService.CreateAsync(new CohorteInput { ChallengeId = challenge.Id, Nom = "Cohorte Test" });
        await cohorteService.AjouterMembreManuelAsync(cohorteId!.Value, premierMembre.Id);

        // Lance (etape 1) puis valide une fois (passe a l'etape 2) avant l'arrivee du
        // retardataire : etapes 1 et 2 sont "deja validees jusqu'a l'etape courante".
        await cohorteService.LancerAsync(cohorteId.Value, gestionnaire.Id, "https://test.local/parcours", "https://test.local/mi-parcours");
        await SignerTousLesEmargementsEtapeCouranteAsync(dbContext, emailService, cohorteId.Value, gestionnaire.Id, premierMembre);
        await cohorteService.ValiderEtapeAsync(cohorteId.Value, gestionnaire.Id, "https://test.local/parcours", "https://test.local/bibliotheque", "https://test.local/satisfaction", "https://test.local/mi-parcours", "https://test.local/attestation");

        await cohorteService.AjouterMembreManuelAsync(cohorteId.Value, retardataire.Id);

        var attributionsRetardataire = await dbContext.CarteAttributions
            .Where(a => a.UtilisateurId == retardataire.Id)
            .ToListAsync();

        Assert.Equal(2, attributionsRetardataire.Count);
        Assert.Contains(attributionsRetardataire, a => a.CarteCompetenceId == cartes[0].Id && a.ChallengeEtapeId == etapes[0].Id);
        Assert.Contains(attributionsRetardataire, a => a.CarteCompetenceId == cartes[1].Id && a.ChallengeEtapeId == etapes[1].Id);
        Assert.DoesNotContain(attributionsRetardataire, a => a.ChallengeEtapeId == etapes[2].Id);
    }

    [Fact]
    public async Task ImporterMembresAsync_CreeUnCompteSansMotDePasse_EtEnvoieUneInvitation()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var userManager = TestUserManagerFactory.Create(dbContext);
        var emailService = new FakeEmailService();
        var cohorteService = new CohorteService(dbContext, userManager, emailService, new PreuveService(dbContext, userManager, new FakePreuveFichierStockageService(), new NotificationService(dbContext), new FakeEmailService()), new NotificationService(dbContext));

        var (challenge, _, _) = await PreparerChallengePublieAsync(dbContext, nombreEtapes: 1, mode: ModePlateforme.BtoB);

        var gestionnaire = new ApplicationUser { UserName = "coach@test.local", Email = "coach@test.local" };
        dbContext.Users.Add(gestionnaire);
        var organisation = new Web.Data.Entities.Organisation { CodeAdherent = "ORG-1", RaisonSociale = "Entreprise Test" };
        dbContext.Organisations.Add(organisation);
        await dbContext.SaveChangesAsync();

        var (createSuccess, createError, cohorteId) = await cohorteService.CreateAsync(new CohorteInput { ChallengeId = challenge.Id, Nom = "Cohorte Import", OrganisationId = organisation.Id });
        Assert.True(createSuccess, createError);

        var rapport = await cohorteService.ImporterMembresAsync(
            cohorteId!.Value,
            [new MembreImportInput { Email = "nouveau@entreprise.fr", Prenom = "Jean", Nom = "Dupont" }],
            gestionnaire.Id,
            token => $"https://test.local/definir-mot-de-passe?token={token}");

        Assert.Equal(1, rapport.ComptesCrees);
        Assert.Empty(rapport.Erreurs);

        var nouveauCompte = await userManager.FindByEmailAsync("nouveau@entreprise.fr");
        Assert.NotNull(nouveauCompte);
        Assert.Null(nouveauCompte!.PasswordHash);
        Assert.True(nouveauCompte.EmailConfirmed);
        Assert.Equal(ModePlateforme.BtoB, nouveauCompte.Mode);

        var estMembre = await dbContext.CohorteMembres.AnyAsync(m => m.CohorteId == cohorteId.Value && m.UtilisateurId == nouveauCompte.Id && m.MethodeAjout == MethodeAjoutMembre.Import);
        Assert.True(estMembre);

        var invitation = await dbContext.InvitationsComptes.SingleAsync(i => i.UtilisateurId == nouveauCompte.Id);
        Assert.True(invitation.EstActif);
        Assert.Null(invitation.UtiliseLe);

        var email = Assert.Single(emailService.Envois);
        Assert.Equal("nouveau@entreprise.fr", email.Destinataire);
        Assert.Contains(invitation.Token, email.CorpsHtml);
    }

    [Fact]
    public async Task ImporterMembresAsync_RattacheDirectementUnCompteDejaExistant_SansRecreerNiReenvoyerDInvitation()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var userManager = TestUserManagerFactory.Create(dbContext);
        var emailService = new FakeEmailService();
        var cohorteService = new CohorteService(dbContext, userManager, emailService, new PreuveService(dbContext, userManager, new FakePreuveFichierStockageService(), new NotificationService(dbContext), new FakeEmailService()), new NotificationService(dbContext));

        var (challenge, _, _) = await PreparerChallengePublieAsync(dbContext, nombreEtapes: 1, mode: ModePlateforme.BtoB);

        var gestionnaire = new ApplicationUser { UserName = "coach@test.local", Email = "coach@test.local" };
        var compteExistant = new ApplicationUser
        {
            UserName = "existant@entreprise.fr",
            Email = "existant@entreprise.fr",
            NormalizedEmail = "EXISTANT@ENTREPRISE.FR",
            NormalizedUserName = "EXISTANT@ENTREPRISE.FR",
        };
        dbContext.Users.AddRange(gestionnaire, compteExistant);
        var organisation = new Web.Data.Entities.Organisation { CodeAdherent = "ORG-1", RaisonSociale = "Entreprise Test" };
        dbContext.Organisations.Add(organisation);
        await dbContext.SaveChangesAsync();

        var (createSuccess, createError, cohorteId) = await cohorteService.CreateAsync(new CohorteInput { ChallengeId = challenge.Id, Nom = "Cohorte Import", OrganisationId = organisation.Id });
        Assert.True(createSuccess, createError);

        var rapport = await cohorteService.ImporterMembresAsync(
            cohorteId!.Value,
            [new MembreImportInput { Email = "existant@entreprise.fr" }],
            gestionnaire.Id,
            token => $"https://test.local/definir-mot-de-passe?token={token}");

        Assert.Equal(0, rapport.ComptesCrees);
        Assert.Equal(1, rapport.ComptesExistantsRattaches);
        Assert.Empty(emailService.Envois);

        var estMembre = await dbContext.CohorteMembres.AnyAsync(m => m.CohorteId == cohorteId.Value && m.UtilisateurId == compteExistant.Id);
        Assert.True(estMembre);
    }

    [Fact]
    public async Task SupprimerAsync_Reussit_SiCohorteEnPreparation_EtRetireAussiSesMembres()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var userManager = TestUserManagerFactory.Create(dbContext);
        var cohorteService = new CohorteService(dbContext, userManager, new FakeEmailService(), new PreuveService(dbContext, userManager, new FakePreuveFichierStockageService(), new NotificationService(dbContext), new FakeEmailService()), new NotificationService(dbContext));

        var (challenge, _, _) = await PreparerChallengePublieAsync(dbContext, nombreEtapes: 1);
        var apprenant = new ApplicationUser { UserName = "apprenant@test.local", Email = "apprenant@test.local" };
        dbContext.Users.Add(apprenant);
        await dbContext.SaveChangesAsync();

        var (_, _, cohorteId) = await cohorteService.CreateAsync(new CohorteInput { ChallengeId = challenge.Id, Nom = "Cohorte Test" });
        await cohorteService.AjouterMembreManuelAsync(cohorteId!.Value, apprenant.Id);

        var (success, errorMessage) = await cohorteService.SupprimerAsync(cohorteId.Value);

        Assert.True(success, errorMessage);
        Assert.Null(await cohorteService.GetResumeAsync(cohorteId.Value));
        Assert.False(await dbContext.CohorteMembres.AnyAsync(m => m.CohorteId == cohorteId.Value));
    }

    [Fact]
    public async Task SupprimerAsync_Echoue_SiCohorteDejaLancee()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var userManager = TestUserManagerFactory.Create(dbContext);
        var cohorteService = new CohorteService(dbContext, userManager, new FakeEmailService(), new PreuveService(dbContext, userManager, new FakePreuveFichierStockageService(), new NotificationService(dbContext), new FakeEmailService()), new NotificationService(dbContext));

        var (challenge, _, _) = await PreparerChallengePublieAsync(dbContext, nombreEtapes: 1);
        var gestionnaire = new ApplicationUser { UserName = "coach@test.local", Email = "coach@test.local" };
        dbContext.Users.Add(gestionnaire);
        await dbContext.SaveChangesAsync();

        var (_, _, cohorteId) = await cohorteService.CreateAsync(new CohorteInput { ChallengeId = challenge.Id, Nom = "Cohorte Test" });
        await cohorteService.LancerAsync(cohorteId!.Value, gestionnaire.Id, "https://test.local/parcours", "https://test.local/mi-parcours");

        var (success, errorMessage) = await cohorteService.SupprimerAsync(cohorteId.Value);

        Assert.False(success);
        Assert.NotNull(errorMessage);
        Assert.NotNull(await cohorteService.GetResumeAsync(cohorteId.Value));
    }

    [Fact]
    public async Task ValiderEtapeAsync_EnvoieLeQuestionnaireMiParcours_UniquementALEtapeMediane()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var userManager = TestUserManagerFactory.Create(dbContext);
        var emailService = new FakeEmailService();
        var cohorteService = new CohorteService(dbContext, userManager, emailService, new PreuveService(dbContext, userManager, new FakePreuveFichierStockageService(), new NotificationService(dbContext), new FakeEmailService()), new NotificationService(dbContext));

        // Parcours a 10 etapes (comme un bilan de competences type) : etape mediane = 5.
        var (challenge, _, _) = await PreparerChallengePublieAsync(dbContext, nombreEtapes: 10);

        var gestionnaire = new ApplicationUser { UserName = "coach@test.local", Email = "coach@test.local" };
        var apprenant = new ApplicationUser { UserName = "apprenant@test.local", Email = "apprenant@test.local" };
        dbContext.Users.AddRange(gestionnaire, apprenant);
        await dbContext.SaveChangesAsync();

        var (_, _, cohorteId) = await cohorteService.CreateAsync(new CohorteInput { ChallengeId = challenge.Id, Nom = "Cohorte Test" });
        await cohorteService.AjouterMembreManuelAsync(cohorteId!.Value, apprenant.Id);

        await cohorteService.LancerAsync(cohorteId.Value, gestionnaire.Id, "https://test.local/parcours", "https://test.local/mi-parcours");
        // Etapes 1 -> 4 : aucun questionnaire mi-parcours ne doit partir.
        for (var i = 0; i < 3; i++)
        {
            await SignerTousLesEmargementsEtapeCouranteAsync(dbContext, emailService, cohorteId.Value, gestionnaire.Id, apprenant);
            await cohorteService.ValiderEtapeAsync(cohorteId.Value, gestionnaire.Id, "https://test.local/parcours", "https://test.local/bibliotheque", "https://test.local/satisfaction", "https://test.local/mi-parcours", "https://test.local/attestation");
        }
        Assert.DoesNotContain(emailService.Envois, e => e.Sujet.Contains("mi-parcours"));

        emailService.Envois.Clear();

        // Passage a l'etape 5 : le questionnaire doit partir, une seule fois.
        await SignerTousLesEmargementsEtapeCouranteAsync(dbContext, emailService, cohorteId.Value, gestionnaire.Id, apprenant);
        var (success, errorMessage) = await cohorteService.ValiderEtapeAsync(cohorteId.Value, gestionnaire.Id, "https://test.local/parcours", "https://test.local/bibliotheque", "https://test.local/satisfaction", "https://test.local/mi-parcours", "https://test.local/attestation");
        Assert.True(success, errorMessage);

        var emailMiParcours = Assert.Single(emailService.Envois, e => e.Sujet.Contains("mi-parcours"));
        Assert.Equal(apprenant.Email, emailMiParcours.Destinataire);
        Assert.Contains($"https://test.local/mi-parcours?cohorteId={cohorteId.Value}", emailMiParcours.CorpsHtml);

        emailService.Envois.Clear();

        // Etape 6 : plus aucun questionnaire mi-parcours ne doit repartir.
        await SignerTousLesEmargementsEtapeCouranteAsync(dbContext, emailService, cohorteId.Value, gestionnaire.Id, apprenant);
        await cohorteService.ValiderEtapeAsync(cohorteId.Value, gestionnaire.Id, "https://test.local/parcours", "https://test.local/bibliotheque", "https://test.local/satisfaction", "https://test.local/mi-parcours", "https://test.local/attestation");
        Assert.DoesNotContain(emailService.Envois, e => e.Sujet.Contains("mi-parcours"));
    }

    // ---- Personnalisation des cartes (parcours Bilan de competences individuel uniquement) ----

    private static CohorteService CreerCohorteService(ApplicationDbContext dbContext)
    {
        var userManager = TestUserManagerFactory.Create(dbContext);
        return new CohorteService(dbContext, userManager, new FakeEmailService(),
            new PreuveService(dbContext, userManager, new FakePreuveFichierStockageService(), new NotificationService(dbContext), new FakeEmailService()),
            new NotificationService(dbContext));
    }

    [Fact]
    public async Task DefinirCartesSupplementairesMembreAsync_Echoue_SurUnChallengeCollectif()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var cohorteService = CreerCohorteService(dbContext);
        var carteService = new CarteCompetenceService(dbContext);

        var (challenge, etapes, _) = await PreparerChallengePublieAsync(dbContext, format: FormatChallenge.Collectif);
        var (_, _, carteSupplementaire) = await carteService.CreateAsync(new CarteCompetenceInput { Code = "SUP-1", Niveau = NiveauCarte.Debutant, TitreTheorie = "Carte supplémentaire" });

        var apprenant = new ApplicationUser { UserName = "apprenant@test.local", Email = "apprenant@test.local" };
        var coach = new ApplicationUser { UserName = "coach@test.local", Email = "coach@test.local" };
        dbContext.Users.AddRange(apprenant, coach);
        await dbContext.SaveChangesAsync();

        var (_, _, cohorteId) = await cohorteService.CreateAsync(new CohorteInput { ChallengeId = challenge.Id, Nom = "Cohorte Collective" });
        await cohorteService.AjouterMembreManuelAsync(cohorteId!.Value, apprenant.Id);
        var membre = await dbContext.CohorteMembres.SingleAsync(m => m.CohorteId == cohorteId.Value && m.UtilisateurId == apprenant.Id);

        var (success, errorMessage) = await cohorteService.DefinirCartesSupplementairesMembreAsync(membre.Id, etapes[0].Id, [carteSupplementaire!.Id], coach.Id);

        Assert.False(success);
        Assert.Contains("Bilan de compétences individuel", errorMessage);
        Assert.Empty(await dbContext.CohorteMembreCartesSupplementaires.ToListAsync());
    }

    [Fact]
    public async Task DefinirCartesSupplementairesMembreAsync_Reussit_SurUnBilanIndividuel_SansToucherAuTemplatePartage()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var cohorteService = CreerCohorteService(dbContext);
        var carteService = new CarteCompetenceService(dbContext);

        var (challenge, etapes, cartesTemplate) = await PreparerChallengePublieAsync(dbContext, format: FormatChallenge.BilanCompetencesIndividuel);
        var (_, _, carteA) = await carteService.CreateAsync(new CarteCompetenceInput { Code = "SUP-A", Niveau = NiveauCarte.Debutant, TitreTheorie = "Carte A" });
        var (_, _, carteB) = await carteService.CreateAsync(new CarteCompetenceInput { Code = "SUP-B", Niveau = NiveauCarte.Debutant, TitreTheorie = "Carte B" });

        var apprenant = new ApplicationUser { UserName = "apprenant@test.local", Email = "apprenant@test.local" };
        var coach = new ApplicationUser { UserName = "coach@test.local", Email = "coach@test.local" };
        dbContext.Users.AddRange(apprenant, coach);
        await dbContext.SaveChangesAsync();

        var (_, _, cohorteId) = await cohorteService.CreateAsync(new CohorteInput { ChallengeId = challenge.Id, Nom = "Bilan individuel" });
        await cohorteService.AjouterMembreManuelAsync(cohorteId!.Value, apprenant.Id);
        var membre = await dbContext.CohorteMembres.SingleAsync(m => m.CohorteId == cohorteId.Value && m.UtilisateurId == apprenant.Id);

        var (success1, errorMessage1) = await cohorteService.DefinirCartesSupplementairesMembreAsync(membre.Id, etapes[0].Id, [carteA!.Id, carteB!.Id], coach.Id);
        Assert.True(success1, errorMessage1);

        // Remplacement total : ne garder que carteB doit retirer carteA, jamais toucher au
        // template partage (ChallengeEtapeCarte) de l'etape.
        var (success2, errorMessage2) = await cohorteService.DefinirCartesSupplementairesMembreAsync(membre.Id, etapes[0].Id, [carteB.Id], coach.Id);
        Assert.True(success2, errorMessage2);

        var supplementaires = await dbContext.CohorteMembreCartesSupplementaires
            .Where(cs => cs.CohorteMembreId == membre.Id && cs.ChallengeEtapeId == etapes[0].Id)
            .ToListAsync();
        var carteRestante = Assert.Single(supplementaires);
        Assert.Equal(carteB.Id, carteRestante.CarteCompetenceId);
        Assert.Equal(coach.Id, carteRestante.AjouteeParId);

        var etapeRecue = await dbContext.ChallengeEtapes.Include(e => e.Cartes).SingleAsync(e => e.Id == etapes[0].Id);
        Assert.Equal(cartesTemplate[0].Id, Assert.Single(etapeRecue.Cartes).CarteCompetenceId);
    }

    [Fact]
    public async Task GetMesParcoursEnCoursAsync_SurfaceLesCartesPersonnalisees_PourLeBonMembreUniquement()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var cohorteService = CreerCohorteService(dbContext);
        var carteService = new CarteCompetenceService(dbContext);

        var (challenge, etapes, _) = await PreparerChallengePublieAsync(dbContext, format: FormatChallenge.BilanCompetencesIndividuel);
        var (_, _, cartePersonnalisee) = await carteService.CreateAsync(new CarteCompetenceInput { Code = "SUP-P", Niveau = NiveauCarte.Debutant, TitreTheorie = "Carte perso" });

        var gestionnaire = new ApplicationUser { UserName = "coach@test.local", Email = "coach@test.local" };
        var apprenantCible = new ApplicationUser { UserName = "cible@test.local", Email = "cible@test.local" };
        var autreApprenant = new ApplicationUser { UserName = "autre@test.local", Email = "autre@test.local" };
        dbContext.Users.AddRange(gestionnaire, apprenantCible, autreApprenant);
        await dbContext.SaveChangesAsync();

        var (_, _, cohorteId) = await cohorteService.CreateAsync(new CohorteInput { ChallengeId = challenge.Id, Nom = "Bilan individuel" });
        await cohorteService.AjouterMembreManuelAsync(cohorteId!.Value, apprenantCible.Id);
        await cohorteService.AjouterMembreManuelAsync(cohorteId.Value, autreApprenant.Id);
        await cohorteService.LancerAsync(cohorteId.Value, gestionnaire.Id, "https://test.local/parcours", "https://test.local/mi-parcours");

        var membreCible = await dbContext.CohorteMembres.SingleAsync(m => m.CohorteId == cohorteId.Value && m.UtilisateurId == apprenantCible.Id);
        await cohorteService.DefinirCartesSupplementairesMembreAsync(membreCible.Id, etapes[0].Id, [cartePersonnalisee!.Id], gestionnaire.Id);

        var parcoursCible = Assert.Single(await cohorteService.GetMesParcoursEnCoursAsync(apprenantCible.Id));
        Assert.Equal(cartePersonnalisee.Id, Assert.Single(parcoursCible.CartesPersonnalisees).Id);

        var parcoursAutre = Assert.Single(await cohorteService.GetMesParcoursEnCoursAsync(autreApprenant.Id));
        Assert.Empty(parcoursAutre.CartesPersonnalisees);
    }

    [Fact]
    public async Task DefinirInstructionsPersonnaliseesMembreAsync_Echoue_SurUnChallengeCollectif()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var cohorteService = CreerCohorteService(dbContext);

        var (challenge, etapes, _) = await PreparerChallengePublieAsync(dbContext, format: FormatChallenge.Collectif);

        var apprenant = new ApplicationUser { UserName = "apprenant@test.local", Email = "apprenant@test.local" };
        var coach = new ApplicationUser { UserName = "coach@test.local", Email = "coach@test.local" };
        dbContext.Users.AddRange(apprenant, coach);
        await dbContext.SaveChangesAsync();

        var (_, _, cohorteId) = await cohorteService.CreateAsync(new CohorteInput { ChallengeId = challenge.Id, Nom = "Cohorte Collective" });
        await cohorteService.AjouterMembreManuelAsync(cohorteId!.Value, apprenant.Id);
        var membre = await dbContext.CohorteMembres.SingleAsync(m => m.CohorteId == cohorteId.Value && m.UtilisateurId == apprenant.Id);

        var (success, errorMessage) = await cohorteService.DefinirInstructionsPersonnaliseesMembreAsync(membre.Id, etapes[0].Id, "Instructions sur-mesure", coach.Id);

        Assert.False(success);
        Assert.Contains("Bilan de compétences individuel", errorMessage);
        Assert.Empty(await dbContext.CohorteMembreEtapePersonnalisations.ToListAsync());
    }

    [Fact]
    public async Task DefinirInstructionsPersonnaliseesMembreAsync_RemplaceLeDefiIndividuel_PourCeMembreUniquement_SansToucherAuTemplate()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var cohorteService = CreerCohorteService(dbContext);

        var (challenge, etapes, _) = await PreparerChallengePublieAsync(dbContext, format: FormatChallenge.BilanCompetencesIndividuel);
        var defiPartageOriginal = etapes[0].DefiIndividuel;

        var gestionnaire = new ApplicationUser { UserName = "coach@test.local", Email = "coach@test.local" };
        var apprenantCible = new ApplicationUser { UserName = "cible@test.local", Email = "cible@test.local" };
        var autreApprenant = new ApplicationUser { UserName = "autre@test.local", Email = "autre@test.local" };
        dbContext.Users.AddRange(gestionnaire, apprenantCible, autreApprenant);
        await dbContext.SaveChangesAsync();

        var (_, _, cohorteId) = await cohorteService.CreateAsync(new CohorteInput { ChallengeId = challenge.Id, Nom = "Bilan individuel" });
        await cohorteService.AjouterMembreManuelAsync(cohorteId!.Value, apprenantCible.Id);
        await cohorteService.AjouterMembreManuelAsync(cohorteId.Value, autreApprenant.Id);
        await cohorteService.LancerAsync(cohorteId.Value, gestionnaire.Id, "https://test.local/parcours", "https://test.local/mi-parcours");

        var membreCible = await dbContext.CohorteMembres.SingleAsync(m => m.CohorteId == cohorteId.Value && m.UtilisateurId == apprenantCible.Id);

        var (success, errorMessage) = await cohorteService.DefinirInstructionsPersonnaliseesMembreAsync(membreCible.Id, etapes[0].Id, "Observe ton équipe lors du prochain point hebdo.", gestionnaire.Id);
        Assert.True(success, errorMessage);

        var parcoursCible = Assert.Single(await cohorteService.GetMesParcoursEnCoursAsync(apprenantCible.Id));
        Assert.Equal("Observe ton équipe lors du prochain point hebdo.", parcoursCible.DefiIndividuel);

        // Le texte partage du template n'est jamais modifie, et un autre membre de la meme
        // Cohorte continue de voir le texte partage.
        var etapeRecue = await dbContext.ChallengeEtapes.SingleAsync(e => e.Id == etapes[0].Id);
        Assert.Equal(defiPartageOriginal, etapeRecue.DefiIndividuel);

        var parcoursAutre = Assert.Single(await cohorteService.GetMesParcoursEnCoursAsync(autreApprenant.Id));
        Assert.Equal(defiPartageOriginal, parcoursAutre.DefiIndividuel);

        // Un texte vide retire la surcharge : le membre cible revoit le texte partage.
        var (successRetour, errorMessageRetour) = await cohorteService.DefinirInstructionsPersonnaliseesMembreAsync(membreCible.Id, etapes[0].Id, "", gestionnaire.Id);
        Assert.True(successRetour, errorMessageRetour);
        var parcoursCibleApresRetour = Assert.Single(await cohorteService.GetMesParcoursEnCoursAsync(apprenantCible.Id));
        Assert.Equal(defiPartageOriginal, parcoursCibleApresRetour.DefiIndividuel);
    }

    [Fact]
    public async Task GetEtapesPersonnalisationAsync_ListeToutesLesEtapes_YComprisLesEtapesSuivantesPasEncoreAtteintes()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var cohorteService = CreerCohorteService(dbContext);
        var carteService = new CarteCompetenceService(dbContext);

        var (challenge, etapes, _) = await PreparerChallengePublieAsync(dbContext, nombreEtapes: 3, format: FormatChallenge.BilanCompetencesIndividuel);
        var (_, _, cartePersonnalisee) = await carteService.CreateAsync(new CarteCompetenceInput { Code = "SUP-P", Niveau = NiveauCarte.Debutant, TitreTheorie = "Carte perso" });

        var gestionnaire = new ApplicationUser { UserName = "coach@test.local", Email = "coach@test.local" };
        var apprenant = new ApplicationUser { UserName = "apprenant@test.local", Email = "apprenant@test.local" };
        dbContext.Users.AddRange(gestionnaire, apprenant);
        await dbContext.SaveChangesAsync();

        var (_, _, cohorteId) = await cohorteService.CreateAsync(new CohorteInput { ChallengeId = challenge.Id, Nom = "Bilan individuel" });
        await cohorteService.AjouterMembreManuelAsync(cohorteId!.Value, apprenant.Id);
        await cohorteService.LancerAsync(cohorteId.Value, gestionnaire.Id, "https://test.local/parcours", "https://test.local/mi-parcours");

        var membre = await dbContext.CohorteMembres.SingleAsync(m => m.CohorteId == cohorteId.Value && m.UtilisateurId == apprenant.Id);

        // La Cohorte vient d'etre lancee : EtapeCourante = 1. On personnalise pourtant l'etape
        // 3 (future, pas encore atteinte) en avance.
        await cohorteService.DefinirCartesSupplementairesMembreAsync(membre.Id, etapes[2].Id, [cartePersonnalisee!.Id], gestionnaire.Id);
        await cohorteService.DefinirInstructionsPersonnaliseesMembreAsync(membre.Id, etapes[2].Id, "Prépare ton bilan intermédiaire.", gestionnaire.Id);

        var liste = await cohorteService.GetEtapesPersonnalisationAsync(cohorteId.Value, membre.Id);

        Assert.NotNull(liste);
        Assert.Equal(3, liste!.Count);
        Assert.Equal([1, 2, 3], liste.Select(e => e.NumeroEtape));

        var etape1 = liste.Single(e => e.NumeroEtape == 1);
        Assert.True(etape1.EstEtapeCourante);
        Assert.Equal(0, etape1.NombreCartesPersonnalisees);
        Assert.False(etape1.AInstructionsPersonnalisees);

        var etape3 = liste.Single(e => e.NumeroEtape == 3);
        Assert.False(etape3.EstEtapeCourante);
        Assert.Equal(1, etape3.NombreCartesPersonnalisees);
        Assert.True(etape3.AInstructionsPersonnalisees);
    }

    [Fact]
    public async Task GetEtapesPersonnalisationAsync_RenvoieNull_SiLeMembreNAppartientPasACetteCohorte()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var cohorteService = CreerCohorteService(dbContext);

        var (challengeA, _, _) = await PreparerChallengePublieAsync(dbContext, format: FormatChallenge.BilanCompetencesIndividuel, codePrefix: "CODE-A");
        var (challengeB, _, _) = await PreparerChallengePublieAsync(dbContext, format: FormatChallenge.BilanCompetencesIndividuel, codePrefix: "CODE-B");

        var apprenant = new ApplicationUser { UserName = "apprenant@test.local", Email = "apprenant@test.local" };
        dbContext.Users.Add(apprenant);
        await dbContext.SaveChangesAsync();

        var (_, _, cohorteAId) = await cohorteService.CreateAsync(new CohorteInput { ChallengeId = challengeA.Id, Nom = "Bilan A" });
        var (_, _, cohorteBId) = await cohorteService.CreateAsync(new CohorteInput { ChallengeId = challengeB.Id, Nom = "Bilan B" });
        await cohorteService.AjouterMembreManuelAsync(cohorteAId!.Value, apprenant.Id);
        var membreCohorteA = await dbContext.CohorteMembres.SingleAsync(m => m.CohorteId == cohorteAId.Value && m.UtilisateurId == apprenant.Id);

        var liste = await cohorteService.GetEtapesPersonnalisationAsync(cohorteBId!.Value, membreCohorteA.Id);

        Assert.Null(liste);
    }
}

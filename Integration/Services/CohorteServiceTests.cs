using Application.Common.Interfaces;
using Domain.Entities;
using Integration.TestSupport;
using Microsoft.EntityFrameworkCore;
using Web.Data;
using Web.Services;

namespace Integration.Services;

public class CohorteServiceTests
{
    private static async Task<(Challenge Challenge, List<ChallengeEtape> Etapes, List<CarteCompetence> Cartes)> PreparerChallengePublieAsync(
        ApplicationDbContext dbContext, int nombreEtapes = 2, ModePlateforme mode = ModePlateforme.BtoC)
    {
        var challengeService = new ChallengeService(dbContext);
        var carteService = new CarteCompetenceService(dbContext);

        var (_, _, challenge) = await challengeService.CreateAsync(new ChallengeInput
        {
            Titre = "Challenge Test",
            NombreEtapes = nombreEtapes,
            Mode = mode,
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

    [Fact]
    public async Task CreateAsync_Echoue_SiLeChallengeEstEnBrouillon()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var challengeService = new ChallengeService(dbContext);
        var cohorteService = new CohorteService(dbContext, TestUserManagerFactory.Create(dbContext), new FakeEmailService(), new PreuveService(dbContext, TestUserManagerFactory.Create(dbContext), new FakePreuveFichierStockageService()));

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
        var cohorteService = new CohorteService(dbContext, userManager, emailService, new PreuveService(dbContext, userManager, new FakePreuveFichierStockageService()));

        var (challenge, etapes, cartes) = await PreparerChallengePublieAsync(dbContext);

        var gestionnaire = new ApplicationUser { UserName = "coach@test.local", Email = "coach@test.local" };
        var apprenant = new ApplicationUser { UserName = "apprenant@test.local", Email = "apprenant@test.local" };
        dbContext.Users.AddRange(gestionnaire, apprenant);
        await dbContext.SaveChangesAsync();

        var (_, _, cohorteId) = await cohorteService.CreateAsync(new CohorteInput { ChallengeId = challenge.Id, Nom = "Cohorte Test" });
        await cohorteService.AjouterMembreManuelAsync(cohorteId!.Value, apprenant.Id);

        var (success, errorMessage) = await cohorteService.LancerAsync(cohorteId.Value, gestionnaire.Id, "https://test.local/parcours");

        Assert.True(success, errorMessage);

        var cohorte = await dbContext.Cohortes.FirstAsync(c => c.Id == cohorteId.Value);
        Assert.Equal(StatutCohorte.Active, cohorte.Statut);
        Assert.Equal(1, cohorte.EtapeCourante);

        var attribution = await dbContext.CarteAttributions.SingleAsync(a =>
            a.CarteCompetenceId == cartes[0].Id && a.UtilisateurId == apprenant.Id);
        Assert.Equal(OrigineAttribution.Challenge, attribution.OrigineType);
        Assert.Equal(cohorteId.Value, attribution.CohorteId);
        Assert.Equal(etapes[0].Id, attribution.ChallengeEtapeId);

        var email = Assert.Single(emailService.Envois);
        Assert.Equal(apprenant.Email, email.Destinataire);
        Assert.Contains("Nouvelle étape", email.Sujet);
    }

    [Fact]
    public async Task ValiderEtapeAsync_AvanceEtapeCouranteEtAttribueLaCarteSuivante_EtNotifie()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var userManager = TestUserManagerFactory.Create(dbContext);
        var emailService = new FakeEmailService();
        var cohorteService = new CohorteService(dbContext, userManager, emailService, new PreuveService(dbContext, userManager, new FakePreuveFichierStockageService()));

        var (challenge, etapes, cartes) = await PreparerChallengePublieAsync(dbContext, nombreEtapes: 2);

        var gestionnaire = new ApplicationUser { UserName = "coach@test.local", Email = "coach@test.local" };
        var apprenant = new ApplicationUser { UserName = "apprenant@test.local", Email = "apprenant@test.local" };
        dbContext.Users.AddRange(gestionnaire, apprenant);
        await dbContext.SaveChangesAsync();

        var (_, _, cohorteId) = await cohorteService.CreateAsync(new CohorteInput { ChallengeId = challenge.Id, Nom = "Cohorte Test" });
        await cohorteService.AjouterMembreManuelAsync(cohorteId!.Value, apprenant.Id);
        await cohorteService.LancerAsync(cohorteId.Value, gestionnaire.Id, "https://test.local/parcours");
        emailService.Envois.Clear();

        var (success, errorMessage) = await cohorteService.ValiderEtapeAsync(cohorteId.Value, gestionnaire.Id, "https://test.local/parcours", "https://test.local/bibliotheque");

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
    public async Task ValiderEtapeAsync_SurLaDerniereEtape_ClotureLaCohorteEtEnvoieUnEmailDeClotureDistinct()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var userManager = TestUserManagerFactory.Create(dbContext);
        var emailService = new FakeEmailService();
        var cohorteService = new CohorteService(dbContext, userManager, emailService, new PreuveService(dbContext, userManager, new FakePreuveFichierStockageService()));

        var (challenge, _, _) = await PreparerChallengePublieAsync(dbContext, nombreEtapes: 1);

        var gestionnaire = new ApplicationUser { UserName = "coach@test.local", Email = "coach@test.local" };
        var apprenant = new ApplicationUser { UserName = "apprenant@test.local", Email = "apprenant@test.local" };
        dbContext.Users.AddRange(gestionnaire, apprenant);
        await dbContext.SaveChangesAsync();

        var (_, _, cohorteId) = await cohorteService.CreateAsync(new CohorteInput { ChallengeId = challenge.Id, Nom = "Cohorte Test" });
        await cohorteService.AjouterMembreManuelAsync(cohorteId!.Value, apprenant.Id);
        await cohorteService.LancerAsync(cohorteId.Value, gestionnaire.Id, "https://test.local/parcours");
        emailService.Envois.Clear();

        var (success, errorMessage) = await cohorteService.ValiderEtapeAsync(cohorteId.Value, gestionnaire.Id, "https://test.local/parcours", "https://test.local/bibliotheque");

        Assert.True(success, errorMessage);

        var cohorte = await dbContext.Cohortes.FirstAsync(c => c.Id == cohorteId.Value);
        Assert.Equal(StatutCohorte.Terminee, cohorte.Statut);

        var email = Assert.Single(emailService.Envois);
        Assert.Equal(apprenant.Email, email.Destinataire);
        Assert.Contains("terminé", email.Sujet);
        Assert.DoesNotContain("Nouvelle étape", email.Sujet);
    }

    [Fact]
    public async Task LancerAsync_NeDupliquePasUneAttributionDejaExistante()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var userManager = TestUserManagerFactory.Create(dbContext);
        var cohorteService = new CohorteService(dbContext, userManager, new FakeEmailService(), new PreuveService(dbContext, userManager, new FakePreuveFichierStockageService()));

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

        var (success, errorMessage) = await cohorteService.LancerAsync(cohorteId.Value, gestionnaire.Id, "https://test.local/parcours");
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
        var cohorteService = new CohorteService(dbContext, userManager, new FakeEmailService(), new PreuveService(dbContext, userManager, new FakePreuveFichierStockageService()));

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
        await cohorteService.LancerAsync(cohorteId.Value, gestionnaire.Id, "https://test.local/parcours");
        await cohorteService.ValiderEtapeAsync(cohorteId.Value, gestionnaire.Id, "https://test.local/parcours", "https://test.local/bibliotheque");

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
        var cohorteService = new CohorteService(dbContext, userManager, emailService, new PreuveService(dbContext, userManager, new FakePreuveFichierStockageService()));

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
        var cohorteService = new CohorteService(dbContext, userManager, emailService, new PreuveService(dbContext, userManager, new FakePreuveFichierStockageService()));

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
        var cohorteService = new CohorteService(dbContext, userManager, new FakeEmailService(), new PreuveService(dbContext, userManager, new FakePreuveFichierStockageService()));

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
        var cohorteService = new CohorteService(dbContext, userManager, new FakeEmailService(), new PreuveService(dbContext, userManager, new FakePreuveFichierStockageService()));

        var (challenge, _, _) = await PreparerChallengePublieAsync(dbContext, nombreEtapes: 1);
        var gestionnaire = new ApplicationUser { UserName = "coach@test.local", Email = "coach@test.local" };
        dbContext.Users.Add(gestionnaire);
        await dbContext.SaveChangesAsync();

        var (_, _, cohorteId) = await cohorteService.CreateAsync(new CohorteInput { ChallengeId = challenge.Id, Nom = "Cohorte Test" });
        await cohorteService.LancerAsync(cohorteId!.Value, gestionnaire.Id, "https://test.local/parcours");

        var (success, errorMessage) = await cohorteService.SupprimerAsync(cohorteId.Value);

        Assert.False(success);
        Assert.NotNull(errorMessage);
        Assert.NotNull(await cohorteService.GetResumeAsync(cohorteId.Value));
    }
}

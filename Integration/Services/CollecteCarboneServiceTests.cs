using Application.Common.Interfaces;
using ClosedXML.Excel;
using Domain.Entities;
using Integration.TestSupport;
using Microsoft.EntityFrameworkCore;
using Web.Data;
using Web.Services;

namespace Integration.Services;

public class CollecteCarboneServiceTests
{
    private const string Lien = "https://test.local/lien";

    private static async Task<PreBilanCarbone> SemerPreBilanAsync(ApplicationDbContext dbContext, string email = "client@test.local")
    {
        var elec = new FacteurEmission
        {
            Code = "ENERGIE_ELEC",
            Poste = PosteEmission.Energie,
            TypeDonnee = TypeDonneeActivite.Physique,
            Nom = "Électricité",
            Unite = "kWh",
            ValeurKgCO2eParUnite = 0.058m,
            Source = "test",
            Ordre = 1,
        };
        var it = new FacteurEmission
        {
            Code = "ACHAT_IT",
            Poste = PosteEmission.AchatsBiensEtServices,
            TypeDonnee = TypeDonneeActivite.Monetaire,
            Nom = "Informatique, logiciels et hébergement",
            Unite = "€",
            ValeurKgCO2eParUnite = 0.075m,
            Source = "test",
            Ordre = 3,
        };
        var preBilan = new PreBilanCarbone
        {
            Email = email,
            Prenom = "Camille",
            Societe = "Test SAS",
            EffectifEtp = 12,
            ChiffreAffairesKEuros = 1500m,
            Lignes =
            [
                new PreBilanLigne { FacteurEmission = elec, ValeurSaisie = 20_000m, EmissionsKgCO2e = 1160m },
                new PreBilanLigne { FacteurEmission = it, ValeurSaisie = 10_000m, EmissionsKgCO2e = 750m },
            ],
            TotalEmissionsKgCO2e = 1910m,
        };
        dbContext.PreBilansCarbone.Add(preBilan);
        await dbContext.SaveChangesAsync();
        return preBilan;
    }

    private static (CollecteCarboneService Service, FakeEmailService Emails) CreerService(ApplicationDbContext dbContext)
    {
        var emails = new FakeEmailService();
        return (new CollecteCarboneService(dbContext, TestUserManagerFactory.Create(dbContext), emails, new FakePreuveFichierStockageService()), emails);
    }

    private static Dictionary<string, string> ReponsesCompletes(string valeurOuiNon) =>
        QuestionnaireCollecteCatalogue.Questions.ToDictionary(
            q => q.Code,
            q => q.Type switch
            {
                TypeQuestionCollecte.OuiNon => valeurOuiNon,
                TypeQuestionCollecte.Choix => q.Choix![0],
                _ => "10",
            });

    [Fact]
    public async Task OuvrirCompteClientAsync_CreeUnCompteClientDistinctEtEnvoieLInvitation()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var preBilan = await SemerPreBilanAsync(dbContext);
        var (service, emails) = CreerService(dbContext);

        var (success, erreur) = await service.OuvrirCompteClientAsync(preBilan.Id, "gestionnaire-1", token => $"{Lien}?token={token}");

        Assert.True(success, erreur);
        var utilisateur = await dbContext.Users.SingleAsync(u => u.Email == "client@test.local");
        Assert.Equal(TypeCompte.ClientCarbone, utilisateur.TypeCompte);
        Assert.Null(utilisateur.PasswordHash);

        var dossier = await dbContext.DossiersCarbone.SingleAsync();
        Assert.Equal(preBilan.Id, dossier.PreBilanCarboneId);
        Assert.Equal("Test SAS", dossier.RaisonSociale);

        var invitation = await dbContext.InvitationsComptes.SingleAsync();
        var envoi = Assert.Single(emails.Envois);
        Assert.Equal("client@test.local", envoi.Destinataire);
        Assert.Contains(invitation.Token, envoi.CorpsHtml);
        Assert.Contains("client@test.local", envoi.CorpsHtml);

        Assert.Equal(StatutPreBilanCrm.Qualifie, (await dbContext.PreBilansCarbone.SingleAsync()).StatutCrm);
    }

    [Fact]
    public async Task OuvrirCompteClientSelfServiceAsync_CreeUnCompteClientSansGestionnaire()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var preBilan = await SemerPreBilanAsync(dbContext);
        var (service, emails) = CreerService(dbContext);

        var (success, erreur) = await service.OuvrirCompteClientSelfServiceAsync(preBilan.Id, token => $"{Lien}?token={token}");

        Assert.True(success, erreur);
        var utilisateur = await dbContext.Users.SingleAsync(u => u.Email == "client@test.local");
        Assert.Equal(TypeCompte.ClientCarbone, utilisateur.TypeCompte);

        var dossier = await dbContext.DossiersCarbone.SingleAsync();
        Assert.Null(dossier.OuvertParId);
        Assert.Single(emails.Envois);
    }

    [Fact]
    public async Task OuvrirCompteClientAsync_CumuleAvecUnCompteParticipantExistant_SansChangerSonTypeCompte()
    {
        // Les acces se cumulent : un compte Participant qui utilise deja cet email recoit
        // en plus l'acces a l'Espace Client Carbone, sans que son TypeCompte ne change
        // (cf. Domain.Entities.TypeCompte).
        await using var dbContext = InMemoryDbContextFactory.Create();
        var preBilan = await SemerPreBilanAsync(dbContext);
        var userManager = TestUserManagerFactory.Create(dbContext);
        await userManager.CreateAsync(new ApplicationUser { UserName = "client@test.local", Email = "client@test.local" });
        var (service, emails) = CreerService(dbContext);

        var (success, erreur) = await service.OuvrirCompteClientAsync(preBilan.Id, "gestionnaire-1", _ => Lien);

        Assert.True(success, erreur);
        var utilisateur = await dbContext.Users.SingleAsync(u => u.Email == "client@test.local");
        Assert.Equal(TypeCompte.Participant, utilisateur.TypeCompte);

        var dossier = await dbContext.DossiersCarbone.SingleAsync();
        Assert.Equal(utilisateur.Id, dossier.ClientUtilisateurId);
        Assert.Single(emails.Envois);
    }

    [Fact]
    public async Task OuvrirCompteClientAsync_Refuse_UnSecondDossierPourLeMemePreDiagnostic()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var preBilan = await SemerPreBilanAsync(dbContext);
        var (service, _) = CreerService(dbContext);

        await service.OuvrirCompteClientAsync(preBilan.Id, "g", _ => Lien);
        var (success, _) = await service.OuvrirCompteClientAsync(preBilan.Id, "g", _ => Lien);

        Assert.False(success);
        Assert.Single(await dbContext.DossiersCarbone.ToListAsync());
    }

    [Fact]
    public async Task EnvoyerQuestionnaireAsync_Echoue_SansEspaceClient()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var preBilan = await SemerPreBilanAsync(dbContext);
        var (service, _) = CreerService(dbContext);

        var (success, _) = await service.EnvoyerQuestionnaireAsync(preBilan.Id, Lien);

        Assert.False(success);
    }

    [Fact]
    public async Task ParcoursComplet_OuvertureEnvoiSaisieSoumission()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var preBilan = await SemerPreBilanAsync(dbContext);
        var (service, emails) = CreerService(dbContext);

        await service.OuvrirCompteClientAsync(preBilan.Id, "g", _ => Lien);
        var (envoye, erreurEnvoi) = await service.EnvoyerQuestionnaireAsync(preBilan.Id, "https://test.local/EspaceClient/Questionnaire");
        Assert.True(envoye, erreurEnvoi);
        Assert.Equal(2, emails.Envois.Count);
        Assert.Contains("/EspaceClient/Questionnaire", emails.Envois[1].CorpsHtml);

        var clientId = (await dbContext.Users.SingleAsync()).Id;

        // Brouillon partiel : accepte.
        var (brouillon, erreurBrouillon) = await service.EnregistrerReponsesAsync(clientId, new() { ["CLIM_FROID"] = "Oui" }, soumettre: false);
        Assert.True(brouillon, erreurBrouillon);
        Assert.Equal(StatutQuestionnaireCollecte.EnCours, (await service.GetDossierClientAsync(clientId))!.StatutQuestionnaire);

        // Soumission incomplete : refusee, mais le brouillon est conserve.
        var (incomplet, _) = await service.EnregistrerReponsesAsync(clientId, new() { ["CLIM_FROID"] = "Oui" }, soumettre: true);
        Assert.False(incomplet);

        // Soumission complete.
        var reponses = ReponsesCompletes("Non");
        reponses["CLIM_FROID"] = "Oui";
        reponses["CODE_INCONNU"] = "Oui";
        var (soumis, erreurSoumission) = await service.EnregistrerReponsesAsync(clientId, reponses, soumettre: true);
        Assert.True(soumis, erreurSoumission);

        var dossier = (await service.GetDossierClientAsync(clientId))!;
        Assert.Equal(StatutQuestionnaireCollecte.Soumis, dossier.StatutQuestionnaire);
        Assert.NotNull(dossier.QuestionnaireSoumisLe);
        Assert.DoesNotContain("CODE_INCONNU", dossier.Reponses.Keys);

        // Une fois soumis, le client ne peut plus modifier.
        var (modification, _) = await service.EnregistrerReponsesAsync(clientId, reponses, soumettre: false);
        Assert.False(modification);

        var checklist = await service.GetChecklistAsync(dossier.Id);
        Assert.Contains(checklist, e => e.Poste == PosteCollecte.AutresEmissionsDirectes);
        Assert.DoesNotContain(checklist, e => e.Poste == PosteCollecte.Fret);

        // Le Gestionnaire peut rouvrir en renvoyant le questionnaire.
        await service.EnvoyerQuestionnaireAsync(preBilan.Id, Lien);
        Assert.Equal(StatutQuestionnaireCollecte.EnCours, (await service.GetDossierClientAsync(clientId))!.StatutQuestionnaire);
    }

    [Fact]
    public async Task GetDossierClientAsync_NeRenvoieQueLeDossierDuClientConnecte()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var preBilanA = await SemerPreBilanAsync(dbContext, "a@test.local");
        var preBilanB = await SemerPreBilanAsync(dbContext, "b@test.local");
        var (service, _) = CreerService(dbContext);
        await service.OuvrirCompteClientAsync(preBilanA.Id, "g", _ => Lien);
        await service.OuvrirCompteClientAsync(preBilanB.Id, "g", _ => Lien);

        var clientA = await dbContext.Users.SingleAsync(u => u.Email == "a@test.local");
        var dossier = await service.GetDossierClientAsync(clientA.Id);

        Assert.Equal(preBilanA.Id, dossier!.PreBilanCarboneId);
    }

    [Fact]
    public void Checklist_PreRemplitLesDonneesDuPreDiagnosticEnUnitesAbc()
    {
        var preBilan = new PreBilanCarbone
        {
            Email = "x@test.local",
            EffectifEtp = 12,
            ChiffreAffairesKEuros = 1500m,
            Lignes =
            [
                new PreBilanLigne
                {
                    ValeurSaisie = 20_000m,
                    FacteurEmission = new FacteurEmission { Code = "ENERGIE_ELEC", Poste = PosteEmission.Energie, TypeDonnee = TypeDonneeActivite.Physique, Nom = "Électricité", Unite = "kWh" },
                },
                new PreBilanLigne
                {
                    ValeurSaisie = 10_000m,
                    FacteurEmission = new FacteurEmission { Code = "ACHAT_IT", Poste = PosteEmission.AchatsBiensEtServices, TypeDonnee = TypeDonneeActivite.Monetaire, Nom = "Informatique", Unite = "€" },
                },
            ],
        };

        var elements = ChecklistCollecteBuilder.Construire(preBilan, null);

        var elec = Assert.Single(elements, e => e.Poste == PosteCollecte.Energie && e.Valeur.HasValue);
        Assert.Equal(20_000m, elec.Valeur);
        Assert.Equal("kWh", elec.Unite);

        // Montant en € dans le Pre-diagnostic -> k€ HT dans la collecte ABC.
        var it = Assert.Single(elements, e => e.Donnee == "Informatique");
        Assert.Equal(PosteCollecte.IntrantsServices, it.Poste);
        Assert.Equal(10m, it.Valeur);
        Assert.Equal("k€ HT", it.Unite);
        Assert.Equal(ChecklistCollecteBuilder.TypeComptable, it.TypeDonnee);

        Assert.Equal(12m, elements.Single(e => e.Donnee == "Effectif moyen").Valeur);

        // Sans reponse au questionnaire : aucun poste conditionnel (fret, fluides, aval).
        Assert.DoesNotContain(elements, e => e.Poste is PosteCollecte.Fret or PosteCollecte.AutresEmissionsDirectes or PosteCollecte.Utilisation or PosteCollecte.FinDeVie);
    }

    [Fact]
    public void Checklist_JeNeSaisPasActiveLePosteCommeOui()
    {
        var preBilan = new PreBilanCarbone { Email = "x@test.local" };

        var elements = ChecklistCollecteBuilder.Construire(preBilan, new Dictionary<string, string>
        {
            ["CLIM_FROID"] = QuestionnaireCollecteCatalogue.NeSaitPas,
            ["PRODUITS_VENDUS"] = QuestionnaireCollecteCatalogue.Non,
        });

        Assert.Contains(elements, e => e.Poste == PosteCollecte.AutresEmissionsDirectes);
        Assert.DoesNotContain(elements, e => e.Poste == PosteCollecte.FinDeVie);
    }

    [Fact]
    public async Task GenererTableurAsync_ProduitUnOngletParPosteConcerneAvecLesValeursPreRemplies()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var preBilan = await SemerPreBilanAsync(dbContext);
        var (service, _) = CreerService(dbContext);

        var tableur = await service.GenererTableurAsync(preBilan.Id);

        Assert.NotNull(tableur);
        Assert.EndsWith(".xlsx", tableur!.Value.NomFichier);

        using var classeur = new XLWorkbook(new MemoryStream(tableur.Value.Contenu));
        var noms = classeur.Worksheets.Select(f => f.Name).ToList();
        Assert.Contains("Lisez-moi", noms);
        Assert.Contains("Suivi", noms);
        Assert.Contains("Énergie", noms);
        Assert.DoesNotContain("Fret", noms);

        var energie = classeur.Worksheet("Énergie");
        Assert.Equal("Donnée à collecter", energie.Cell(3, 2).GetString());
        var ligneElec = energie.RowsUsed().First(r => r.Cell(4).TryGetValue<double>(out var v) && v == 20_000);
        Assert.Equal("Approchée", ligneElec.Cell(6).GetString());
        Assert.Equal("Reçu", ligneElec.Cell(11).GetString());
    }

    [Fact]
    public async Task DeposerTableurRempliAsync_MetAJourLAvancementEtRemplaceLAncienDepot()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var preBilan = await SemerPreBilanAsync(dbContext);
        var (service, _) = CreerService(dbContext);
        await service.OuvrirCompteClientAsync(preBilan.Id, "g", _ => Lien);
        var clientId = (await dbContext.Users.SingleAsync()).Id;

        var tableur = await service.GenererTableurAsync(preBilan.Id);
        var (premierDepot, erreur1) = await service.DeposerTableurRempliAsync(clientId, new MemoryStream(tableur!.Value.Contenu), "rempli-v1.xlsx", collecteDeclareeTerminee: false);
        Assert.True(premierDepot, erreur1);

        var dossierApres1 = await service.GetDossierClientAsync(clientId);
        Assert.NotNull(dossierApres1!.TableurDeposeLe);
        Assert.NotNull(dossierApres1.TableurDeposeAvancementPourcent);
        Assert.True(dossierApres1.TableurDeposeAvancementPourcent is > 0 and < 100);
        Assert.False(dossierApres1.CollecteDeclareeTerminee);
        Assert.Equal("rempli-v1.xlsx", dossierApres1.TableurDeposeNomFichier);

        // Second depot : remplace le precedent (un seul conserve), peut declarer termine.
        var (secondDepot, erreur2) = await service.DeposerTableurRempliAsync(clientId, new MemoryStream(tableur.Value.Contenu), "rempli-v2.xlsx", collecteDeclareeTerminee: true);
        Assert.True(secondDepot, erreur2);

        var dossierApres2 = await service.GetDossierClientAsync(clientId);
        Assert.Equal("rempli-v2.xlsx", dossierApres2!.TableurDeposeNomFichier);
        Assert.True(dossierApres2.CollecteDeclareeTerminee);
    }

    [Fact]
    public async Task DeposerTableurRempliAsync_Refuse_FichierInvalide()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var preBilan = await SemerPreBilanAsync(dbContext);
        var (service, _) = CreerService(dbContext);
        await service.OuvrirCompteClientAsync(preBilan.Id, "g", _ => Lien);
        var clientId = (await dbContext.Users.SingleAsync()).Id;

        var (success, erreur) = await service.DeposerTableurRempliAsync(clientId, new MemoryStream("pas un xlsx"u8.ToArray()), "rempli.xlsx", collecteDeclareeTerminee: false);

        Assert.False(success);
        Assert.NotNull(erreur);
        Assert.Null((await service.GetDossierClientAsync(clientId))!.TableurDeposeLe);
    }

    [Fact]
    public async Task TelechargerDernierTableurDeposeAsync_RenvoieNull_SiAucunDepot()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var preBilan = await SemerPreBilanAsync(dbContext);
        var (service, _) = CreerService(dbContext);
        await service.OuvrirCompteClientAsync(preBilan.Id, "g", _ => Lien);

        Assert.Null(await service.TelechargerDernierTableurDeposeAsync(preBilan.Id));
    }

    [Fact]
    public async Task TelechargerDernierTableurDeposeAsync_RenvoieLeDernierFichierDepose()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var preBilan = await SemerPreBilanAsync(dbContext);
        var (service, _) = CreerService(dbContext);
        await service.OuvrirCompteClientAsync(preBilan.Id, "g", _ => Lien);
        var clientId = (await dbContext.Users.SingleAsync()).Id;
        var tableur = await service.GenererTableurAsync(preBilan.Id);
        await service.DeposerTableurRempliAsync(clientId, new MemoryStream(tableur!.Value.Contenu), "rempli.xlsx", collecteDeclareeTerminee: false);

        var depot = await service.TelechargerDernierTableurDeposeAsync(preBilan.Id);

        Assert.NotNull(depot);
        Assert.Equal("rempli.xlsx", depot!.Value.NomFichier);
        using var classeur = new XLWorkbook(depot.Value.Contenu);
        Assert.Contains("Lisez-moi", classeur.Worksheets.Select(f => f.Name));
    }
}

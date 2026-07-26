using Application.Common.Interfaces;
using Domain.Entities;
using Integration.TestSupport;
using Microsoft.EntityFrameworkCore;
using Web.Data;
using Web.Services;

namespace Integration.Services;

public class PreuveServiceTests
{
    private static async Task<(int CohorteId, int ChallengeEtapeId, List<ApplicationUser> Membres, string GestionnaireId)> CreerCohorteActiveAsync(
        ApplicationDbContext dbContext, ICohorteService cohorteService, int nombreMembres = 3, int nombreEtapes = 1)
    {
        var challengeService = new ChallengeService(dbContext);
        var (_, _, challenge) = await challengeService.CreateAsync(new ChallengeInput { Titre = "Challenge Preuves", NombreEtapes = nombreEtapes, Mode = ModePlateforme.BtoC });

        int etapeId = 0;
        for (var i = 1; i <= nombreEtapes; i++)
        {
            var (_, _, etape) = await challengeService.CreerEtapeAsync(challenge!.Id, new ChallengeEtapeInput { TitreEtape = $"Étape {i}", DefiIndividuel = $"Défi {i}" });
            if (i == 1)
            {
                etapeId = etape!.Id;
            }
        }
        await challengeService.PublierAsync(challenge!.Id);

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

        var (_, _, cohorteId) = await cohorteService.CreateAsync(new CohorteInput { ChallengeId = challenge.Id, Nom = "Cohorte Preuves" });
        foreach (var membre in membres)
        {
            await cohorteService.AjouterMembreManuelAsync(cohorteId!.Value, membre.Id);
        }
        await cohorteService.LancerAsync(cohorteId!.Value, gestionnaire.Id, "https://test.local/parcours");

        return (cohorteId.Value, etapeId, membres, gestionnaire.Id);
    }

    private static (ApplicationDbContext DbContext, ICohorteService CohorteService, IPreuveService PreuveService) CreerServices()
    {
        var dbContext = InMemoryDbContextFactory.Create();
        var userManager = TestUserManagerFactory.Create(dbContext);
        var preuveService = new PreuveService(dbContext, userManager, new FakePreuveFichierStockageService(), new NotificationService(dbContext), new FakeEmailService());
        var cohorteService = new CohorteService(dbContext, userManager, new FakeEmailService(), preuveService);
        return (dbContext, cohorteService, preuveService);
    }

    private static FichierPreuveInput CreerFichier(string nom = "capture.png") => new()
    {
        NomFichier = nom,
        Contenu = new MemoryStream([1, 2, 3]),
        TailleOctets = 3,
    };

    [Fact]
    public async Task DeposerOuModifierAsync_CreeUnePreuveMultiFichiers_PuisModificationLaRemetSoumiseEtInvalideLesVotesPairs()
    {
        var (dbContext, cohorteService, preuveService) = CreerServices();
        await using var _ = dbContext;

        var (cohorteId, etapeId, membres, _) = await CreerCohorteActiveAsync(dbContext, cohorteService);
        var auteur = membres[0];
        var pair = membres[1];

        var (success, errorMessage, preuveId) = await preuveService.DeposerOuModifierAsync(
            auteur.Id, cohorteId, etapeId, "Ma description", [CreerFichier("photo.jpg"), CreerFichier("rapport.pdf")], null);

        Assert.True(success, errorMessage);
        var preuve = await preuveService.GetMaPreuveAsync(auteur.Id, etapeId);
        Assert.Equal(2, preuve!.Fichiers.Count);
        Assert.Equal(StatutPreuve.Soumise, preuve.Statut);

        var (voteSuccess, voteError) = await preuveService.ValiderParPairAsync(preuveId!.Value, pair.Id, DecisionValidationPair.Valide, null, "https://test.local/suivi-preuve");
        Assert.True(voteSuccess, voteError);

        var apresVote = await preuveService.GetMaPreuveAsync(auteur.Id, etapeId);
        Assert.Single(apresVote!.Retours);

        // Modification : ajoute un fichier, retire l'autre.
        var idARetirer = apresVote.Fichiers.First(f => f.NomFichier == "photo.jpg").Id;
        await preuveService.DeposerOuModifierAsync(auteur.Id, cohorteId, etapeId, "Description mise à jour", [CreerFichier("video.mp4")], [idARetirer]);

        var apresModification = await preuveService.GetMaPreuveAsync(auteur.Id, etapeId);
        Assert.Equal(StatutPreuve.Soumise, apresModification!.Statut);
        Assert.Equal(2, apresModification.Fichiers.Count);
        Assert.Contains(apresModification.Fichiers, f => f.NomFichier == "rapport.pdf");
        Assert.Contains(apresModification.Fichiers, f => f.NomFichier == "video.mp4");
        Assert.DoesNotContain(apresModification.Fichiers, f => f.NomFichier == "photo.jpg");

        // Le vote pair precedent a ete invalide (supprime) par la modification.
        Assert.Empty(apresModification.Retours);
    }

    [Fact]
    public async Task ValiderParPairAsync_Echoue_SiLeValideurEstLAuteur()
    {
        var (dbContext, cohorteService, preuveService) = CreerServices();
        await using var _ = dbContext;

        var (cohorteId, etapeId, membres, _) = await CreerCohorteActiveAsync(dbContext, cohorteService);
        var auteur = membres[0];

        var (_, _, preuveId) = await preuveService.DeposerOuModifierAsync(auteur.Id, cohorteId, etapeId, "Description", [], null);

        var (success, errorMessage) = await preuveService.ValiderParPairAsync(preuveId!.Value, auteur.Id, DecisionValidationPair.Valide, null, "https://test.local/suivi-preuve");

        Assert.False(success);
        Assert.NotNull(errorMessage);
    }

    [Fact]
    public async Task ValiderParPairAsync_GenerePointsKarma_PourValideCommePourARevoir()
    {
        var (dbContext, cohorteService, preuveService) = CreerServices();
        await using var _ = dbContext;

        var (cohorteId, etapeId, membres, _) = await CreerCohorteActiveAsync(dbContext, cohorteService, nombreMembres: 3);
        var auteur1 = membres[0];
        var auteur2 = membres[1];
        var pair = membres[2];

        var (_, _, preuve1Id) = await preuveService.DeposerOuModifierAsync(auteur1.Id, cohorteId, etapeId, "D1", [], null);
        var (_, _, preuve2Id) = await preuveService.DeposerOuModifierAsync(auteur2.Id, cohorteId, etapeId, "D2", [], null);

        await preuveService.ValiderParPairAsync(preuve1Id!.Value, pair.Id, DecisionValidationPair.Valide, null, "https://test.local/suivi-preuve");
        await preuveService.ValiderParPairAsync(preuve2Id!.Value, pair.Id, DecisionValidationPair.ARevoir, "À préciser", "https://test.local/suivi-preuve");

        var points = await preuveService.GetMesPointsAsync(pair.Id);
        Assert.Equal(2 * Application.Common.PointsConfig.PointsKarmaDecisionPair, points.TotalPointsKarma);
    }

    [Fact]
    public async Task ValiderParPairAsync_ModifierSonAvis_NeDupliquePasLaLigneNiLesPoints()
    {
        var (dbContext, cohorteService, preuveService) = CreerServices();
        await using var _ = dbContext;

        var (cohorteId, etapeId, membres, _) = await CreerCohorteActiveAsync(dbContext, cohorteService);
        var auteur = membres[0];
        var pair = membres[1];

        var (_, _, preuveId) = await preuveService.DeposerOuModifierAsync(auteur.Id, cohorteId, etapeId, "D", [], null);

        await preuveService.ValiderParPairAsync(preuveId!.Value, pair.Id, DecisionValidationPair.ARevoir, "Premier avis", "https://test.local/suivi-preuve");
        await preuveService.ValiderParPairAsync(preuveId.Value, pair.Id, DecisionValidationPair.Valide, null, "https://test.local/suivi-preuve");

        var nombreValidations = await dbContext.PreuveValidationsPairs.CountAsync(v => v.PreuveId == preuveId.Value && v.ValideurId == pair.Id);
        Assert.Equal(1, nombreValidations);

        var points = await preuveService.GetMesPointsAsync(pair.Id);
        Assert.Equal(Application.Common.PointsConfig.PointsKarmaDecisionPair, points.TotalPointsKarma);
    }

    [Theory]
    [InlineData(1, 1, true)]   // 1 Valide / 1 = 100%
    [InlineData(1, 2, true)]   // 1 Valide / 2 = 50%
    [InlineData(2, 3, true)]   // 2 Valide / 3 = 66%
    [InlineData(0, 1, false)]  // 0/1 = 0%
    [InlineData(1, 3, false)]  // 1/3 = 33%
    public async Task ValiderParPairAsync_AppliqueLeSeuilDe50Pourcent(int nombreValide, int nombreTotal, bool doitEtreValideeParLesPairs)
    {
        var (dbContext, cohorteService, preuveService) = CreerServices();
        await using var _ = dbContext;

        var (cohorteId, etapeId, membres, _) = await CreerCohorteActiveAsync(dbContext, cohorteService, nombreMembres: nombreTotal + 1);
        var auteur = membres[0];
        var pairs = membres.Skip(1).Take(nombreTotal).ToList();

        var (_, _, preuveId) = await preuveService.DeposerOuModifierAsync(auteur.Id, cohorteId, etapeId, "D", [], null);

        for (var i = 0; i < nombreTotal; i++)
        {
            var decision = i < nombreValide ? DecisionValidationPair.Valide : DecisionValidationPair.ARevoir;
            var commentaire = decision == DecisionValidationPair.ARevoir ? "À revoir" : null;
            await preuveService.ValiderParPairAsync(preuveId!.Value, pairs[i].Id, decision, commentaire, "https://test.local/suivi-preuve");
        }

        var preuve = await dbContext.Preuves.FirstAsync(p => p.Id == preuveId);
        Assert.Equal(doitEtreValideeParLesPairs ? StatutPreuve.ValideeParLesPairs : StatutPreuve.Soumise, preuve.Statut);
    }

    [Fact]
    public async Task ValiderParPairAsync_Reversion_RepasseASoumiseSiNouveauVoteFaitChuterLeRatio()
    {
        var (dbContext, cohorteService, preuveService) = CreerServices();
        await using var _ = dbContext;

        var (cohorteId, etapeId, membres, _) = await CreerCohorteActiveAsync(dbContext, cohorteService, nombreMembres: 4);
        var auteur = membres[0];
        var pair1 = membres[1];
        var pair2 = membres[2];
        var pair3 = membres[3];

        var (_, _, preuveId) = await preuveService.DeposerOuModifierAsync(auteur.Id, cohorteId, etapeId, "D", [], null);

        // 1 Valide / 1 = 100% -> franchit le seuil.
        await preuveService.ValiderParPairAsync(preuveId!.Value, pair1.Id, DecisionValidationPair.Valide, null, "https://test.local/suivi-preuve");
        var apresPremierVote = await dbContext.Preuves.FirstAsync(p => p.Id == preuveId);
        Assert.Equal(StatutPreuve.ValideeParLesPairs, apresPremierVote.Statut);

        // 1 Valide / 2 = 50% -> encore au seuil, reste ValideeParLesPairs.
        await preuveService.ValiderParPairAsync(preuveId.Value, pair2.Id, DecisionValidationPair.ARevoir, "Pas convaincant", "https://test.local/suivi-preuve");
        var apresDeuxiemeVote = await dbContext.Preuves.FirstAsync(p => p.Id == preuveId);
        Assert.Equal(StatutPreuve.ValideeParLesPairs, apresDeuxiemeVote.Statut);

        // 1 Valide / 3 = 33% -> repasse sous le seuil : reversion vers Soumise.
        await preuveService.ValiderParPairAsync(preuveId.Value, pair3.Id, DecisionValidationPair.ARevoir, "Pas convaincant non plus", "https://test.local/suivi-preuve");
        var apresTroisiemeVote = await dbContext.Preuves.FirstAsync(p => p.Id == preuveId);
        Assert.Equal(StatutPreuve.Soumise, apresTroisiemeVote.Statut);
    }

    [Fact]
    public async Task GetPreuvesAValiderAsync_ContientLesPreuvesDejaValideesParLesPairs_MaisPasLesFinalisees()
    {
        var (dbContext, cohorteService, preuveService) = CreerServices();
        await using var _ = dbContext;

        var (cohorteId, etapeId, membres, gestionnaireId) = await CreerCohorteActiveAsync(dbContext, cohorteService, nombreMembres: 4);
        var auteur1 = membres[0]; // deviendra ValideeParLesPairs
        var auteur2 = membres[1]; // deviendra ValideeDefinitivement (via Gestionnaire)
        var pairObserve = membres[2];
        var pairQuiVoteDeja = membres[3];

        var (_, _, preuve1Id) = await preuveService.DeposerOuModifierAsync(auteur1.Id, cohorteId, etapeId, "D1", [], null);
        var (_, _, preuve2Id) = await preuveService.DeposerOuModifierAsync(auteur2.Id, cohorteId, etapeId, "D2", [], null);

        // Preuve 1 : validee par un AUTRE pair (pas encore par pairObserve) -> doit rester
        // dans la file de pairObserve (chacun doit voir chaque preuve, meme deja validee).
        await preuveService.ValiderParPairAsync(preuve1Id!.Value, pairQuiVoteDeja.Id, DecisionValidationPair.Valide, null, "https://test.local/suivi-preuve");

        // Preuve 2 : finalisee directement par le Gestionnaire -> ne doit plus apparaitre.
        await preuveService.ValiderParGestionnaireAsync(preuve2Id!.Value, gestionnaireId, DecisionValidationGestionnaire.Valide, null, "https://test.local/suivi-preuve");

        var file = await preuveService.GetPreuvesAValiderAsync(pairObserve.Id, cohorteId);

        Assert.Single(file);
        Assert.Equal(preuve1Id, file[0].PreuveId);
    }

    [Fact]
    public async Task GetApercuPourPairAsync_NeMontreJamaisLesAvisDesAutresPairs_MemeSiJaiDejaVote()
    {
        var (dbContext, cohorteService, preuveService) = CreerServices();
        await using var _ = dbContext;

        var (cohorteId, etapeId, membres, _) = await CreerCohorteActiveAsync(dbContext, cohorteService, nombreMembres: 3);
        var auteur = membres[0];
        var pairA = membres[1];
        var pairB = membres[2];

        var (_, _, preuveId) = await preuveService.DeposerOuModifierAsync(auteur.Id, cohorteId, etapeId, "D", [], null);

        await preuveService.ValiderParPairAsync(preuveId!.Value, pairA.Id, DecisionValidationPair.ARevoir, "Avis de pairA, secret", "https://test.local/suivi-preuve");

        // pairB n'a pas encore vote : ne doit voir ni statut agrege ni l'avis de pairA.
        var apercuAvantVote = await preuveService.GetApercuPourPairAsync(preuveId.Value, pairB.Id);
        Assert.NotNull(apercuAvantVote);
        Assert.Null(apercuAvantVote!.MaDecisionPrecedente);

        await preuveService.ValiderParPairAsync(preuveId.Value, pairB.Id, DecisionValidationPair.Valide, null, "https://test.local/suivi-preuve");

        // pairB a maintenant vote : voit seulement SON propre avis precedent, jamais celui de pairA.
        var apercuApresVote = await preuveService.GetApercuPourPairAsync(preuveId.Value, pairB.Id);
        Assert.Equal(DecisionValidationPair.Valide, apercuApresVote!.MaDecisionPrecedente);

        // Seul l'auteur voit le fil complet (les deux avis).
        var detailAuteur = await preuveService.GetMaPreuveAsync(auteur.Id, etapeId);
        Assert.Equal(2, detailAuteur!.Retours.Count);
    }

    [Fact]
    public async Task ValiderParGestionnaireAsync_Valide_FinaliseImmediatementEtAttribueXPUneSeuleFois()
    {
        var (dbContext, cohorteService, preuveService) = CreerServices();
        await using var _ = dbContext;

        var (cohorteId, etapeId, membres, gestionnaireId) = await CreerCohorteActiveAsync(dbContext, cohorteService);
        var auteur = membres[0];

        var (_, _, preuveId) = await preuveService.DeposerOuModifierAsync(auteur.Id, cohorteId, etapeId, "D", [], null);

        var (success, errorMessage) = await preuveService.ValiderParGestionnaireAsync(preuveId!.Value, gestionnaireId, DecisionValidationGestionnaire.Valide, null, "https://test.local/suivi-preuve");
        Assert.True(success, errorMessage);

        var preuve = await dbContext.Preuves.FirstAsync(p => p.Id == preuveId);
        Assert.Equal(StatutPreuve.ValideeDefinitivement, preuve.Statut);

        var points = await preuveService.GetMesPointsAsync(auteur.Id);
        Assert.Equal(Application.Common.PointsConfig.XPSavoirPreuveValidee, points.TotalXPSavoir);

        // Re-validation : doit echouer (deja definitive), pas de second XP.
        var (secondSuccess, _) = await preuveService.ValiderParGestionnaireAsync(preuveId.Value, gestionnaireId, DecisionValidationGestionnaire.Valide, null, "https://test.local/suivi-preuve");
        Assert.False(secondSuccess);

        var pointsApres = await preuveService.GetMesPointsAsync(auteur.Id);
        Assert.Equal(Application.Common.PointsConfig.XPSavoirPreuveValidee, pointsApres.TotalXPSavoir);
    }

    [Fact]
    public async Task ValiderParGestionnaireAsync_Refuse_RemetASoumiseAvecCommentaireObligatoire()
    {
        var (dbContext, cohorteService, preuveService) = CreerServices();
        await using var _ = dbContext;

        var (cohorteId, etapeId, membres, gestionnaireId) = await CreerCohorteActiveAsync(dbContext, cohorteService);
        var auteur = membres[0];

        var (_, _, preuveId) = await preuveService.DeposerOuModifierAsync(auteur.Id, cohorteId, etapeId, "D", [], null);

        var (echecSansCommentaire, erreur) = await preuveService.ValiderParGestionnaireAsync(preuveId!.Value, gestionnaireId, DecisionValidationGestionnaire.Refuse, null, "https://test.local/suivi-preuve");
        Assert.False(echecSansCommentaire);
        Assert.NotNull(erreur);

        var (success, _) = await preuveService.ValiderParGestionnaireAsync(preuveId.Value, gestionnaireId, DecisionValidationGestionnaire.Refuse, "À corriger", "https://test.local/suivi-preuve");
        Assert.True(success);

        var preuve = await dbContext.Preuves.FirstAsync(p => p.Id == preuveId);
        Assert.Equal(StatutPreuve.Soumise, preuve.Statut);

        var detail = await preuveService.GetMaPreuveAsync(auteur.Id, etapeId);
        Assert.Contains(detail!.Retours, r => r.EstGestionnaire && r.Decision == "Refusé" && r.Commentaire == "À corriger");
    }

    [Fact]
    public async Task ClorePreuvesEtapeAsync_FinaliseSansDoubleComptage_EtAttribueLesPointsCorrects()
    {
        var (dbContext, cohorteService, preuveService) = CreerServices();
        await using var _ = dbContext;

        var (cohorteId, etapeId, membres, gestionnaireId) = await CreerCohorteActiveAsync(dbContext, cohorteService, nombreMembres: 4);
        var auteurValideeParLesPairs = membres[0];
        var auteurDejaValideeDirectement = membres[1];
        var auteurSoumise = membres[2];
        var pair = membres[3];

        var (_, _, preuve1Id) = await preuveService.DeposerOuModifierAsync(auteurValideeParLesPairs.Id, cohorteId, etapeId, "D1", [], null);
        var (_, _, preuve2Id) = await preuveService.DeposerOuModifierAsync(auteurDejaValideeDirectement.Id, cohorteId, etapeId, "D2", [], null);
        await preuveService.DeposerOuModifierAsync(auteurSoumise.Id, cohorteId, etapeId, "D3", [], null);

        await preuveService.ValiderParPairAsync(preuve1Id!.Value, pair.Id, DecisionValidationPair.Valide, null, "https://test.local/suivi-preuve");
        await preuveService.ValiderParGestionnaireAsync(preuve2Id!.Value, gestionnaireId, DecisionValidationGestionnaire.Valide, null, "https://test.local/suivi-preuve");

        await preuveService.ClorePreuvesEtapeAsync(cohorteId, 1);

        var preuve1 = await dbContext.Preuves.FirstAsync(p => p.Id == preuve1Id);
        Assert.Equal(StatutPreuve.ValideeDefinitivement, preuve1.Statut);
        var points1 = await preuveService.GetMesPointsAsync(auteurValideeParLesPairs.Id);
        Assert.Equal(Application.Common.PointsConfig.XPSavoirPreuveValidee, points1.TotalXPSavoir);
        Assert.Equal(Application.Common.PointsConfig.PointsAssiduitePreuveValideeALaCloture, points1.TotalPointsAssiduite);

        // Deja finalisee via 4.1 avant la cloture : pas de second XP_Savoir.
        var points2 = await preuveService.GetMesPointsAsync(auteurDejaValideeDirectement.Id);
        Assert.Equal(Application.Common.PointsConfig.XPSavoirPreuveValidee, points2.TotalXPSavoir);
        Assert.Equal(0, points2.TotalPointsAssiduite);

        var preuve3 = await dbContext.Preuves.SingleAsync(p => p.UtilisateurId == auteurSoumise.Id);
        Assert.Equal(StatutPreuve.NonValideeALaCloture, preuve3.Statut);
    }

    [Fact]
    public async Task StatutFige_NeChangePlusApresValideeDefinitivement_MemeAvecDeNouveauxVotesPairs()
    {
        var (dbContext, cohorteService, preuveService) = CreerServices();
        await using var _ = dbContext;

        var (cohorteId, etapeId, membres, gestionnaireId) = await CreerCohorteActiveAsync(dbContext, cohorteService, nombreMembres: 3);
        var auteur = membres[0];
        var pair = membres[1];

        var (_, _, preuveId) = await preuveService.DeposerOuModifierAsync(auteur.Id, cohorteId, etapeId, "D", [], null);
        await preuveService.ValiderParGestionnaireAsync(preuveId!.Value, gestionnaireId, DecisionValidationGestionnaire.Valide, null, "https://test.local/suivi-preuve");

        // Un pair vote APRES la finalisation : enregistre, mais ne change pas le statut.
        var (success, _) = await preuveService.ValiderParPairAsync(preuveId.Value, pair.Id, DecisionValidationPair.ARevoir, "Trop tard", "https://test.local/suivi-preuve");
        Assert.True(success);

        var preuve = await dbContext.Preuves.FirstAsync(p => p.Id == preuveId);
        Assert.Equal(StatutPreuve.ValideeDefinitivement, preuve.Statut);

        var nombreVotesEnregistres = await dbContext.PreuveValidationsPairs.CountAsync(v => v.PreuveId == preuveId.Value);
        Assert.Equal(1, nombreVotesEnregistres);
    }

    [Fact]
    public async Task AttribuerBadgeSuperHelperAsync_AttribueAuMembreLePlusKarma_DepuisLaClotureEnCours()
    {
        var (dbContext, cohorteService, preuveService) = CreerServices();
        await using var _ = dbContext;

        var (cohorteId, etapeId, membres, _) = await CreerCohorteActiveAsync(dbContext, cohorteService, nombreMembres: 3);
        var auteur = membres[0];
        var superHelper = membres[1];
        var autrePair = membres[2];

        var (_, _, preuveId) = await preuveService.DeposerOuModifierAsync(auteur.Id, cohorteId, etapeId, "D", [], null);

        // superHelper est le seul a voter -> le plus de Karma.
        await preuveService.ValiderParPairAsync(preuveId!.Value, superHelper.Id, DecisionValidationPair.Valide, null, "https://test.local/suivi-preuve");

        await preuveService.ClorePreuvesEtapeAsync(cohorteId, 1);
        await preuveService.AttribuerBadgeSuperHelperAsync(cohorteId, 1);

        var badgesSuperHelper = await preuveService.GetMesBadgesAsync(superHelper.Id);
        Assert.Single(badgesSuperHelper);
        Assert.Equal(TypeBadgeSocial.SuperHelper, badgesSuperHelper[0].TypeBadge);

        var badgesAutrePair = await preuveService.GetMesBadgesAsync(autrePair.Id);
        Assert.Empty(badgesAutrePair);
    }

    [Fact]
    public async Task GetMesPointsAsync_NeRenvoieQueMesPropresPoints_JamaisCeuxDunAutreMembre()
    {
        var (dbContext, cohorteService, preuveService) = CreerServices();
        await using var _ = dbContext;

        var (cohorteId, etapeId, membres, _) = await CreerCohorteActiveAsync(dbContext, cohorteService, nombreMembres: 3);
        var auteur = membres[0];
        var pairActif = membres[1];
        var pairInactif = membres[2];

        var (_, _, preuveId) = await preuveService.DeposerOuModifierAsync(auteur.Id, cohorteId, etapeId, "D", [], null);
        await preuveService.ValiderParPairAsync(preuveId!.Value, pairActif.Id, DecisionValidationPair.Valide, null, "https://test.local/suivi-preuve");

        var pointsPairActif = await preuveService.GetMesPointsAsync(pairActif.Id);
        var pointsPairInactif = await preuveService.GetMesPointsAsync(pairInactif.Id);

        Assert.Equal(Application.Common.PointsConfig.PointsKarmaDecisionPair, pointsPairActif.TotalPointsKarma);
        Assert.Equal(0, pointsPairInactif.TotalPointsKarma);
    }
}

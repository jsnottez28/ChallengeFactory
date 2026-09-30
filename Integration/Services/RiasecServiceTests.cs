using Application.Common.Interfaces;
using Integration.TestSupport;
using Microsoft.EntityFrameworkCore;
using Web.Data;
using Web.Services;

namespace Integration.Services;

public class RiasecServiceTests
{
    // Par construction (cf. RiasecService.Questions), NumeroQuestion 1-10 = Realiste,
    // 11-20 = Investigateur, 21-30 = Artistique, 31-40 = Social, 41-50 = Entreprenant,
    // 51-60 = Conventionnel - utilise ici pour construire des strategies de reponse
    // controlees, jamais expose par le DTO public (presentation "en aveugle").
    private static string DimensionDe(int numeroQuestion) => numeroQuestion switch
    {
        <= 10 => "R",
        <= 20 => "I",
        <= 30 => "A",
        <= 40 => "S",
        <= 50 => "E",
        _ => "C",
    };

    private static Dictionary<int, int> RepondreEnPreferant(List<RiasecPaireInfo> paires, params string[] dimensionsPreferees)
    {
        var reponses = new Dictionary<int, int>();
        foreach (var paire in paires)
        {
            var choix = dimensionsPreferees.Contains(DimensionDe(paire.OptionA.NumeroQuestion)) ? paire.OptionA.NumeroQuestion
                : dimensionsPreferees.Contains(DimensionDe(paire.OptionB.NumeroQuestion)) ? paire.OptionB.NumeroQuestion
                : paire.OptionA.NumeroQuestion;
            reponses[paire.PaireId] = choix;
        }
        return reponses;
    }

    // Construit les reponses du round de fiabilite avec la meme strategie deterministe
    // "preferer R" que RepondreEnPreferant, a partir des paires reellement proposees par
    // PreparerFiabilite pour ces reponses de round 1.
    private static Dictionary<int, int> RepondreFiabiliteEnPreferant(RiasecService riasecService, Dictionary<int, int> reponsesRound1, params string[] dimensionsPreferees)
    {
        var pairesFiabilite = riasecService.PreparerFiabilite(reponsesRound1)!;
        return RepondreEnPreferant(pairesFiabilite, dimensionsPreferees);
    }

    [Fact]
    public void GetPairesRound1_Renvoie30PairesValides()
    {
        var riasecService = new RiasecService(InMemoryDbContextFactory.Create(), new FakeEmailService());

        var paires = riasecService.GetPairesRound1();

        Assert.Equal(30, paires.Count);
        Assert.Equal(30, paires.Select(p => p.PaireId).Distinct().Count());
        Assert.All(paires, p =>
        {
            Assert.NotEqual(p.OptionA.NumeroQuestion, p.OptionB.NumeroQuestion);
            Assert.False(string.IsNullOrWhiteSpace(p.OptionA.Texte));
            Assert.False(string.IsNullOrWhiteSpace(p.OptionB.Texte));
        });
    }

    [Fact]
    public void PreparerFiabilite_ConfronteChaqueGagnantAUnGagnantDifferentJamaisSonAdversaireDOrigine()
    {
        // Round de fiabilite : jamais la meme paire reposee a l'identique - chaque paire
        // confronte le gagnant reel d'une paire de base a un AUTRE gagnant, toujours d'une
        // dimension differente (comme partout ailleurs dans le test).
        var riasecService = new RiasecService(InMemoryDbContextFactory.Create(), new FakeEmailService());
        var pairesRound1 = riasecService.GetPairesRound1();
        var reponsesRound1 = RepondreEnPreferant(pairesRound1, "R");

        var fiabilite = riasecService.PreparerFiabilite(reponsesRound1);

        Assert.NotNull(fiabilite);
        Assert.Equal(6, fiabilite!.Count);
        Assert.Equal(6, fiabilite.Select(p => p.PaireId).Distinct().Count());
        Assert.All(fiabilite, p =>
        {
            Assert.NotEqual(p.OptionA.NumeroQuestion, p.OptionB.NumeroQuestion);
            Assert.NotEqual(DimensionDe(p.OptionA.NumeroQuestion), DimensionDe(p.OptionB.NumeroQuestion));
        });

        // Aucune paire de fiabilite ne doit reproduire exactement une paire deja posee au
        // round 1 (memes deux items, dans un ordre ou l'autre).
        var pairesRound1Set = pairesRound1
            .Select(p => (Math.Min(p.OptionA.NumeroQuestion, p.OptionB.NumeroQuestion), Math.Max(p.OptionA.NumeroQuestion, p.OptionB.NumeroQuestion)))
            .ToHashSet();
        Assert.All(fiabilite, p =>
        {
            var cle = (Math.Min(p.OptionA.NumeroQuestion, p.OptionB.NumeroQuestion), Math.Max(p.OptionA.NumeroQuestion, p.OptionB.NumeroQuestion));
            Assert.DoesNotContain(cle, pairesRound1Set);
        });
    }

    [Fact]
    public void PreparerFiabilite_RenvoieNull_SiRound1Incomplet()
    {
        var riasecService = new RiasecService(InMemoryDbContextFactory.Create(), new FakeEmailService());

        Assert.Null(riasecService.PreparerFiabilite([]));
    }

    [Fact]
    public void PreparerRound2_RenvoieNull_SiReponsesIncompletes()
    {
        var riasecService = new RiasecService(InMemoryDbContextFactory.Create(), new FakeEmailService());

        Assert.Null(riasecService.PreparerRound2([]));
    }

    [Fact]
    public async Task RepondreAsync_Echoue_SiRound1Incomplet()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var utilisateur = new ApplicationUser { UserName = "stagiaire@test.local", Email = "stagiaire@test.local" };
        dbContext.Users.Add(utilisateur);
        await dbContext.SaveChangesAsync();

        var riasecService = new RiasecService(dbContext, new FakeEmailService());

        var (success, errorMessage, resultat) = await riasecService.RepondreAsync(utilisateur.Id, [], [], []);

        Assert.False(success);
        Assert.NotNull(errorMessage);
        Assert.Null(resultat);
        Assert.False(await dbContext.RiasecResultats.AnyAsync());
    }

    [Fact]
    public async Task RepondreAsync_ScenarioCompletAvecDepartage_CalculeScoresCodeHollandEtCoherence()
    {
        // Strategie deterministe "preferer Realiste" (vérifiée manuellement) : donne
        // Scores R=10,I=1,A=4,S=5,E=2,C=8 sur les 30 paires de base, et declenche un
        // round 2 de departage entre A (4) et S (5) - 4 paires, toujours "OptionA =
        // Artistique" vs "OptionB = Social".
        await using var dbContext = InMemoryDbContextFactory.Create();
        var utilisateur = new ApplicationUser { UserName = "stagiaire@test.local", Email = "stagiaire@test.local" };
        dbContext.Users.Add(utilisateur);
        await dbContext.SaveChangesAsync();

        var riasecService = new RiasecService(dbContext, new FakeEmailService());
        var pairesRound1 = riasecService.GetPairesRound1();
        var reponsesRound1 = RepondreEnPreferant(pairesRound1, "R");

        var round2 = riasecService.PreparerRound2(reponsesRound1);
        Assert.NotNull(round2);
        Assert.Equal("A", round2!.DimensionA);
        Assert.Equal("S", round2.DimensionB);
        Assert.Equal(4, round2.Paires.Count);
        Assert.All(round2.Paires, p =>
        {
            Assert.Equal("A", DimensionDe(p.OptionA.NumeroQuestion));
            Assert.Equal("S", DimensionDe(p.OptionB.NumeroQuestion));
        });

        // Choisit systematiquement l'option Artistique (OptionA) au round 2 : Artistique
        // gagne le departage 4-0 malgre un score de round 1 inferieur a Social (4 < 5).
        var reponsesRound2 = round2.Paires.ToDictionary(p => p.PaireId, p => p.OptionA.NumeroQuestion);
        var reponsesFiabilite = RepondreFiabiliteEnPreferant(riasecService, reponsesRound1, "R");

        var (success, errorMessage, resultat) = await riasecService.RepondreAsync(utilisateur.Id, reponsesRound1, reponsesFiabilite, reponsesRound2);

        Assert.True(success, errorMessage);
        Assert.NotNull(resultat);
        Assert.Equal(10, resultat!.Dimensions.Single(d => d.Code == "R").Score);
        Assert.Equal(4, resultat.Dimensions.Single(d => d.Code == "A").Score);
        Assert.Equal(5, resultat.Dimensions.Single(d => d.Code == "S").Score);
        Assert.Equal(8, resultat.Dimensions.Single(d => d.Code == "C").Score);
        // Le departage inverse l'ordre naturel (A=4 < S=5) : Artistique passe devant Social
        // dans le code Holland malgre son score de round 1 plus faible.
        Assert.Equal("RCA", resultat.CodeHolland);
        Assert.Equal("A", resultat.DepartageDimensionA);
        Assert.Equal("S", resultat.DepartageDimensionB);
        Assert.Equal("A", resultat.DepartageGagnant);
        // Coherence du round de fiabilite (valeur reelle verifiee via diagnostic) : la
        // strategie de test ne "prefere" que R, elle ne sait pas arbitrer entre les autres
        // dimensions - sur une paire de fiabilite ne mettant pas en jeu R, elle retombe sur
        // OptionA par defaut, qui ne correspond pas toujours a la dimension la mieux notee.
        // 5/6 reste un taux de coherence eleve, sans qu'aucune paire ne soit jamais reposee
        // a l'identique (cf. PreparerFiabilite_...JamaisSonAdversaireDOrigine ci-dessus).
        Assert.Equal(5, resultat.NombrePairesCoherentes);
        Assert.Equal(6, resultat.NombrePairesControle);

        var enBase = await dbContext.RiasecResultats.SingleAsync();
        Assert.Equal("RCA", enBase.CodeHolland);

        // Synthese interpretative (cf. RiasecService.ConstruireSynthese) : pour le code
        // RCA, R-C sont voisines sur l'hexagone (distance 1) mais C-A sont opposees
        // (distance 3) -> profil "Contrasté". Verifie aussi que le contenu est construit a
        // partir des 3 dimensions dominantes uniquement (jamais dimension par dimension).
        var synthese = resultat.Synthese;
        Assert.Equal("Contrasté", synthese.TypeProfil);
        Assert.False(string.IsNullOrWhiteSpace(synthese.TypeProfilDescription));
        Assert.Equal(3, synthese.Synergies.Count);
        Assert.Equal(6, synthese.MotivationsCles.Count);
        Assert.Equal(6, synthese.TachesPreferees.Count);
        Assert.Equal(3, synthese.EnvironnementIdeal.Count);
        // Dimensions les plus faibles hors du code Holland (I=1, E=2, S=5) : I et E.
        Assert.Equal(2, synthese.PointsVigilance.Count);
        Assert.Contains("RCA", synthese.Resume);
    }

    [Fact]
    public async Task RepondreAsync_EnvoieUnEmailDeResultat()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var utilisateur = new ApplicationUser { UserName = "stagiaire@test.local", Email = "stagiaire@test.local" };
        dbContext.Users.Add(utilisateur);
        await dbContext.SaveChangesAsync();

        var emailService = new FakeEmailService();
        var riasecService = new RiasecService(dbContext, emailService);
        var pairesRound1 = riasecService.GetPairesRound1();
        var reponsesRound1 = RepondreEnPreferant(pairesRound1, "R");
        var round2 = riasecService.PreparerRound2(reponsesRound1)!;
        var reponsesRound2 = round2.Paires.ToDictionary(p => p.PaireId, p => p.OptionA.NumeroQuestion);
        var reponsesFiabilite = RepondreFiabiliteEnPreferant(riasecService, reponsesRound1, "R");

        await riasecService.RepondreAsync(utilisateur.Id, reponsesRound1, reponsesFiabilite, reponsesRound2);

        var envoi = Assert.Single(emailService.Envois);
        Assert.Equal("stagiaire@test.local", envoi.Destinataire);
        Assert.Contains("RIASEC", envoi.Sujet);
    }

    [Fact]
    public async Task RepondreAsync_Echoue_SiReponsesRound2Incompletes()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var utilisateur = new ApplicationUser { UserName = "stagiaire@test.local", Email = "stagiaire@test.local" };
        dbContext.Users.Add(utilisateur);
        await dbContext.SaveChangesAsync();

        var riasecService = new RiasecService(dbContext, new FakeEmailService());
        var pairesRound1 = riasecService.GetPairesRound1();
        var reponsesRound1 = RepondreEnPreferant(pairesRound1, "R");
        var reponsesFiabilite = RepondreFiabiliteEnPreferant(riasecService, reponsesRound1, "R");

        var (success, errorMessage, resultat) = await riasecService.RepondreAsync(utilisateur.Id, reponsesRound1, reponsesFiabilite, []);

        Assert.False(success);
        Assert.NotNull(errorMessage);
        Assert.Null(resultat);
        Assert.False(await dbContext.RiasecResultats.AnyAsync());
    }

    [Fact]
    public async Task GetDernierResultatAsync_RenvoieLePlusRecent()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var utilisateur = new ApplicationUser { UserName = "stagiaire@test.local", Email = "stagiaire@test.local" };
        dbContext.Users.Add(utilisateur);
        await dbContext.SaveChangesAsync();

        var riasecService = new RiasecService(dbContext, new FakeEmailService());
        var pairesRound1 = riasecService.GetPairesRound1();
        var reponsesRound1 = RepondreEnPreferant(pairesRound1, "R");
        var round2 = riasecService.PreparerRound2(reponsesRound1)!;
        var reponsesRound2 = round2.Paires.ToDictionary(p => p.PaireId, p => p.OptionA.NumeroQuestion);
        var reponsesFiabilite = RepondreFiabiliteEnPreferant(riasecService, reponsesRound1, "R");

        await riasecService.RepondreAsync(utilisateur.Id, reponsesRound1, reponsesFiabilite, reponsesRound2);
        await Task.Delay(10);
        var (_, _, deuxiemeResultat) = await riasecService.RepondreAsync(utilisateur.Id, reponsesRound1, reponsesFiabilite, reponsesRound2);

        var dernier = await riasecService.GetDernierResultatAsync(utilisateur.Id);

        Assert.NotNull(dernier);
        Assert.Equal(deuxiemeResultat!.CompleteLe, dernier!.CompleteLe);
        Assert.Equal(2, await dbContext.RiasecResultats.CountAsync());
    }

    [Fact]
    public async Task GetDernierResultatAsync_RenvoieNull_SiJamaisPasse()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var riasecService = new RiasecService(dbContext, new FakeEmailService());

        var resultat = await riasecService.GetDernierResultatAsync("inconnu");

        Assert.Null(resultat);
    }
}

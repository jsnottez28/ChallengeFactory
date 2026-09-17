using Integration.TestSupport;
using Microsoft.EntityFrameworkCore;
using Web.Data;
using Web.Services;

namespace Integration.Services;

public class RiasecServiceTests
{
    [Fact]
    public void GetQuestions_Renvoie66EmplacementsSansDoublonDeSlotId_EtNexposePasLaDimension()
    {
        // 60 items originaux + 6 doublons de controle de coherence (un par dimension, cf.
        // RiasecService.NumerosControle) = 66 emplacements. La dimension mesuree par chaque
        // item n'est jamais exposee cote client (presentation "en aveugle").
        var riasecService = new RiasecService(InMemoryDbContextFactory.Create(), new FakeEmailService());

        var questions = riasecService.GetQuestions();

        Assert.Equal(66, questions.Count);
        Assert.Equal(66, questions.Select(q => q.SlotId).Distinct().Count());
        Assert.All(questions, q => Assert.False(string.IsNullOrWhiteSpace(q.Texte)));
    }

    [Fact]
    public async Task RepondreAsync_Echoue_SiAucuneActiviteCochee()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var utilisateur = new ApplicationUser { UserName = "stagiaire@test.local", Email = "stagiaire@test.local" };
        dbContext.Users.Add(utilisateur);
        await dbContext.SaveChangesAsync();

        var riasecService = new RiasecService(dbContext, new FakeEmailService());

        var (success, errorMessage, resultat) = await riasecService.RepondreAsync(utilisateur.Id, []);

        Assert.False(success);
        Assert.NotNull(errorMessage);
        Assert.Null(resultat);
        Assert.False(await dbContext.RiasecResultats.AnyAsync());
    }

    [Fact]
    public async Task RepondreAsync_CompteLesCochesParDimension_EtCalculeLeCodeHolland()
    {
        // Les questions 1-10 sont toutes "Realiste" (cf. RiasecService.Questions) : cocher
        // exactement ces 10 doit donner ScoreR=10 et tout le reste a 0. En cas d'egalite a
        // 0, le code Holland se departage par l'ordre fixe R,I,A,S,E,C -> "RIA".
        await using var dbContext = InMemoryDbContextFactory.Create();
        var utilisateur = new ApplicationUser { UserName = "stagiaire@test.local", Email = "stagiaire@test.local" };
        dbContext.Users.Add(utilisateur);
        await dbContext.SaveChangesAsync();

        var riasecService = new RiasecService(dbContext, new FakeEmailService());
        var reponses = Enumerable.Range(1, 10).ToHashSet();

        var (success, errorMessage, resultat) = await riasecService.RepondreAsync(utilisateur.Id, reponses);

        Assert.True(success, errorMessage);
        Assert.NotNull(resultat);
        Assert.Equal(10, resultat!.Dimensions.Single(d => d.Code == "R").Score);
        Assert.All(resultat.Dimensions.Where(d => d.Code != "R"), d => Assert.Equal(0, d.Score));
        Assert.Equal("RIA", resultat.CodeHolland);
    }

    [Fact]
    public async Task RepondreAsync_LesDoublonsDeControleNeComptentJamaisDansLeScoreDeDimension()
    {
        // La question 3 (Realiste) est aussi le doublon de controle au SlotId 61. Cocher
        // les deux ne doit compter qu'une seule fois pour R (le doublon ne doit jamais
        // gonfler le score au-dessus de 10).
        await using var dbContext = InMemoryDbContextFactory.Create();
        var utilisateur = new ApplicationUser { UserName = "stagiaire@test.local", Email = "stagiaire@test.local" };
        dbContext.Users.Add(utilisateur);
        await dbContext.SaveChangesAsync();

        var riasecService = new RiasecService(dbContext, new FakeEmailService());

        var (success, errorMessage, resultat) = await riasecService.RepondreAsync(utilisateur.Id, [3, 61]);

        Assert.True(success, errorMessage);
        Assert.Equal(1, resultat!.Dimensions.Single(d => d.Code == "R").Score);
    }

    [Fact]
    public async Task RepondreAsync_CalculeLaCoherenceDesReponsesSurLesPairesDeControle()
    {
        // Paires de controle : (3,61)=R, (13,62)=I, (23,63)=A, (33,64)=S, (43,65)=E,
        // (53,66)=C. On coche 3 ET 61 (paire coherente : meme reponse aux deux
        // emplacements) et 13 seul (paire incoherente : cochee a l'original, pas au
        // doublon). Les 4 autres paires ne sont cochees nulle part -> coherentes (les deux
        // a false). Total attendu : 5/6 coherentes.
        await using var dbContext = InMemoryDbContextFactory.Create();
        var utilisateur = new ApplicationUser { UserName = "stagiaire@test.local", Email = "stagiaire@test.local" };
        dbContext.Users.Add(utilisateur);
        await dbContext.SaveChangesAsync();

        var riasecService = new RiasecService(dbContext, new FakeEmailService());

        var (success, errorMessage, resultat) = await riasecService.RepondreAsync(utilisateur.Id, [3, 61, 13]);

        Assert.True(success, errorMessage);
        Assert.Equal(6, resultat!.NombrePairesControle);
        Assert.Equal(5, resultat.NombrePairesCoherentes);
    }

    [Fact]
    public async Task RepondreAsync_EnregistreLeResultatEtLenvoieParEmail()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var utilisateur = new ApplicationUser { UserName = "stagiaire@test.local", Email = "stagiaire@test.local" };
        dbContext.Users.Add(utilisateur);
        await dbContext.SaveChangesAsync();

        var emailService = new FakeEmailService();
        var riasecService = new RiasecService(dbContext, emailService);

        await riasecService.RepondreAsync(utilisateur.Id, [1, 2, 3]);

        var enBase = await dbContext.RiasecResultats.SingleAsync();
        Assert.Equal(utilisateur.Id, enBase.UtilisateurId);
        Assert.Equal(3, enBase.ScoreR);

        var envoi = Assert.Single(emailService.Envois);
        Assert.Equal("stagiaire@test.local", envoi.Destinataire);
        Assert.Contains("RIASEC", envoi.Sujet);
    }

    [Fact]
    public async Task GetDernierResultatAsync_RenvoieLePlusRecent()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var utilisateur = new ApplicationUser { UserName = "stagiaire@test.local", Email = "stagiaire@test.local" };
        dbContext.Users.Add(utilisateur);
        await dbContext.SaveChangesAsync();

        var riasecService = new RiasecService(dbContext, new FakeEmailService());

        await riasecService.RepondreAsync(utilisateur.Id, [1]);
        await Task.Delay(10);
        var (_, _, deuxiemeResultat) = await riasecService.RepondreAsync(utilisateur.Id, [11, 12]);

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

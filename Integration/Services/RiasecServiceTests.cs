using Integration.TestSupport;
using Microsoft.EntityFrameworkCore;
using Web.Data;
using Web.Services;

namespace Integration.Services;

public class RiasecServiceTests
{
    [Fact]
    public void GetQuestions_Renvoie60QuestionsAvec10ParDimension()
    {
        var riasecService = new RiasecService(InMemoryDbContextFactory.Create(), new FakeEmailService());

        var questions = riasecService.GetQuestions();

        Assert.Equal(60, questions.Count);
        var parDimension = questions.GroupBy(q => q.Dimension).ToDictionary(g => g.Key, g => g.Count());
        Assert.Equal(new[] { "A", "C", "E", "I", "R", "S" }, parDimension.Keys.OrderBy(k => k));
        Assert.All(parDimension.Values, count => Assert.Equal(10, count));
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

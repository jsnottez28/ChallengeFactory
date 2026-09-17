using Integration.TestSupport;
using Microsoft.EntityFrameworkCore;
using Web.Data;
using Web.Services;

namespace Integration.Services;

public class DiscServiceTests
{
    [Fact]
    public void GetQuestions_Renvoie25QuestionsAvec4OptionsChacune()
    {
        var discService = new DiscService(InMemoryDbContextFactory.Create(), new FakeEmailService());

        var questions = discService.GetQuestions();

        Assert.Equal(25, questions.Count);
        Assert.All(questions, q => Assert.Equal(4, q.Options.Count));
        Assert.Equal(["a", "b", "c", "d"], questions[0].Options.Select(o => o.Lettre));
    }

    [Fact]
    public async Task RepondreAsync_Echoue_SiUneQuestionNestPasRepondue()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var utilisateur = new ApplicationUser { UserName = "stagiaire@test.local", Email = "stagiaire@test.local" };
        dbContext.Users.Add(utilisateur);
        await dbContext.SaveChangesAsync();

        var discService = new DiscService(dbContext, new FakeEmailService());
        var reponses = Enumerable.Range(1, 24).ToDictionary(n => n, _ => "a"); // il manque la question 25

        var (success, errorMessage, resultat) = await discService.RepondreAsync(utilisateur.Id, reponses);

        Assert.False(success);
        Assert.NotNull(errorMessage);
        Assert.Null(resultat);
        Assert.False(await dbContext.DiscResultats.AnyAsync());
    }

    [Fact]
    public async Task RepondreAsync_CalculeLesScoresSelonLaGrilleDeReference_EtDeterminleProfilDominant()
    {
        // Reponse "a" a toutes les questions : verifie independamment (cf. grille source
        // challenges-factory.com/Test/test-disc.html) que la repartition attendue est
        // D=6, I=6, S=6, C=7 - C etant seul en tete, dominant sans ambiguite.
        await using var dbContext = InMemoryDbContextFactory.Create();
        var utilisateur = new ApplicationUser { UserName = "stagiaire@test.local", Email = "stagiaire@test.local" };
        dbContext.Users.Add(utilisateur);
        await dbContext.SaveChangesAsync();

        var discService = new DiscService(dbContext, new FakeEmailService());
        var reponses = Enumerable.Range(1, 25).ToDictionary(n => n, _ => "a");

        var (success, errorMessage, resultat) = await discService.RepondreAsync(utilisateur.Id, reponses);

        Assert.True(success, errorMessage);
        Assert.NotNull(resultat);
        Assert.Equal(6, resultat!.ScoreD);
        Assert.Equal(6, resultat.ScoreI);
        Assert.Equal(6, resultat.ScoreS);
        Assert.Equal(7, resultat.ScoreC);
        Assert.Equal("C", resultat.ProfilDominant);
        Assert.Equal("Consciencieux", resultat.ProfilDominantNom);
    }

    [Fact]
    public async Task RepondreAsync_EnregistreLeResultatEtLenvoieParEmail()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var utilisateur = new ApplicationUser { UserName = "stagiaire@test.local", Email = "stagiaire@test.local" };
        dbContext.Users.Add(utilisateur);
        await dbContext.SaveChangesAsync();

        var emailService = new FakeEmailService();
        var discService = new DiscService(dbContext, emailService);
        var reponses = Enumerable.Range(1, 25).ToDictionary(n => n, _ => "a");

        await discService.RepondreAsync(utilisateur.Id, reponses);

        var enBase = await dbContext.DiscResultats.SingleAsync();
        Assert.Equal(utilisateur.Id, enBase.UtilisateurId);

        var envoi = Assert.Single(emailService.Envois);
        Assert.Equal("stagiaire@test.local", envoi.Destinataire);
        Assert.Contains("DISC", envoi.Sujet);
    }

    [Fact]
    public async Task GetDernierResultatAsync_RenvoieLePlusRecent()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var utilisateur = new ApplicationUser { UserName = "stagiaire@test.local", Email = "stagiaire@test.local" };
        dbContext.Users.Add(utilisateur);
        await dbContext.SaveChangesAsync();

        var discService = new DiscService(dbContext, new FakeEmailService());

        var reponsesA = Enumerable.Range(1, 25).ToDictionary(n => n, _ => "a");
        var reponsesB = Enumerable.Range(1, 25).ToDictionary(n => n, _ => "b");

        await discService.RepondreAsync(utilisateur.Id, reponsesA);
        await Task.Delay(10); // garantit un CompleteLe strictement posterieur
        var (_, _, deuxiemeResultat) = await discService.RepondreAsync(utilisateur.Id, reponsesB);

        var dernier = await discService.GetDernierResultatAsync(utilisateur.Id);

        Assert.NotNull(dernier);
        Assert.Equal(deuxiemeResultat!.CompleteLe, dernier!.CompleteLe);
        Assert.Equal(2, await dbContext.DiscResultats.CountAsync());
    }

    [Fact]
    public async Task GetDernierResultatAsync_RenvoieNull_SiJamaisPasse()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var discService = new DiscService(dbContext, new FakeEmailService());

        var resultat = await discService.GetDernierResultatAsync("inconnu");

        Assert.Null(resultat);
    }
}

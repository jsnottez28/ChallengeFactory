using Integration.TestSupport;
using Microsoft.EntityFrameworkCore;
using Web.Data;
using Web.Services;

namespace Integration.Services;

public class KilmannServiceTests
{
    private static readonly string[] Modes = ["Rivaliser", "Collaborer", "Compromis", "Eviter", "Ceder"];

    [Fact]
    public void GetQuestions_Renvoie30QuestionsAvec2OptionsChacune()
    {
        var kilmannService = new KilmannService(InMemoryDbContextFactory.Create(), new FakeEmailService());

        var questions = kilmannService.GetQuestions();

        Assert.Equal(30, questions.Count);
        Assert.All(questions, q => Assert.Equal(2, q.Options.Count));
        Assert.Equal(["A", "B"], questions[0].Options.Select(o => o.Lettre));
    }

    [Fact]
    public void Calculer_Echoue_SiUneQuestionNestPasRepondue()
    {
        var kilmannService = new KilmannService(InMemoryDbContextFactory.Create(), new FakeEmailService());
        var reponses = Enumerable.Range(1, 29).ToDictionary(n => n, _ => "A"); // il manque la question 30

        var (success, errorMessage, resultat) = kilmannService.Calculer(reponses);

        Assert.False(success);
        Assert.NotNull(errorMessage);
        Assert.Null(resultat);
    }

    [Fact]
    public void Calculer_RepartitEquitablement_SiToutesLesReponsesSontA()
    {
        // Chaque mode apparait exactement 12 fois sur les 30 questions (6 fois cote A, 6 fois
        // cote B - grille source Thomas-Kilmann) : repondre "A" partout donne donc 6 points a
        // chaque mode, un cas d'egalite parfaite (cf. depart egal en points, dominant
        // arbitraire parmi les 5 - meme principe de tie-break que DiscService).
        var kilmannService = new KilmannService(InMemoryDbContextFactory.Create(), new FakeEmailService());
        var reponses = Enumerable.Range(1, 30).ToDictionary(n => n, _ => "A");

        var (success, errorMessage, resultat) = kilmannService.Calculer(reponses);

        Assert.True(success, errorMessage);
        Assert.NotNull(resultat);
        Assert.Equal(5, resultat!.Modes.Count);
        Assert.All(resultat.Modes, m => Assert.Equal(6, m.Score));
        Assert.Equal(30, resultat.Modes.Sum(m => m.Score));
        Assert.Contains(resultat.ProfilDominant, Modes);
    }

    [Fact]
    public void Calculer_NeTouchePasLaBaseDeDonnees()
    {
        // Le parcours anonyme (sans compte) appelle Calculer directement, jamais
        // RepondreAsync - aucune ligne ne doit etre ecrite en base ni d'email envoye.
        var dbContext = InMemoryDbContextFactory.Create();
        var emailService = new FakeEmailService();
        var kilmannService = new KilmannService(dbContext, emailService);
        var reponses = Enumerable.Range(1, 30).ToDictionary(n => n, _ => "A");

        var (success, _, resultat) = kilmannService.Calculer(reponses);

        Assert.True(success);
        Assert.NotNull(resultat);
        Assert.False(dbContext.KilmannResultats.Any());
        Assert.Empty(emailService.Envois);
    }

    [Fact]
    public async Task RepondreAsync_EnregistreLeResultatEtLenvoieParEmail()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var utilisateur = new ApplicationUser { UserName = "stagiaire@test.local", Email = "stagiaire@test.local" };
        dbContext.Users.Add(utilisateur);
        await dbContext.SaveChangesAsync();

        var emailService = new FakeEmailService();
        var kilmannService = new KilmannService(dbContext, emailService);
        var reponses = Enumerable.Range(1, 30).ToDictionary(n => n, _ => "A");

        var (success, errorMessage, resultat) = await kilmannService.RepondreAsync(utilisateur.Id, reponses);

        Assert.True(success, errorMessage);
        Assert.NotNull(resultat);

        var enBase = await dbContext.KilmannResultats.SingleAsync();
        Assert.Equal(utilisateur.Id, enBase.UtilisateurId);
        Assert.Equal(resultat!.ProfilDominant, enBase.ProfilDominant);

        var envoi = Assert.Single(emailService.Envois);
        Assert.Equal("stagiaire@test.local", envoi.Destinataire);
        Assert.Contains("conflits", envoi.Sujet, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task RepondreAsync_Echoue_SiLutilisateurNexistePas()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var kilmannService = new KilmannService(dbContext, new FakeEmailService());
        var reponses = Enumerable.Range(1, 30).ToDictionary(n => n, _ => "A");

        var (success, errorMessage, resultat) = await kilmannService.RepondreAsync("inconnu", reponses);

        Assert.False(success);
        Assert.NotNull(errorMessage);
        Assert.Null(resultat);
    }

    [Fact]
    public async Task GetDernierResultatAsync_RenvoieLePlusRecent()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var utilisateur = new ApplicationUser { UserName = "stagiaire@test.local", Email = "stagiaire@test.local" };
        dbContext.Users.Add(utilisateur);
        await dbContext.SaveChangesAsync();

        var kilmannService = new KilmannService(dbContext, new FakeEmailService());

        var reponsesA = Enumerable.Range(1, 30).ToDictionary(n => n, _ => "A");
        var reponsesB = Enumerable.Range(1, 30).ToDictionary(n => n, _ => "B");

        await kilmannService.RepondreAsync(utilisateur.Id, reponsesA);
        await Task.Delay(10); // garantit un CompleteLe strictement posterieur
        var (_, _, deuxiemeResultat) = await kilmannService.RepondreAsync(utilisateur.Id, reponsesB);

        var dernier = await kilmannService.GetDernierResultatAsync(utilisateur.Id);

        Assert.NotNull(dernier);
        Assert.Equal(deuxiemeResultat!.CompleteLe, dernier!.CompleteLe);
        Assert.Equal(2, await dbContext.KilmannResultats.CountAsync());
    }

    [Fact]
    public async Task GetDernierResultatAsync_RenvoieNull_SiJamaisPasse()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var kilmannService = new KilmannService(dbContext, new FakeEmailService());

        var resultat = await kilmannService.GetDernierResultatAsync("inconnu");

        Assert.Null(resultat);
    }
}

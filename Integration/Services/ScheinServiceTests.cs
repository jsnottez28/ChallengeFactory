using Integration.TestSupport;
using Microsoft.EntityFrameworkCore;
using Web.Data;
using Web.Services;

namespace Integration.Services;

public class ScheinServiceTests
{
    [Fact]
    public void GetQuestions_Renvoie45Questions()
    {
        var service = new ScheinService(InMemoryDbContextFactory.Create(), new FakeEmailService());

        var questions = service.GetQuestions();

        Assert.Equal(45, questions.Count);
    }

    [Fact]
    public async Task RepondreAsync_Echoue_SiUneAffirmationNestPasRepondue()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var utilisateur = new ApplicationUser { UserName = "stagiaire@test.local", Email = "stagiaire@test.local" };
        dbContext.Users.Add(utilisateur);
        await dbContext.SaveChangesAsync();

        var service = new ScheinService(dbContext, new FakeEmailService());
        var reponses = Enumerable.Range(1, 44).ToDictionary(n => n, _ => 3); // il manque l'item 45

        var (success, errorMessage, resultat) = await service.RepondreAsync(utilisateur.Id, reponses, [1, 2, 3]);

        Assert.False(success);
        Assert.NotNull(errorMessage);
        Assert.Null(resultat);
        Assert.False(await dbContext.ScheinResultats.AnyAsync());
    }

    [Theory]
    [InlineData(2)] // pas assez
    [InlineData(4)] // trop
    public async Task RepondreAsync_Echoue_SiLeNombreDeChoixPrioritairesNestPasExactementTrois(int nombreChoix)
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var utilisateur = new ApplicationUser { UserName = "stagiaire@test.local", Email = "stagiaire@test.local" };
        dbContext.Users.Add(utilisateur);
        await dbContext.SaveChangesAsync();

        var service = new ScheinService(dbContext, new FakeEmailService());
        var reponses = Enumerable.Range(1, 45).ToDictionary(n => n, _ => 3);
        var choix = Enumerable.Range(1, nombreChoix).ToList();

        var (success, errorMessage, resultat) = await service.RepondreAsync(utilisateur.Id, reponses, choix);

        Assert.False(success);
        Assert.NotNull(errorMessage);
        Assert.Null(resultat);
        Assert.False(await dbContext.ScheinResultats.AnyAsync());
    }

    [Fact]
    public async Task RepondreAsync_Echoue_SiUnChoixPrioritaireEstHorsBornes()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var utilisateur = new ApplicationUser { UserName = "stagiaire@test.local", Email = "stagiaire@test.local" };
        dbContext.Users.Add(utilisateur);
        await dbContext.SaveChangesAsync();

        var service = new ScheinService(dbContext, new FakeEmailService());
        var reponses = Enumerable.Range(1, 45).ToDictionary(n => n, _ => 3);

        var (success, errorMessage, resultat) = await service.RepondreAsync(utilisateur.Id, reponses, [1, 2, 46]);

        Assert.False(success);
        Assert.NotNull(errorMessage);
        Assert.Null(resultat);
    }

    [Fact]
    public async Task RepondreAsync_ReponseUniforme_DonneUnScoreEgalSurLes9Ancres_EtToutesDominantes()
    {
        // 45 items notes 3, 3 choix prioritaires isoles dans un item par ancre (1 a 3, soit
        // TECH/MG/AUT) : sans les points bonus, chaque ancre vaudrait 5x3=15 ; avec les 4
        // points ajoutes a TECH, MG et AUT, ces trois ancres remontent a 19 pendant que les 6
        // autres restent a 15 - donc TECH/MG/AUT devraient etre dominantes.
        await using var dbContext = InMemoryDbContextFactory.Create();
        var utilisateur = new ApplicationUser { UserName = "stagiaire@test.local", Email = "stagiaire@test.local" };
        dbContext.Users.Add(utilisateur);
        await dbContext.SaveChangesAsync();

        var service = new ScheinService(dbContext, new FakeEmailService());
        var reponses = Enumerable.Range(1, 45).ToDictionary(n => n, _ => 3);

        var (success, errorMessage, resultat) = await service.RepondreAsync(utilisateur.Id, reponses, [1, 2, 3]);

        Assert.True(success, errorMessage);
        var ancres = resultat!.Ancres.ToDictionary(a => a.Code);
        Assert.Equal(19, ancres["TECH"].Score);
        Assert.Equal(19, ancres["MG"].Score);
        Assert.Equal(19, ancres["AUT"].Score);
        Assert.Equal(15, ancres["SEC"].Score);
        Assert.Equal(15, ancres["CRE"].Score);
        Assert.Equal(15, ancres["CAU"].Score);
        Assert.Equal(15, ancres["DEF"].Score);
        Assert.Equal(15, ancres["VIE"].Score);
        Assert.Equal(15, ancres["INTER"].Score);

        Assert.True(ancres["TECH"].EstDominante);
        Assert.True(ancres["MG"].EstDominante);
        Assert.True(ancres["AUT"].EstDominante);
        Assert.False(ancres["SEC"].EstDominante);
    }

    [Fact]
    public async Task RepondreAsync_TroisChoixPrioritairesDistinctsSurLaMemeAncre_CumulentLesPointsBonus()
    {
        // Items 1, 10 et 19 appartiennent tous les trois a l'ancre TECH (1er item de chacune
        // des 5 lignes de la grille source, cf. ScheinService.Items) : 3 choix prioritaires
        // DISTINCTS (numeros differents) peuvent donc tous porter sur la meme ancre. Toutes
        // les affirmations notees 1 : TECH = 5x1 = 5, +4x3 (les 3 choix) = 17. Les 8 autres
        // ancres restent a 5 (aucun bonus).
        await using var dbContext = InMemoryDbContextFactory.Create();
        var utilisateur = new ApplicationUser { UserName = "stagiaire@test.local", Email = "stagiaire@test.local" };
        dbContext.Users.Add(utilisateur);
        await dbContext.SaveChangesAsync();

        var service = new ScheinService(dbContext, new FakeEmailService());
        var reponses = Enumerable.Range(1, 45).ToDictionary(n => n, _ => 1);

        var (success, errorMessage, resultat) = await service.RepondreAsync(utilisateur.Id, reponses, [1, 10, 19]);

        Assert.True(success, errorMessage);
        var ancres = resultat!.Ancres.ToDictionary(a => a.Code);
        Assert.Equal(17, ancres["TECH"].Score);
        Assert.True(ancres["TECH"].EstDominante);
        Assert.Equal(5, ancres["MG"].Score);
        Assert.False(ancres["MG"].EstDominante);
    }

    [Fact]
    public async Task RepondreAsync_Echoue_SiLesChoixPrioritairesNeSontPasDistincts()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var utilisateur = new ApplicationUser { UserName = "stagiaire@test.local", Email = "stagiaire@test.local" };
        dbContext.Users.Add(utilisateur);
        await dbContext.SaveChangesAsync();

        var service = new ScheinService(dbContext, new FakeEmailService());
        var reponses = Enumerable.Range(1, 45).ToDictionary(n => n, _ => 3);

        // Le meme numero repete 3 fois : Distinct() le reduit a 1 seul choix, ce qui ne
        // satisfait pas l'exigence d'exactement 3 choix distincts.
        var (success, errorMessage, resultat) = await service.RepondreAsync(utilisateur.Id, reponses, [1, 1, 1]);

        Assert.False(success);
        Assert.NotNull(errorMessage);
        Assert.Null(resultat);
    }

    [Fact]
    public async Task RepondreAsync_EnregistreLeResultatEtLenvoieParEmail()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var utilisateur = new ApplicationUser { UserName = "stagiaire@test.local", Email = "stagiaire@test.local" };
        dbContext.Users.Add(utilisateur);
        await dbContext.SaveChangesAsync();

        var emailService = new FakeEmailService();
        var service = new ScheinService(dbContext, emailService);
        var reponses = Enumerable.Range(1, 45).ToDictionary(n => n, _ => 3);

        await service.RepondreAsync(utilisateur.Id, reponses, [1, 2, 3]);

        var enBase = await dbContext.ScheinResultats.SingleAsync();
        Assert.Equal(utilisateur.Id, enBase.UtilisateurId);
        Assert.Equal(19, enBase.ScoreTechnique);

        var envoi = Assert.Single(emailService.Envois);
        Assert.Equal("stagiaire@test.local", envoi.Destinataire);
        Assert.Contains("ancres", envoi.Sujet, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GetDernierResultatAsync_RenvoieLePlusRecent()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var utilisateur = new ApplicationUser { UserName = "stagiaire@test.local", Email = "stagiaire@test.local" };
        dbContext.Users.Add(utilisateur);
        await dbContext.SaveChangesAsync();

        var service = new ScheinService(dbContext, new FakeEmailService());
        var reponsesA = Enumerable.Range(1, 45).ToDictionary(n => n, _ => 2);
        var reponsesB = Enumerable.Range(1, 45).ToDictionary(n => n, _ => 4);

        await service.RepondreAsync(utilisateur.Id, reponsesA, [1, 2, 3]);
        await Task.Delay(10); // garantit un CompleteLe strictement posterieur
        var (_, _, deuxiemeResultat) = await service.RepondreAsync(utilisateur.Id, reponsesB, [4, 5, 6]);

        var dernier = await service.GetDernierResultatAsync(utilisateur.Id);

        Assert.NotNull(dernier);
        Assert.Equal(deuxiemeResultat!.CompleteLe, dernier!.CompleteLe);
        Assert.Equal(2, await dbContext.ScheinResultats.CountAsync());
    }

    [Fact]
    public async Task GetDernierResultatAsync_RenvoieNull_SiJamaisPasse()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var service = new ScheinService(dbContext, new FakeEmailService());

        var resultat = await service.GetDernierResultatAsync("inconnu");

        Assert.Null(resultat);
    }
}

using Integration.TestSupport;
using Microsoft.EntityFrameworkCore;
using Web.Data;
using Web.Services;

namespace Integration.Services;

public class BigFiveServiceTests
{
    [Fact]
    public void GetQuestions_Renvoie58QuestionsHorsLaFacetteSensibleO6()
    {
        var service = new BigFiveService(InMemoryDbContextFactory.Create(), new FakeEmailService());

        var questions = service.GetQuestions();

        // 30 facettes x 2 items = 60 dans l'instrument d'origine (IPIP-NEO-60), moins les 2
        // items de la facette O6 (Liberalism, donnees sensibles RGPD) volontairement exclue
        // - cf. BigFiveService.
        Assert.Equal(58, questions.Count);
    }

    [Fact]
    public async Task RepondreAsync_Echoue_SiUneAffirmationNestPasRepondue()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var utilisateur = new ApplicationUser { UserName = "stagiaire@test.local", Email = "stagiaire@test.local" };
        dbContext.Users.Add(utilisateur);
        await dbContext.SaveChangesAsync();

        var service = new BigFiveService(dbContext, new FakeEmailService());
        var reponses = Enumerable.Range(1, 57).ToDictionary(n => n, _ => 3); // il manque l'item 58

        var (success, errorMessage, resultat) = await service.RepondreAsync(utilisateur.Id, reponses);

        Assert.False(success);
        Assert.NotNull(errorMessage);
        Assert.Null(resultat);
        Assert.False(await dbContext.BigFiveResultats.AnyAsync());
    }

    [Fact]
    public async Task RepondreAsync_ReponseNeutrePartout_DonneUnProfilModereSurTousLesDomaines()
    {
        // Repondre 3 ("Neutre") a toutes les affirmations : l'Indice d'Acquiescement (IA)
        // vaut exactement 3, donc CalculerValeurCorrigee se reduit a 3 pour chaque item
        // (3+(3-3)=3 pour un item normal, 3-(3-3)=3 pour un item inverse) - chaque facette
        // (2 items) vaut 2x3=6, chaque domaine est exactement au milieu de son echelle
        // theorique (36/[12,60] pour N/E/A/C, 30/[10,50] pour O) : "Modéré" partout.
        await using var dbContext = InMemoryDbContextFactory.Create();
        var utilisateur = new ApplicationUser { UserName = "stagiaire@test.local", Email = "stagiaire@test.local" };
        dbContext.Users.Add(utilisateur);
        await dbContext.SaveChangesAsync();

        var service = new BigFiveService(dbContext, new FakeEmailService());
        var reponses = Enumerable.Range(1, 58).ToDictionary(n => n, _ => 3);

        var (success, errorMessage, resultat) = await service.RepondreAsync(utilisateur.Id, reponses);

        Assert.True(success, errorMessage);
        Assert.Equal(3m, resultat!.IndiceAcquiescement);
        var domaines = resultat.Domaines.ToDictionary(d => d.Code);
        Assert.Equal(36m, domaines["N"].Score);
        Assert.Equal(36m, domaines["E"].Score);
        Assert.Equal(30m, domaines["O"].Score);
        Assert.Equal(36m, domaines["A"].Score);
        Assert.Equal(36m, domaines["C"].Score);
        Assert.All(domaines.Values, d => Assert.Equal("Modéré", d.Niveau));
    }

    [Theory]
    [InlineData(5)] // tendance "toujours d'accord"
    [InlineData(1)] // tendance "jamais d'accord"
    public async Task RepondreAsync_ReponseUniforme_CorrigeLeBiaisDacquiescenceEtDonneUnProfilNeutre(int noteUniforme)
    {
        // Repondre la MEME note a toutes les 58 affirmations (independamment du sens de
        // chaque item) ne contient aucun signal sur le contenu - uniquement une tendance a
        // repondre toujours pareil (biais d'acquiescence). L'IA vaut alors exactement cette
        // note, donc CalculerValeurCorrigee se reduit a 3 pour CHAQUE item (ecart nul a
        // l'IA) : sans la correction, une reponse uniforme a 5 produirait un profil a
        // l'Extraversion/Ouverture maximale et au Nevrosisme maximal (biais pur, aucun sens
        // psychologique) ; avec la correction, le profil ressort neutre - la preuve que le
        // biais est bien neutralise.
        await using var dbContext = InMemoryDbContextFactory.Create();
        var utilisateur = new ApplicationUser { UserName = "stagiaire@test.local", Email = "stagiaire@test.local" };
        dbContext.Users.Add(utilisateur);
        await dbContext.SaveChangesAsync();

        var service = new BigFiveService(dbContext, new FakeEmailService());
        var reponses = Enumerable.Range(1, 58).ToDictionary(n => n, _ => noteUniforme);

        var (success, errorMessage, resultat) = await service.RepondreAsync(utilisateur.Id, reponses);

        Assert.True(success, errorMessage);
        Assert.Equal((decimal)noteUniforme, resultat!.IndiceAcquiescement);
        var domaines = resultat.Domaines.ToDictionary(d => d.Code);
        Assert.Equal(36m, domaines["N"].Score);
        Assert.Equal(36m, domaines["E"].Score);
        Assert.Equal(30m, domaines["O"].Score);
        Assert.Equal(36m, domaines["A"].Score);
        Assert.Equal(36m, domaines["C"].Score);
        Assert.All(domaines.Values, d => Assert.Equal("Modéré", d.Niveau));
    }

    [Fact]
    public async Task RepondreAsync_DetecteUnSignalReelMalgreUneTendanceModereeAlacquiescement()
    {
        // 34 items (N, O, C) a 4, les 12 items d'Extraversion a 5, les 12 items
        // d'Agreabilite a 3 -> IA = (34x4 + 12x5 + 12x3)/58 = 232/58 = 4.0 exactement.
        // Valeurs attendues verifiees independamment (calcul a la main, cf. commentaire du
        // test) : Extraversion ressort "Élevé" (46/[12,60]), Agreabilite au milieu mais
        // legerement tiree vers le bas par le calcul exact (36/[12,60], "Modéré"), N/O/C
        // exactement au milieu puisque leur note (4) egale l'IA (4) - aucun ecart, donc
        // aucune contribution au-dela du neutre (3 par item).
        await using var dbContext = InMemoryDbContextFactory.Create();
        var utilisateur = new ApplicationUser { UserName = "stagiaire@test.local", Email = "stagiaire@test.local" };
        dbContext.Users.Add(utilisateur);
        await dbContext.SaveChangesAsync();

        var service = new BigFiveService(dbContext, new FakeEmailService());
        var questions = service.GetQuestions();
        var reponses = new Dictionary<int, int>();
        foreach (var question in questions)
        {
            // Les 12 premiers numeros correspondent a N (N1..N6), les 12 suivants a E
            // (E1..E6), cf. l'ordre du tableau Items dans BigFiveService.
            if (question.NumeroQuestion is >= 13 and <= 24)
            {
                reponses[question.NumeroQuestion] = 5; // E
            }
            else if (question.NumeroQuestion is >= 35 and <= 46)
            {
                reponses[question.NumeroQuestion] = 3; // A (items 35-46 : apres N=12, E=12, O=10)
            }
            else
            {
                reponses[question.NumeroQuestion] = 4; // N, O, C
            }
        }

        var (success, errorMessage, resultat) = await service.RepondreAsync(utilisateur.Id, reponses);

        Assert.True(success, errorMessage);
        Assert.Equal(4.0m, resultat!.IndiceAcquiescement);
        var domaines = resultat.Domaines.ToDictionary(d => d.Code);
        Assert.Equal(36m, domaines["N"].Score);
        Assert.Equal(46m, domaines["E"].Score);
        Assert.Equal(30m, domaines["O"].Score);
        Assert.Equal(36m, domaines["A"].Score);
        Assert.Equal(36m, domaines["C"].Score);
        Assert.Equal("Élevé", domaines["E"].Niveau);
        Assert.Equal("Modéré", domaines["A"].Niveau);
    }

    [Fact]
    public async Task RepondreAsync_CalculeChaqueFacetteSurDeuxItems_EtExclutO6()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var utilisateur = new ApplicationUser { UserName = "stagiaire@test.local", Email = "stagiaire@test.local" };
        dbContext.Users.Add(utilisateur);
        await dbContext.SaveChangesAsync();

        var service = new BigFiveService(dbContext, new FakeEmailService());
        var reponses = Enumerable.Range(1, 58).ToDictionary(n => n, _ => 3);

        var (success, errorMessage, resultat) = await service.RepondreAsync(utilisateur.Id, reponses);

        Assert.True(success, errorMessage);
        var domaineN = resultat!.Domaines.Single(d => d.Code == "N");
        Assert.Equal(6, domaineN.Facettes.Count);
        Assert.All(domaineN.Facettes, f => Assert.Equal(6m, f.Score)); // 2 items x 3 = 6
        var domaineO = resultat.Domaines.Single(d => d.Code == "O");
        Assert.Equal(5, domaineO.Facettes.Count); // O6 exclue
        Assert.DoesNotContain(domaineO.Facettes, f => f.Code == "O6");
    }

    [Fact]
    public async Task RepondreAsync_EnregistreLeResultatEtLenvoieParEmail()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var utilisateur = new ApplicationUser { UserName = "stagiaire@test.local", Email = "stagiaire@test.local" };
        dbContext.Users.Add(utilisateur);
        await dbContext.SaveChangesAsync();

        var emailService = new FakeEmailService();
        var service = new BigFiveService(dbContext, emailService);
        var reponses = Enumerable.Range(1, 58).ToDictionary(n => n, _ => 3);

        await service.RepondreAsync(utilisateur.Id, reponses);

        var enBase = await dbContext.BigFiveResultats.Include(r => r.Facettes).SingleAsync();
        Assert.Equal(utilisateur.Id, enBase.UtilisateurId);
        Assert.Equal(29, enBase.Facettes.Count); // 30 facettes de l'instrument d'origine, moins O6
        Assert.Equal(3m, enBase.IndiceAcquiescement);

        var envoi = Assert.Single(emailService.Envois);
        Assert.Equal("stagiaire@test.local", envoi.Destinataire);
        Assert.Contains("Big Five", envoi.Sujet);
    }

    [Fact]
    public async Task GetDernierResultatAsync_RenvoieLePlusRecent()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var utilisateur = new ApplicationUser { UserName = "stagiaire@test.local", Email = "stagiaire@test.local" };
        dbContext.Users.Add(utilisateur);
        await dbContext.SaveChangesAsync();

        var service = new BigFiveService(dbContext, new FakeEmailService());
        var reponsesA = Enumerable.Range(1, 58).ToDictionary(n => n, _ => 2);
        var reponsesB = Enumerable.Range(1, 58).ToDictionary(n => n, _ => 4);

        await service.RepondreAsync(utilisateur.Id, reponsesA);
        await Task.Delay(10); // garantit un CompleteLe strictement posterieur
        var (_, _, deuxiemeResultat) = await service.RepondreAsync(utilisateur.Id, reponsesB);

        var dernier = await service.GetDernierResultatAsync(utilisateur.Id);

        Assert.NotNull(dernier);
        Assert.Equal(deuxiemeResultat!.CompleteLe, dernier!.CompleteLe);
        Assert.Equal(2, await dbContext.BigFiveResultats.CountAsync());
    }

    [Fact]
    public async Task GetDernierResultatAsync_RenvoieNull_SiJamaisPasse()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var service = new BigFiveService(dbContext, new FakeEmailService());

        var resultat = await service.GetDernierResultatAsync("inconnu");

        Assert.Null(resultat);
    }
}

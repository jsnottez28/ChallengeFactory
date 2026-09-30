using Integration.TestSupport;
using Microsoft.EntityFrameworkCore;
using Web.Data;
using Web.Services;

namespace Integration.Services;

public class BigFiveServiceTests
{
    [Fact]
    public void GetQuestions_Renvoie116QuestionsHorsLaFacetteSensibleO6()
    {
        var service = new BigFiveService(InMemoryDbContextFactory.Create(), new FakeEmailService());

        var questions = service.GetQuestions();

        // 30 facettes x 4 items = 120 dans l'instrument d'origine (IPIP-NEO-120), moins les
        // 4 items de la facette O6 (Liberalism, donnee sensible RGPD) volontairement exclue
        // - cf. BigFiveService.
        Assert.Equal(116, questions.Count);
    }

    [Fact]
    public async Task RepondreAsync_Echoue_SiUneAffirmationNestPasRepondue()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var utilisateur = new ApplicationUser { UserName = "stagiaire@test.local", Email = "stagiaire@test.local" };
        dbContext.Users.Add(utilisateur);
        await dbContext.SaveChangesAsync();

        var service = new BigFiveService(dbContext, new FakeEmailService());
        var reponses = Enumerable.Range(1, 115).ToDictionary(n => n, _ => 3); // il manque l'item 116

        var (success, errorMessage, resultat) = await service.RepondreAsync(utilisateur.Id, reponses);

        Assert.False(success);
        Assert.NotNull(errorMessage);
        Assert.Null(resultat);
        Assert.False(await dbContext.BigFiveResultats.AnyAsync());
    }

    [Fact]
    public async Task RepondreAsync_RecodeLesItemsInverses_AvantDeSommerLesScoresDeDomaine()
    {
        // Repondre 5 ("Très exact") a toutes les affirmations : les items positivement
        // gardes comptent 5, les items negativement gardes sont recodes en (6-5)=1 avant
        // sommation. Valeurs attendues verifiees independamment (cf. script de controle
        // sur le tableau Items) : N=92, E=96, O=60, A=52, C=68 sur les echelles theoriques
        // [24,120] pour N/E/A/C et [20,100] pour O (O6 exclue).
        await using var dbContext = InMemoryDbContextFactory.Create();
        var utilisateur = new ApplicationUser { UserName = "stagiaire@test.local", Email = "stagiaire@test.local" };
        dbContext.Users.Add(utilisateur);
        await dbContext.SaveChangesAsync();

        var service = new BigFiveService(dbContext, new FakeEmailService());
        var reponses = Enumerable.Range(1, 116).ToDictionary(n => n, _ => 5);

        var (success, errorMessage, resultat) = await service.RepondreAsync(utilisateur.Id, reponses);

        Assert.True(success, errorMessage);
        Assert.NotNull(resultat);
        var domaines = resultat!.Domaines.ToDictionary(d => d.Code);
        Assert.Equal(92, domaines["N"].Score);
        Assert.Equal(96, domaines["E"].Score);
        Assert.Equal(60, domaines["O"].Score);
        Assert.Equal(52, domaines["A"].Score);
        Assert.Equal(68, domaines["C"].Score);

        // Niveaux derives des tiers de l'echelle theorique (cf. BigFiveService.CalculerNiveau) :
        // N (92/96 = 71%) et E (96/96 = 75%) Élevé, O (60/80 = 50%) Modéré, A (52/96 = 29%)
        // Faible, C (68/96 = 46%) Modéré.
        Assert.Equal("Élevé", domaines["N"].Niveau);
        Assert.Equal("Élevé", domaines["E"].Niveau);
        Assert.Equal("Modéré", domaines["O"].Niveau);
        Assert.Equal("Faible", domaines["A"].Niveau);
        Assert.Equal("Modéré", domaines["C"].Niveau);

        // Synthese : seul E est "Élevé" parmi E/O/A/C -> seul point fort ; N "Élevé" -> seul
        // point de vigilance (c'est le seul domaine dont un score eleve signale une
        // vigilance plutot qu'un atout, cf. BigFiveService.ConstruireSynthese).
        Assert.Single(resultat.Synthese.PointsForts);
        Assert.Contains("Extraversion", resultat.Synthese.PointsForts[0]);
        Assert.Single(resultat.Synthese.PointsVigilance);
        Assert.Contains("Névrosisme", resultat.Synthese.PointsVigilance[0]);
    }

    [Fact]
    public async Task RepondreAsync_CalculeChaqueFacetteSur4A20_EtLesRattacheAuBonDomaine()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var utilisateur = new ApplicationUser { UserName = "stagiaire@test.local", Email = "stagiaire@test.local" };
        dbContext.Users.Add(utilisateur);
        await dbContext.SaveChangesAsync();

        var service = new BigFiveService(dbContext, new FakeEmailService());
        var reponses = Enumerable.Range(1, 116).ToDictionary(n => n, _ => 3); // reponse neutre partout

        var (success, errorMessage, resultat) = await service.RepondreAsync(utilisateur.Id, reponses);

        Assert.True(success, errorMessage);
        var domaineN = resultat!.Domaines.Single(d => d.Code == "N");
        Assert.Equal(6, domaineN.Facettes.Count);
        Assert.All(domaineN.Facettes, f => Assert.Equal(12, f.Score)); // 4 items x 3 (recode de 3 = 3) = 12, quel que soit le sens
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
        var reponses = Enumerable.Range(1, 116).ToDictionary(n => n, _ => 3);

        await service.RepondreAsync(utilisateur.Id, reponses);

        var enBase = await dbContext.BigFiveResultats.Include(r => r.Facettes).SingleAsync();
        Assert.Equal(utilisateur.Id, enBase.UtilisateurId);
        Assert.Equal(29, enBase.Facettes.Count); // 30 facettes de l'instrument d'origine, moins O6

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
        var reponsesA = Enumerable.Range(1, 116).ToDictionary(n => n, _ => 2);
        var reponsesB = Enumerable.Range(1, 116).ToDictionary(n => n, _ => 4);

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

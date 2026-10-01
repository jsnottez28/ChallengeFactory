using Integration.TestSupport;
using Microsoft.EntityFrameworkCore;
using Web.Data;
using Web.Services;

namespace Integration.Services;

public class BigFiveServiceTests
{
    // Reponses de fiabilite "par defaut" (toujours OptionA) - utilisees dans les tests qui ne
    // portent pas specifiquement sur le calcul de coherence, juste pour fournir un round de
    // fiabilite structurellement valide.
    private static Dictionary<int, int> ReponsesFiabiliteDefaut(BigFiveService service) =>
        service.GetPairesFiabilite().ToDictionary(p => p.PaireId, p => p.OptionA.NumeroQuestion);

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
    public void GetPairesFiabilite_Renvoie5PairesUneParDomaine_AvecDesItemsReelsDistincts()
    {
        var service = new BigFiveService(InMemoryDbContextFactory.Create(), new FakeEmailService());
        var questions = service.GetQuestions().ToDictionary(q => q.NumeroQuestion, q => q.Texte);

        var paires = service.GetPairesFiabilite();

        Assert.Equal(5, paires.Count);
        Assert.All(paires, p =>
        {
            // Les deux options d'une paire sont toujours deux items reels et distincts du
            // questionnaire (jamais de texte invente), jamais la meme question des deux cotes.
            Assert.NotEqual(p.OptionA.NumeroQuestion, p.OptionB.NumeroQuestion);
            Assert.Equal(questions[p.OptionA.NumeroQuestion], p.OptionA.Texte);
            Assert.Equal(questions[p.OptionB.NumeroQuestion], p.OptionB.Texte);
        });
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

        var (success, errorMessage, resultat) = await service.RepondreAsync(utilisateur.Id, reponses, ReponsesFiabiliteDefaut(service));

        Assert.False(success);
        Assert.NotNull(errorMessage);
        Assert.Null(resultat);
        Assert.False(await dbContext.BigFiveResultats.AnyAsync());
    }

    [Fact]
    public async Task RepondreAsync_Echoue_SiLesPairesDeFiabiliteNeSontPasToutesRepondues()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var utilisateur = new ApplicationUser { UserName = "stagiaire@test.local", Email = "stagiaire@test.local" };
        dbContext.Users.Add(utilisateur);
        await dbContext.SaveChangesAsync();

        var service = new BigFiveService(dbContext, new FakeEmailService());
        var reponses = Enumerable.Range(1, 58).ToDictionary(n => n, _ => 3);
        var reponsesFiabilite = ReponsesFiabiliteDefaut(service);
        reponsesFiabilite.Remove(reponsesFiabilite.Keys.First()); // il manque une paire

        var (success, errorMessage, resultat) = await service.RepondreAsync(utilisateur.Id, reponses, reponsesFiabilite);

        Assert.False(success);
        Assert.NotNull(errorMessage);
        Assert.Null(resultat);
        Assert.False(await dbContext.BigFiveResultats.AnyAsync());
    }

    [Fact]
    public async Task RepondreAsync_CalculeLaCoherenceFiabilite_EnComparantAuxMemesItemsSurLechelleLikert()
    {
        // Les 5 paires de fiabilite opposent les items (1,5), (13,21), (25,27), (35,39),
        // (47,53) - cf. BigFiveService.PairesFiabiliteBase. On fixe des notes Likert
        // tranchees pour ces 10 items precis (le reste a 3, neutre, sans incidence sur ce
        // test) puis on verifie que le choix force "attendu" (celui qui a la note la plus
        // haute, egalite departagee vers OptionA) est bien celui compte comme coherent.
        await using var dbContext = InMemoryDbContextFactory.Create();
        var utilisateur = new ApplicationUser { UserName = "stagiaire@test.local", Email = "stagiaire@test.local" };
        dbContext.Users.Add(utilisateur);
        await dbContext.SaveChangesAsync();

        var service = new BigFiveService(dbContext, new FakeEmailService());
        var reponses = Enumerable.Range(1, 58).ToDictionary(n => n, _ => 3);
        reponses[1] = 5; reponses[5] = 2;   // paire 1 : item 1 gagne
        reponses[13] = 2; reponses[21] = 5; // paire 2 : item 21 gagne
        reponses[25] = 3; reponses[27] = 3; // paire 3 : egalite -> attendu = OptionA (25)
        reponses[35] = 4; reponses[39] = 1; // paire 4 : item 35 gagne
        reponses[47] = 1; reponses[53] = 5; // paire 5 : item 53 gagne

        var reponsesCoherentes = new Dictionary<int, int> { [1] = 1, [2] = 21, [3] = 25, [4] = 35, [5] = 53 };
        var (success, errorMessage, resultatCoherent) = await service.RepondreAsync(utilisateur.Id, reponses, reponsesCoherentes);
        Assert.True(success, errorMessage);
        Assert.Equal(5, resultatCoherent!.NombrePairesCoherentes);
        Assert.Equal(5, resultatCoherent.NombrePairesControle);

        // Meme reponses Likert, mais choix forces tous opposes a l'attendu : 0 paire coherente.
        var reponsesIncoherentes = new Dictionary<int, int> { [1] = 5, [2] = 13, [3] = 27, [4] = 39, [5] = 47 };
        var (success2, errorMessage2, resultatIncoherent) = await service.RepondreAsync(utilisateur.Id, reponses, reponsesIncoherentes);
        Assert.True(success2, errorMessage2);
        Assert.Equal(0, resultatIncoherent!.NombrePairesCoherentes);
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

        var (success, errorMessage, resultat) = await service.RepondreAsync(utilisateur.Id, reponses, ReponsesFiabiliteDefaut(service));

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

        var (success, errorMessage, resultat) = await service.RepondreAsync(utilisateur.Id, reponses, ReponsesFiabiliteDefaut(service));

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
    public async Task RepondreAsync_DetecteUnSignalReelMalgreUneTendanceModereeAlacquiescence()
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

        var (success, errorMessage, resultat) = await service.RepondreAsync(utilisateur.Id, reponses, ReponsesFiabiliteDefaut(service));

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

        var (success, errorMessage, resultat) = await service.RepondreAsync(utilisateur.Id, reponses, ReponsesFiabiliteDefaut(service));

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

        await service.RepondreAsync(utilisateur.Id, reponses, ReponsesFiabiliteDefaut(service));

        var enBase = await dbContext.BigFiveResultats.Include(r => r.Facettes).SingleAsync();
        Assert.Equal(utilisateur.Id, enBase.UtilisateurId);
        Assert.Equal(29, enBase.Facettes.Count); // 30 facettes de l'instrument d'origine, moins O6
        Assert.Equal(3m, enBase.IndiceAcquiescement);
        Assert.Equal(5, enBase.NombrePairesControle);

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
        var reponsesFiabilite = ReponsesFiabiliteDefaut(service);
        var reponsesA = Enumerable.Range(1, 58).ToDictionary(n => n, _ => 2);
        var reponsesB = Enumerable.Range(1, 58).ToDictionary(n => n, _ => 4);

        await service.RepondreAsync(utilisateur.Id, reponsesA, reponsesFiabilite);
        await Task.Delay(10); // garantit un CompleteLe strictement posterieur
        var (_, _, deuxiemeResultat) = await service.RepondreAsync(utilisateur.Id, reponsesB, reponsesFiabilite);

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

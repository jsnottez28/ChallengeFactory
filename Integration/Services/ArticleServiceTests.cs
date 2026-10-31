using Application.Common.Interfaces;
using Domain.Entities;
using Integration.TestSupport;
using Microsoft.EntityFrameworkCore;
using Web.Services;

namespace Integration.Services;

public class ArticleServiceTests
{
    private static ArticleInput InputValide(string titre = "Mon premier article", string slug = "mon-premier-article") => new()
    {
        Titre = titre,
        Slug = slug,
        Resume = "Un résumé court.",
        MetaDescription = "Une meta description de moins de 160 caractères.",
        ContenuMarkdown = "# Titre\n\nUn paragraphe.",
        Auteur = "Jean-Sébastien Nottez",
        DatePublication = DateTime.UtcNow.AddDays(-1),
        Statut = StatutArticle.Publie,
    };

    [Theory]
    [InlineData("Décarbonation & Équipes : le Guide 2026 !", "decarbonation-equipes-le-guide-2026")]
    [InlineData("  Espaces   multiples  ", "espaces-multiples")]
    [InlineData("Café, thé... ou l'inverse ?", "cafe-the-ou-l-inverse")]
    public void GenererSlug_NormaliseLeTitre(string titre, string slugAttendu)
    {
        var service = new ArticleService(InMemoryDbContextFactory.Create());

        Assert.Equal(slugAttendu, service.GenererSlug(titre));
    }

    [Fact]
    public async Task CreateAsync_Reussit_AvecUnInputValide()
    {
        var service = new ArticleService(InMemoryDbContextFactory.Create());

        var (success, errorMessage, article) = await service.CreateAsync(InputValide());

        Assert.True(success, errorMessage);
        Assert.NotNull(article);
        Assert.Equal("mon-premier-article", article!.Slug);
    }

    [Theory]
    [InlineData("", "mon-premier-article")] // titre manquant
    [InlineData("Titre", "")] // slug manquant
    public async Task CreateAsync_Echoue_SiChampObligatoireManquant(string titre, string slug)
    {
        var service = new ArticleService(InMemoryDbContextFactory.Create());
        var input = InputValide(titre.Length == 0 ? "x" : titre, slug.Length == 0 ? "x" : slug);
        if (titre.Length == 0) input.Titre = titre;
        if (slug.Length == 0) input.Slug = slug;

        var (success, errorMessage, article) = await service.CreateAsync(input);

        Assert.False(success);
        Assert.NotNull(errorMessage);
        Assert.Null(article);
    }

    [Fact]
    public async Task CreateAsync_Echoue_SiMetaDescriptionTropLongue()
    {
        var service = new ArticleService(InMemoryDbContextFactory.Create());
        var input = InputValide();
        input.MetaDescription = new string('a', 161);

        var (success, errorMessage, _) = await service.CreateAsync(input);

        Assert.False(success);
        Assert.Contains("160", errorMessage);
    }

    [Theory]
    [InlineData("Mon Slug")]
    [InlineData("mon_slug")]
    [InlineData("-mon-slug")]
    public async Task CreateAsync_Echoue_SiSlugMalForme(string slugInvalide)
    {
        var service = new ArticleService(InMemoryDbContextFactory.Create());

        var (success, errorMessage, _) = await service.CreateAsync(InputValide(slug: slugInvalide));

        Assert.False(success);
        Assert.NotNull(errorMessage);
    }

    [Fact]
    public async Task CreateAsync_Echoue_SiSlugEstRss()
    {
        var service = new ArticleService(InMemoryDbContextFactory.Create());

        var (success, errorMessage, _) = await service.CreateAsync(InputValide(slug: "rss"));

        Assert.False(success);
        Assert.Contains("rss", errorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CreateAsync_Echoue_SiLeSlugExisteDeja()
    {
        var service = new ArticleService(InMemoryDbContextFactory.Create());
        await service.CreateAsync(InputValide());

        var (success, errorMessage, _) = await service.CreateAsync(InputValide(titre: "Autre titre"));

        Assert.False(success);
        Assert.Contains("existe déjà", errorMessage);
    }

    [Fact]
    public async Task UpdateAsync_AutoriseAGarderSonPropreSlug()
    {
        var service = new ArticleService(InMemoryDbContextFactory.Create());
        var (_, _, article) = await service.CreateAsync(InputValide());

        var input = InputValide();
        input.Resume = "Résumé mis à jour.";
        var (success, errorMessage) = await service.UpdateAsync(article!.Id, input);

        Assert.True(success, errorMessage);
    }

    [Fact]
    public async Task DeleteAsync_SupprimeEgalementLesSources()
    {
        var dbContext = InMemoryDbContextFactory.Create();
        var service = new ArticleService(dbContext);
        var input = InputValide();
        input.Sources = [new ArticleSourceInput { Libelle = "Source", Url = "https://exemple.fr" }];
        var (_, _, article) = await service.CreateAsync(input);

        await service.DeleteAsync(article!.Id);

        Assert.False(await dbContext.Articles.AnyAsync());
        Assert.False(await dbContext.ArticleSources.AnyAsync());
    }

    [Fact]
    public void DeterminerStatutAffiche_RenvoieBrouillon()
    {
        var service = new ArticleService(InMemoryDbContextFactory.Create());
        var article = new Article { Statut = StatutArticle.Brouillon, DatePublication = DateTime.UtcNow };

        Assert.Equal("Brouillon", service.DeterminerStatutAffiche(article));
    }

    [Fact]
    public void DeterminerStatutAffiche_RenvoieProgramme_SiDateFuture()
    {
        var service = new ArticleService(InMemoryDbContextFactory.Create());
        var article = new Article { Statut = StatutArticle.Publie, DatePublication = DateTime.UtcNow.AddDays(5) };

        Assert.Equal("Programmé", service.DeterminerStatutAffiche(article));
    }

    [Fact]
    public void DeterminerStatutAffiche_RenvoiePublie_SiDatePassee()
    {
        var service = new ArticleService(InMemoryDbContextFactory.Create());
        var article = new Article { Statut = StatutArticle.Publie, DatePublication = DateTime.UtcNow.AddDays(-5) };

        Assert.Equal("Publié", service.DeterminerStatutAffiche(article));
    }

    [Fact]
    public async Task GetPublicsAsync_ExclutLesBrouillonsEtLesArticlesProgrammesDansLeFutur()
    {
        var service = new ArticleService(InMemoryDbContextFactory.Create());

        var publie = InputValide(titre: "Publié", slug: "publie");
        publie.Statut = StatutArticle.Publie;
        publie.DatePublication = DateTime.UtcNow.AddDays(-1);
        await service.CreateAsync(publie);

        var brouillon = InputValide(titre: "Brouillon", slug: "brouillon");
        brouillon.Statut = StatutArticle.Brouillon;
        await service.CreateAsync(brouillon);

        var programme = InputValide(titre: "Programmé", slug: "programme");
        programme.Statut = StatutArticle.Publie;
        programme.DatePublication = DateTime.UtcNow.AddDays(5);
        await service.CreateAsync(programme);

        var (articles, total) = await service.GetPublicsAsync(1, 20);

        Assert.Equal(1, total);
        Assert.Equal("publie", Assert.Single(articles).Slug);
    }

    [Fact]
    public async Task GetParSlugAsync_RenvoieLArticleMemeEnBrouillon()
    {
        // Utilise par l'apercu admin (cf. ArticlesController.Details) : contrairement a
        // GetPublicParSlugAsync, aucun filtre de statut/date.
        var service = new ArticleService(InMemoryDbContextFactory.Create());
        var input = InputValide();
        input.Statut = StatutArticle.Brouillon;
        await service.CreateAsync(input);

        var resultat = await service.GetParSlugAsync("mon-premier-article");

        Assert.NotNull(resultat);
        Assert.Equal(StatutArticle.Brouillon, resultat!.Statut);
    }

    [Fact]
    public async Task GetPublicParSlugAsync_RenvoieNull_SiBrouillonOuProgramme()
    {
        var service = new ArticleService(InMemoryDbContextFactory.Create());
        var input = InputValide();
        input.Statut = StatutArticle.Brouillon;
        await service.CreateAsync(input);

        var resultat = await service.GetPublicParSlugAsync("mon-premier-article");

        Assert.Null(resultat);
    }

    [Fact]
    public void RendreHtml_ConvertitLeMarkdownDeBase()
    {
        var service = new ArticleService(InMemoryDbContextFactory.Create());

        var html = service.RendreHtml("# Titre\n\nUn **texte en gras**.");

        Assert.Contains("<h1", html);
        Assert.Contains("<strong>texte en gras</strong>", html);
    }

    [Fact]
    public void RendreHtml_NeepasLaisserPasserDeHtmlBrut()
    {
        // DisableHtml() sur le pipeline Markdig : toute balise HTML presente dans le
        // Markdown source est echappee, jamais inseree telle quelle - cf. demande "HTML
        // nettoye" (le contenu vient d'un back-office authentifie, mais on ne fait jamais
        // confiance a une entree qui finit sur une page publique).
        var service = new ArticleService(InMemoryDbContextFactory.Create());

        var html = service.RendreHtml("Texte <script>alert('xss')</script> suite.");

        Assert.DoesNotContain("<script>", html);
    }
}

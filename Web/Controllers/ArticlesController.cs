using System.Text;
using System.Xml.Linq;
using Application.Common.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers;

// Blog public (/articles) - toujours accessible sans compte, meme principe que les tests
// psychotechniques (cf. TestDiscModel) meme si ici il n'y a pas de notion de connexion du
// tout : un article publie est visible par n'importe qui. Edition reservee a
// /admin/articles (cf. ArticlesAdminController).
[Route("articles")]
public class ArticlesController(IArticleService articleService) : Controller
{
    private const int TaillePage = 9;
    private const string DomainePublic = "https://challenges-factory.com";

    [HttpGet("")]
    public async Task<IActionResult> Index(int page = 1)
    {
        var (articles, total) = await articleService.GetPublicsAsync(Math.Max(page, 1), TaillePage);

        ViewData["Title"] = "Articles";
        ViewData["MetaDescription"] = "Les articles Challenges Factory : décarbonation, compétences et Challenge Based Learning expliqués simplement.";

        var viewModel = new ArticlesIndexViewModel
        {
            Articles = [.. articles.Select(VersApercu)],
            Page = Math.Max(page, 1),
            NombrePages = (int)Math.Ceiling(total / (double)TaillePage),
        };

        return View(viewModel);
    }

    [HttpGet("rss")]
    public async Task<IActionResult> Rss()
    {
        var articles = await articleService.GetDerniersPublicsAsync(30);

        var canal = new XElement("channel",
            new XElement("title", "Articles Challenges Factory"),
            new XElement("link", $"{DomainePublic}/articles"),
            new XElement("description", "Les derniers articles Challenges Factory."),
            new XElement("language", "fr-fr"),
            articles.Select(article => new XElement("item",
                new XElement("title", article.Titre),
                new XElement("link", $"{DomainePublic}/articles/{article.Slug}"),
                new XElement("guid", $"{DomainePublic}/articles/{article.Slug}"),
                new XElement("pubDate", DateTime.SpecifyKind(article.DatePublication, DateTimeKind.Utc).ToString("r")),
                new XElement("description", article.Resume))));

        var rss = new XElement("rss", new XAttribute("version", "2.0"), canal);
        var document = new XDocument(new XDeclaration("1.0", "utf-8", null), rss);

        return Content(document.Declaration + document.ToString(), "application/rss+xml", Encoding.UTF8);
    }

    [HttpGet("{slug}")]
    public async Task<IActionResult> Details(string slug)
    {
        var article = await articleService.GetPublicParSlugAsync(slug);
        if (article is null)
        {
            return NotFound();
        }

        ViewData["Title"] = article.Titre;
        ViewData["MetaDescription"] = article.MetaDescription;
        ViewData["OgType"] = "article";
        if (!string.IsNullOrWhiteSpace(article.ImageCouverture))
        {
            ViewData["OgImage"] = $"{DomainePublic}/uploads/articles/{article.ImageCouverture}";
        }

        var viewModel = new ArticleDetailViewModel
        {
            Titre = article.Titre,
            Auteur = article.Auteur,
            // Stocke en UTC (Kind=Unspecified apres le round-trip SQL Server, mais la valeur
            // numerique est bien l'instant UTC - cf. ArticlesAdminController.VersInput) :
            // ToLocalTime() traite un DateTime Unspecified comme deja UTC, conversion correcte
            // pour l'affichage.
            DatePublication = DateTime.SpecifyKind(article.DatePublication, DateTimeKind.Utc).ToLocalTime(),
            ImageCouverture = article.ImageCouverture,
            ContenuHtml = articleService.RendreHtml(article.ContenuMarkdown),
            Sources = [.. article.Sources.Select(s => new ArticleSourceViewModel { Libelle = s.Libelle, Url = s.Url })],
        };

        return View(viewModel);
    }

    private static ArticleApercuViewModel VersApercu(Article article) => new()
    {
        Slug = article.Slug,
        Titre = article.Titre,
        Resume = article.Resume,
        Auteur = article.Auteur,
        DatePublication = DateTime.SpecifyKind(article.DatePublication, DateTimeKind.Utc).ToLocalTime(),
        ImageCouverture = article.ImageCouverture,
    };
}

public class ArticlesIndexViewModel
{
    public List<ArticleApercuViewModel> Articles { get; set; } = [];
    public int Page { get; set; }
    public int NombrePages { get; set; }
}

public class ArticleApercuViewModel
{
    public string Slug { get; set; } = string.Empty;
    public string Titre { get; set; } = string.Empty;
    public string Resume { get; set; } = string.Empty;
    public string Auteur { get; set; } = string.Empty;
    public DateTime DatePublication { get; set; }
    public string? ImageCouverture { get; set; }
}

public class ArticleDetailViewModel
{
    public string Titre { get; set; } = string.Empty;
    public string Auteur { get; set; } = string.Empty;
    public DateTime DatePublication { get; set; }
    public string? ImageCouverture { get; set; }
    public string ContenuHtml { get; set; } = string.Empty;
    public List<ArticleSourceViewModel> Sources { get; set; } = [];
}

public class ArticleSourceViewModel
{
    public string? Libelle { get; set; }
    public string Url { get; set; } = string.Empty;
}

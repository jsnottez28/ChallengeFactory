using System.Text;
using System.Xml.Linq;
using Application.Common.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers;

// Sitemap XML servi dynamiquement (remplace l'ancien wwwroot/sitemap.xml statique, qui
// omettait toute page /articles/{slug} - un blog aliemente par la base de donnees ne peut
// pas vivre dans un fichier entretenu a la main : un nouvel article publie doit apparaitre
// automatiquement, sans intervention manuelle). Les pages vitrine (hors Articles) restent
// une liste fixe ci-dessous : leur volume est stable et connu, aucune base de donnees ne
// pilote ce catalogue-la.
public class SitemapController(IArticleService articleService) : Controller
{
    private const string DomainePublic = "https://challenges-factory.com";
    private static readonly XNamespace Ns = "http://www.sitemaps.org/schemas/sitemap/0.9";

    // Pages vitrine statiques (hors Articles) : (chemin, derniere modification, frequence,
    // priorite). A mettre a jour manuellement si une page publique est ajoutee/retiree.
    // Volontairement absent : /formations/{challengeId} (fiches Challenge individuelles,
    // contenu pilote par la base et non stable) - le catalogue /formations suffit comme
    // point d'entree indexable, Google decouvrira les fiches publiees par les liens internes.
    private static readonly (string Chemin, DateTime DerniereModif, string Frequence, string Priorite)[] PagesStatiques =
    [
        ("/", new DateTime(2026, 9, 11), "weekly", "1.0"),
        ("/bilan-carbone", new DateTime(2026, 9, 11), "monthly", "0.9"),
        ("/bilan-de-competences", new DateTime(2026, 9, 11), "monthly", "0.8"),
        ("/bilan-carbone-eure-et-loir", new DateTime(2026, 9, 11), "monthly", "0.7"),
        ("/la-methode", new DateTime(2026, 9, 11), "monthly", "0.8"),
        ("/formations", new DateTime(2026, 9, 11), "weekly", "0.8"),
        ("/a-propos", new DateTime(2026, 9, 11), "monthly", "0.6"),
        ("/le-lab", new DateTime(2026, 9, 11), "weekly", "0.6"),
        ("/articles", new DateTime(2026, 10, 31), "weekly", "0.6"),
        ("/le-lab/beges-obligatoire-2026", new DateTime(2026, 9, 11), "yearly", "0.5"),
        ("/le-lab/diag-decarbon-action", new DateTime(2026, 9, 11), "yearly", "0.5"),
        ("/le-lab/plans-de-transition-dans-un-tiroir", new DateTime(2026, 9, 11), "yearly", "0.5"),
        ("/contact", new DateTime(2026, 9, 11), "yearly", "0.5"),
        ("/accessibilite", new DateTime(2026, 9, 16), "yearly", "0.3"),
        ("/reclamation", new DateTime(2026, 9, 16), "yearly", "0.3"),
        ("/cgu", new DateTime(2026, 9, 11), "yearly", "0.2"),
        ("/confidentialite", new DateTime(2026, 9, 11), "yearly", "0.2"),
    ];

    [HttpGet("sitemap.xml")]
    public async Task<IActionResult> Index()
    {
        var urls = PagesStatiques.Select(p => ConstruireUrl($"{DomainePublic}{p.Chemin}", p.DerniereModif, p.Frequence, p.Priorite)).ToList();

        // Jusqu'a 500 articles publics les plus recents : largement suffisant pour un
        // blog d'entreprise, evite d'ajouter une methode de recuperation "tous" dediee.
        var articles = await articleService.GetDerniersPublicsAsync(500);
        urls.AddRange(articles.Select(a => ConstruireUrl(
            $"{DomainePublic}/articles/{a.Slug}",
            DateTime.SpecifyKind(a.DatePublication, DateTimeKind.Utc),
            "monthly",
            "0.5")));

        var document = new XDocument(
            new XDeclaration("1.0", "utf-8", null),
            new XElement(Ns + "urlset", urls));

        return Content(document.Declaration + document.ToString(), "application/xml", Encoding.UTF8);
    }

    private static XElement ConstruireUrl(string loc, DateTime derniereModif, string frequence, string priorite) => new(Ns + "url",
        new XElement(Ns + "loc", loc),
        new XElement(Ns + "lastmod", derniereModif.ToString("yyyy-MM-dd")),
        new XElement(Ns + "changefreq", frequence),
        new XElement(Ns + "priority", priorite));
}

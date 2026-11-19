using Application.Common.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers;

// Espace public de publication des etudes de cas Bilan Carbone(R)/ACV realisees
// (/rapports), toujours accessible sans compte - meme principe que ArticlesController.
// Edition reservee a /admin/rapports (cf. RapportsAdminController). Exception : Details()
// autorise aussi un apercu aux titulaires du Droit RAPPORT.CONSULTER, pour relire un
// Brouillon/rapport Programme avant publication (meme URL /rapports/{slug}, pas de lien
// d'apercu separe a gerer).
[Route("rapports")]
public class RapportsController(IRapportService rapportService, IAuthorizationService authorizationService) : Controller
{
    private const int TaillePage = 9;
    private const string DomainePublic = "https://challenges-factory.com";

    [HttpGet("")]
    public async Task<IActionResult> Index(int page = 1)
    {
        var (rapports, total) = await rapportService.GetPublicsAsync(Math.Max(page, 1), TaillePage);

        ViewData["Title"] = "Études de cas";
        ViewData["MetaDescription"] = "Les études de cas Challenges Factory : Bilan Carbone® et ACV réalisés, publiés au fil de l'eau.";

        var viewModel = new RapportsIndexViewModel
        {
            Rapports = [.. rapports.Select(VersApercu)],
            Page = Math.Max(page, 1),
            NombrePages = (int)Math.Ceiling(total / (double)TaillePage),
        };

        return View(viewModel);
    }

    [HttpGet("{slug}")]
    public async Task<IActionResult> Details(string slug)
    {
        var rapport = await rapportService.GetParSlugAsync(slug);
        if (rapport is null)
        {
            return NotFound();
        }

        var estPublic = rapport.Statut == StatutRapport.Publie && rapport.DatePublication <= DateTime.UtcNow;
        if (!estPublic)
        {
            var autorisation = await authorizationService.AuthorizeAsync(User, "Droit:RAPPORT.CONSULTER");
            if (!autorisation.Succeeded)
            {
                return NotFound();
            }
        }

        ViewData["Title"] = rapport.Titre;
        ViewData["MetaDescription"] = rapport.MetaDescription;
        ViewData["OgType"] = "article";
        if (!estPublic)
        {
            // Jamais indexer un apercu de Brouillon/rapport Programme, meme accessible aux
            // seuls admins connectes.
            ViewData["Robots"] = "noindex, nofollow";
        }
        if (!string.IsNullOrWhiteSpace(rapport.ImageCouverture))
        {
            ViewData["OgImage"] = $"{DomainePublic}/uploads/rapports/{rapport.ImageCouverture}";
        }

        var viewModel = new RapportDetailViewModel
        {
            Titre = rapport.Titre,
            Auteur = rapport.Auteur,
            Type = rapport.Type,
            EstApercu = !estPublic,
            StatutAffiche = rapportService.DeterminerStatutAffiche(rapport),
            // Stocke en UTC (Kind=Unspecified apres le round-trip SQL Server, mais la
            // valeur numerique est bien l'instant UTC - cf. RapportsAdminController.VersInput) :
            // ToLocalTime() traite un DateTime Unspecified comme deja UTC, conversion correcte
            // pour l'affichage.
            DatePublication = DateTime.SpecifyKind(rapport.DatePublication, DateTimeKind.Utc).ToLocalTime(),
            ImageCouverture = rapport.ImageCouverture,
            FichierPdf = rapport.FichierPdf,
            ContenuHtml = rapportService.RendreHtml(rapport.ContenuMarkdown),
        };

        return View(viewModel);
    }

    private static RapportApercuViewModel VersApercu(Rapport rapport) => new()
    {
        Slug = rapport.Slug,
        Titre = rapport.Titre,
        Resume = rapport.Resume,
        Auteur = rapport.Auteur,
        Type = rapport.Type,
        DatePublication = DateTime.SpecifyKind(rapport.DatePublication, DateTimeKind.Utc).ToLocalTime(),
        ImageCouverture = rapport.ImageCouverture,
    };
}

public class RapportsIndexViewModel
{
    public List<RapportApercuViewModel> Rapports { get; set; } = [];
    public int Page { get; set; }
    public int NombrePages { get; set; }
}

public class RapportApercuViewModel
{
    public string Slug { get; set; } = string.Empty;
    public string Titre { get; set; } = string.Empty;
    public string Resume { get; set; } = string.Empty;
    public string Auteur { get; set; } = string.Empty;
    public TypeRapport Type { get; set; }
    public DateTime DatePublication { get; set; }
    public string? ImageCouverture { get; set; }
}

public class RapportDetailViewModel
{
    public string Titre { get; set; } = string.Empty;
    public string Auteur { get; set; } = string.Empty;
    public TypeRapport Type { get; set; }
    public DateTime DatePublication { get; set; }
    public string? ImageCouverture { get; set; }
    public string? FichierPdf { get; set; }
    public string ContenuHtml { get; set; } = string.Empty;
    public bool EstApercu { get; set; }
    public string StatutAffiche { get; set; } = string.Empty;
}

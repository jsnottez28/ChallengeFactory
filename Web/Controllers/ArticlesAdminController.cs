using System.Globalization;
using Application.Common.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Services;

namespace Web.Controllers;

// Back-office du blog (/admin/articles), reserve aux titulaires des Droits ARTICLE.* -
// authentification par l'identite du site (cf. CLAUDE.md "Pas de clé d'API"), jamais de
// jeton separe. Les champs du formulaire sont lus directement depuis Request.Form (prefixe
// "titre"/"slug"/"resume"/"meta"/"contenu"/"auteur"/"datePublication"/"statut"/"sources")
// plutot que via [BindProperty] : memes id/name que ceux explicitement demandes (pour
// qu'un assistant puisse remplir le formulaire de facon fiable), meme principe de lecture
// manuelle que TestRiasec/TestBigFive/TestSchein (le binder de modele complexe d'ASP.NET
// Core s'est avere instable sur des flux de formulaire similaires ailleurs sur la
// plateforme).
[Route("admin/articles")]
public class ArticlesAdminController(IArticleService articleService, IWebHostEnvironment webHostEnvironment) : Controller
{
    private static readonly string[] ExtensionsImageAutorisees = [".jpg", ".jpeg", ".png", ".webp"];
    private const long TailleMaxImageOctets = 5 * 1024 * 1024; // 5 Mo
    private const int LargeurAttendue = 1200;
    private const int HauteurAttendue = 630;

    [HttpGet("")]
    [Authorize(Policy = "Droit:ARTICLE.CONSULTER")]
    public async Task<IActionResult> Index(int page = 1)
    {
        var resultat = await articleService.RechercherAdminAsync(Math.Max(page, 1), 20);

        ViewData["Page"] = Math.Max(page, 1);
        ViewData["NombrePages"] = (int)Math.Ceiling(resultat.NombreTotal / 20.0);
        ViewData["Statuts"] = resultat.Articles.ToDictionary(a => a.Id, a => articleService.DeterminerStatutAffiche(a));

        return View(resultat.Articles);
    }

    [HttpGet("creer")]
    [Authorize(Policy = "Droit:ARTICLE.CREER")]
    public IActionResult Create() => View("Save", new ArticleFormViewModel
    {
        DatePublication = DateTime.Now,
    });

    [HttpPost("creer")]
    [ActionName("Create")]
    [Authorize(Policy = "Droit:ARTICLE.CREER")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreatePost()
    {
        var model = LireFormulaire();

        var (nomFichierImage, erreurUpload) = await EnregistrerImageAsync(Request.Form.Files.GetFile("image"));
        if (erreurUpload is not null)
        {
            ModelState.AddModelError(string.Empty, erreurUpload);
            return View("Save", model);
        }

        var (success, errorMessage, article) = await articleService.CreateAsync(VersInput(model, nomFichierImage));
        if (!success)
        {
            ModelState.AddModelError(string.Empty, errorMessage ?? "Impossible de créer cet article.");
            return View("Save", model);
        }

        TempData["StatusMessage"] = "Article créé.";
        return RedirectToAction(nameof(Edit), new { id = article!.Id });
    }

    [HttpGet("{id:int}/modifier")]
    [Authorize(Policy = "Droit:ARTICLE.MODIFIER")]
    public async Task<IActionResult> Edit(int id)
    {
        var article = await articleService.GetByIdAsync(id);
        if (article is null)
        {
            return NotFound();
        }

        return View("Save", VersFormModel(article));
    }

    [HttpPost("{id:int}/modifier")]
    [ActionName("Edit")]
    [Authorize(Policy = "Droit:ARTICLE.MODIFIER")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditPost(int id)
    {
        var model = LireFormulaire();
        model.Id = id;

        var (nomFichierImage, erreurUpload) = await EnregistrerImageAsync(Request.Form.Files.GetFile("image"));
        if (erreurUpload is not null)
        {
            ModelState.AddModelError(string.Empty, erreurUpload);
            return View("Save", model);
        }

        var (success, errorMessage) = await articleService.UpdateAsync(id, VersInput(model, nomFichierImage));
        if (!success)
        {
            ModelState.AddModelError(string.Empty, errorMessage ?? "Impossible de modifier cet article.");
            return View("Save", model);
        }

        TempData["StatusMessage"] = "Article modifié.";
        return RedirectToAction(nameof(Edit), new { id });
    }

    [HttpPost("{id:int}/supprimer")]
    [Authorize(Policy = "Droit:ARTICLE.SUPPRIMER")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var (success, errorMessage) = await articleService.DeleteAsync(id);
        TempData["StatusMessage"] = success ? "Article supprimé." : errorMessage;
        return RedirectToAction(nameof(Index));
    }

    private ArticleFormViewModel LireFormulaire()
    {
        var titre = Request.Form["titre"].ToString();
        var slug = Request.Form["slug"].ToString().Trim();
        if (string.IsNullOrWhiteSpace(slug))
        {
            slug = articleService.GenererSlug(titre);
        }

        DateTime.TryParse(Request.Form["datePublication"], CultureInfo.InvariantCulture, DateTimeStyles.None, out var datePublication);
        Enum.TryParse<StatutArticle>(Request.Form["statut"], out var statut);

        return new ArticleFormViewModel
        {
            Titre = titre,
            Slug = slug,
            Resume = Request.Form["resume"].ToString(),
            MetaDescription = Request.Form["meta"].ToString(),
            ContenuMarkdown = Request.Form["contenu"].ToString(),
            Auteur = Request.Form["auteur"].ToString(),
            DatePublication = datePublication == default ? DateTime.Now : datePublication,
            Statut = statut,
            SourcesTexte = Request.Form["sources"].ToString(),
            ImageCouvertureActuelle = Request.Form["imageActuelle"].ToString() is { Length: > 0 } nom ? nom : null,
        };
    }

    private static ArticleInput VersInput(ArticleFormViewModel model, string? nomFichierImage) => new()
    {
        Titre = model.Titre,
        Slug = model.Slug,
        Resume = model.Resume,
        MetaDescription = model.MetaDescription,
        ContenuMarkdown = model.ContenuMarkdown,
        ImageCouverture = nomFichierImage ?? model.ImageCouvertureActuelle,
        Auteur = model.Auteur,
        DatePublication = model.DatePublication,
        Statut = model.Statut,
        Sources = [.. model.SourcesTexte
            .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(ligne =>
            {
                var parties = ligne.Split('|', 2, StringSplitOptions.TrimEntries);
                return parties.Length == 2
                    ? new ArticleSourceInput { Libelle = parties[0], Url = parties[1] }
                    : new ArticleSourceInput { Libelle = null, Url = parties[0] };
            })],
    };

    private ArticleFormViewModel VersFormModel(Article article) => new()
    {
        Id = article.Id,
        Titre = article.Titre,
        Slug = article.Slug,
        Resume = article.Resume,
        MetaDescription = article.MetaDescription,
        ContenuMarkdown = article.ContenuMarkdown,
        ImageCouvertureActuelle = article.ImageCouverture,
        Auteur = article.Auteur,
        DatePublication = article.DatePublication,
        Statut = article.Statut,
        SourcesTexte = string.Join('\n', article.Sources.OrderBy(s => s.Ordre)
            .Select(s => string.IsNullOrWhiteSpace(s.Libelle) ? s.Url : $"{s.Libelle} | {s.Url}")),
    };

    // Meme principe que CartesController.EnregistrerImageAsync (magic bytes + taille, pas
    // de dependance de conversion) - ici le redimensionnement en 1200x630 est fait cote
    // navigateur avant l'envoi (cf. wwwroot/js/article-form.js) : le serveur se contente de
    // verifier que le fichier recu respecte bien ce format exact, pour ne jamais faire
    // confiance a ce que le navigateur dit avoir deja fait.
    private async Task<(string? NomFichier, string? Erreur)> EnregistrerImageAsync(IFormFile? fichier)
    {
        if (fichier is null || fichier.Length == 0)
        {
            return (null, null);
        }

        var extension = Path.GetExtension(fichier.FileName);
        if (!ExtensionsImageAutorisees.Contains(extension, StringComparer.OrdinalIgnoreCase))
        {
            return (null, "Formats acceptés pour l'image de couverture : JPG, PNG, WEBP.");
        }

        if (fichier.Length > TailleMaxImageOctets)
        {
            return (null, $"L'image ne doit pas dépasser {TailleMaxImageOctets / (1024 * 1024)} Mo.");
        }

        await using var fluxMemoire = new MemoryStream();
        await fichier.CopyToAsync(fluxMemoire);
        var octets = fluxMemoire.ToArray();

        var dimensions = ImageDimensionReader.LireDimensions(octets);
        if (dimensions is null)
        {
            return (null, "Impossible de lire les dimensions de cette image : le fichier est peut-être corrompu ou dans un format non standard.");
        }

        if (dimensions.Value.Largeur != LargeurAttendue || dimensions.Value.Hauteur != HauteurAttendue)
        {
            return (null, $"Cette image fait {dimensions.Value.Largeur}x{dimensions.Value.Hauteur}px : elle doit faire exactement {LargeurAttendue}x{HauteurAttendue}px (redimensionnement automatique au moment de la sélection du fichier - réessayez).");
        }

        var dossierUploads = Path.Combine(webHostEnvironment.WebRootPath, "uploads", "articles");
        Directory.CreateDirectory(dossierUploads);

        var nomFichier = $"{Guid.NewGuid():N}{extension.ToLowerInvariant()}";
        await System.IO.File.WriteAllBytesAsync(Path.Combine(dossierUploads, nomFichier), octets);

        return (nomFichier, null);
    }
}

public class ArticleFormViewModel
{
    public int? Id { get; set; }
    public string Titre { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Resume { get; set; } = string.Empty;
    public string MetaDescription { get; set; } = string.Empty;
    public string ContenuMarkdown { get; set; } = string.Empty;
    public string? ImageCouvertureActuelle { get; set; }
    public string Auteur { get; set; } = string.Empty;
    public DateTime DatePublication { get; set; } = DateTime.Now;
    public StatutArticle Statut { get; set; } = StatutArticle.Brouillon;

    // Une source par ligne, format "Libellé | URL" ou juste "URL".
    public string SourcesTexte { get; set; } = string.Empty;
}

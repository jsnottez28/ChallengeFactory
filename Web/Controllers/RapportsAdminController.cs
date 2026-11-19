using System.Globalization;
using Application.Common.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Services;

namespace Web.Controllers;

// Back-office de l'espace Rapports (/admin/rapports), reserve aux titulaires des Droits
// RAPPORT.* - meme principe que ArticlesAdminController : les champs du formulaire sont lus
// directement depuis Request.Form plutot que via [BindProperty] (meme convention que les
// autres back-offices de contenu de la plateforme).
[Route("admin/rapports")]
public class RapportsAdminController(IRapportService rapportService, IWebHostEnvironment webHostEnvironment) : Controller
{
    private static readonly string[] ExtensionsImageAutorisees = [".jpg", ".jpeg", ".png", ".webp"];
    private const long TailleMaxImageOctets = 5 * 1024 * 1024; // 5 Mo
    private const int LargeurAttendue = 1200;
    private const int HauteurAttendue = 630;
    private const long TailleMaxPdfOctets = 10 * 1024 * 1024; // 10 Mo

    [HttpGet("")]
    [Authorize(Policy = "Droit:RAPPORT.CONSULTER")]
    public async Task<IActionResult> Index(int page = 1)
    {
        var resultat = await rapportService.RechercherAdminAsync(Math.Max(page, 1), 20);

        ViewData["Page"] = Math.Max(page, 1);
        ViewData["NombrePages"] = (int)Math.Ceiling(resultat.NombreTotal / 20.0);
        ViewData["Statuts"] = resultat.Rapports.ToDictionary(r => r.Id, r => rapportService.DeterminerStatutAffiche(r));

        return View(resultat.Rapports);
    }

    [HttpGet("creer")]
    [Authorize(Policy = "Droit:RAPPORT.CREER")]
    public IActionResult Create() => View("Save", new RapportFormViewModel
    {
        DatePublication = DateTime.Now,
    });

    [HttpPost("creer")]
    [ActionName("Create")]
    [Authorize(Policy = "Droit:RAPPORT.CREER")]
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

        var (nomFichierPdf, erreurPdf) = await EnregistrerPdfAsync(Request.Form.Files.GetFile("pdf"));
        if (erreurPdf is not null)
        {
            ModelState.AddModelError(string.Empty, erreurPdf);
            return View("Save", model);
        }

        var (success, errorMessage, rapport) = await rapportService.CreateAsync(VersInput(model, nomFichierImage, nomFichierPdf));
        if (!success)
        {
            ModelState.AddModelError(string.Empty, errorMessage ?? "Impossible de créer ce rapport.");
            return View("Save", model);
        }

        TempData["StatusMessage"] = "Rapport créé.";
        return RedirectToAction(nameof(Edit), new { id = rapport!.Id });
    }

    [HttpGet("{id:int}/modifier")]
    [Authorize(Policy = "Droit:RAPPORT.MODIFIER")]
    public async Task<IActionResult> Edit(int id)
    {
        var rapport = await rapportService.GetByIdAsync(id);
        if (rapport is null)
        {
            return NotFound();
        }

        return View("Save", VersFormModel(rapport));
    }

    [HttpPost("{id:int}/modifier")]
    [ActionName("Edit")]
    [Authorize(Policy = "Droit:RAPPORT.MODIFIER")]
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

        var (nomFichierPdf, erreurPdf) = await EnregistrerPdfAsync(Request.Form.Files.GetFile("pdf"));
        if (erreurPdf is not null)
        {
            ModelState.AddModelError(string.Empty, erreurPdf);
            return View("Save", model);
        }

        var (success, errorMessage) = await rapportService.UpdateAsync(id, VersInput(model, nomFichierImage, nomFichierPdf));
        if (!success)
        {
            ModelState.AddModelError(string.Empty, errorMessage ?? "Impossible de modifier ce rapport.");
            return View("Save", model);
        }

        TempData["StatusMessage"] = "Rapport modifié.";
        return RedirectToAction(nameof(Edit), new { id });
    }

    [HttpPost("{id:int}/supprimer")]
    [Authorize(Policy = "Droit:RAPPORT.SUPPRIMER")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var (success, errorMessage) = await rapportService.DeleteAsync(id);
        TempData["StatusMessage"] = success ? "Rapport supprimé." : errorMessage;
        return RedirectToAction(nameof(Index));
    }

    private RapportFormViewModel LireFormulaire()
    {
        var titre = Request.Form["titre"].ToString();
        var slug = Request.Form["slug"].ToString().Trim();
        if (string.IsNullOrWhiteSpace(slug))
        {
            slug = rapportService.GenererSlug(titre);
        }

        // Le champ datetime-local du formulaire ne porte aucune information de fuseau : la
        // valeur saisie est l'heure murale de l'admin (France). On la marque explicitement
        // Local ici ; la conversion effective en UTC (seule valeur fiable a comparer a
        // DateTime.UtcNow pour decider si le rapport est visible) n'a lieu qu'au moment de
        // VersInput, juste avant persistance - cf. commentaire la-bas.
        DateTime.TryParse(Request.Form["datePublication"], CultureInfo.InvariantCulture, DateTimeStyles.None, out var datePublication);
        Enum.TryParse<StatutRapport>(Request.Form["statut"], out var statut);
        Enum.TryParse<TypeRapport>(Request.Form["type"], out var type);

        return new RapportFormViewModel
        {
            Titre = titre,
            Slug = slug,
            Resume = Request.Form["resume"].ToString(),
            MetaDescription = Request.Form["meta"].ToString(),
            ContenuMarkdown = Request.Form["contenu"].ToString(),
            Type = type,
            Auteur = Request.Form["auteur"].ToString(),
            DatePublication = datePublication == default ? DateTime.Now : datePublication,
            Statut = statut,
            ImageCouvertureActuelle = Request.Form["imageActuelle"].ToString() is { Length: > 0 } nomImage ? nomImage : null,
            FichierPdfActuel = Request.Form["pdfActuel"].ToString() is { Length: > 0 } nomPdf ? nomPdf : null,
        };
    }

    private static RapportInput VersInput(RapportFormViewModel model, string? nomFichierImage, string? nomFichierPdf) => new()
    {
        Titre = model.Titre,
        Slug = model.Slug,
        Resume = model.Resume,
        MetaDescription = model.MetaDescription,
        ContenuMarkdown = model.ContenuMarkdown,
        Type = model.Type,
        ImageCouverture = nomFichierImage ?? model.ImageCouvertureActuelle,
        FichierPdf = nomFichierPdf ?? model.FichierPdfActuel,
        Auteur = model.Auteur,
        // model.DatePublication est toujours en heure locale (France) a ce stade - cf.
        // LireFormulaire/VersFormModel. Conversion explicite en UTC ici, juste avant
        // persistance : SQL Server (datetime2) ne conserve aucune information de fuseau, donc
        // c'est la SEULE valeur qui doit etre comparee a DateTime.UtcNow cote service
        // (RapportService.RequetePublique) pour decider si le rapport est visible.
        DatePublication = DateTime.SpecifyKind(model.DatePublication, DateTimeKind.Local).ToUniversalTime(),
        Statut = model.Statut,
    };

    private RapportFormViewModel VersFormModel(Rapport rapport) => new()
    {
        Id = rapport.Id,
        Titre = rapport.Titre,
        Slug = rapport.Slug,
        Resume = rapport.Resume,
        MetaDescription = rapport.MetaDescription,
        ContenuMarkdown = rapport.ContenuMarkdown,
        Type = rapport.Type,
        ImageCouvertureActuelle = rapport.ImageCouverture,
        FichierPdfActuel = rapport.FichierPdf,
        Auteur = rapport.Auteur,
        // rapport.DatePublication revient de la base en UTC (Kind=Unspecified apres le
        // round-trip SQL Server, mais la valeur numerique est bien l'instant UTC - cf.
        // VersInput) : ToLocalTime() traite un DateTime Unspecified comme deja UTC, donc
        // cette conversion est correcte et affiche l'heure murale francaise dans le champ
        // datetime-local du formulaire.
        DatePublication = DateTime.SpecifyKind(rapport.DatePublication, DateTimeKind.Utc).ToLocalTime(),
        Statut = rapport.Statut,
    };

    // Meme principe que ArticlesAdminController.EnregistrerImageAsync (magic bytes + taille,
    // pas de dependance de conversion) - le redimensionnement en 1200x630 est fait cote
    // navigateur avant l'envoi (cf. wwwroot/js/rapport-form.js).
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

        var dossierUploads = Path.Combine(webHostEnvironment.WebRootPath, "uploads", "rapports");
        Directory.CreateDirectory(dossierUploads);

        var nomFichier = $"{Guid.NewGuid():N}{extension.ToLowerInvariant()}";
        await System.IO.File.WriteAllBytesAsync(Path.Combine(dossierUploads, nomFichier), octets);

        return (nomFichier, null);
    }

    // Verifie la signature PDF ("%PDF-", les 5 premiers octets de tout fichier PDF valide)
    // plutot que de se fier a l'extension seule - meme principe que
    // CartesController.EnregistrerPdfAsync. Le fichier est servi tel quel en telechargement
    // public (wwwroot/uploads/rapports), jamais execute ni interprete cote serveur.
    private async Task<(string? NomFichier, string? Erreur)> EnregistrerPdfAsync(IFormFile? fichier)
    {
        if (fichier is null || fichier.Length == 0)
        {
            return (null, null);
        }

        if (!Path.GetExtension(fichier.FileName).Equals(".pdf", StringComparison.OrdinalIgnoreCase))
        {
            return (null, "Le document complet doit être au format PDF.");
        }

        if (fichier.Length > TailleMaxPdfOctets)
        {
            return (null, $"Le fichier PDF ne doit pas dépasser {TailleMaxPdfOctets / (1024 * 1024)} Mo.");
        }

        await using var fluxMemoire = new MemoryStream();
        await fichier.CopyToAsync(fluxMemoire);
        var octets = fluxMemoire.ToArray();

        var signaturePdf = "%PDF-"u8.ToArray();
        if (octets.Length < signaturePdf.Length || !octets.AsSpan(0, signaturePdf.Length).SequenceEqual(signaturePdf))
        {
            return (null, "Ce fichier n'est pas un PDF valide.");
        }

        var dossierUploads = Path.Combine(webHostEnvironment.WebRootPath, "uploads", "rapports");
        Directory.CreateDirectory(dossierUploads);

        var nomFichier = $"{Guid.NewGuid():N}.pdf";
        await System.IO.File.WriteAllBytesAsync(Path.Combine(dossierUploads, nomFichier), octets);

        return (nomFichier, null);
    }
}

public class RapportFormViewModel
{
    public int? Id { get; set; }
    public string Titre { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Resume { get; set; } = string.Empty;
    public string MetaDescription { get; set; } = string.Empty;
    public string ContenuMarkdown { get; set; } = string.Empty;
    public TypeRapport Type { get; set; } = TypeRapport.BilanCarbone;
    public string? ImageCouvertureActuelle { get; set; }
    public string? FichierPdfActuel { get; set; }
    public string Auteur { get; set; } = string.Empty;
    public DateTime DatePublication { get; set; } = DateTime.Now;
    public StatutRapport Statut { get; set; } = StatutRapport.Brouillon;
}

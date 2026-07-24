using System.ComponentModel.DataAnnotations;
using Application.Common.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Web.Data;

namespace Web.Controllers;

[Route("Administration/[controller]")]
public class CartesController(
    ICarteCompetenceService carteCompetenceService,
    UserManager<ApplicationUser> userManager,
    IWebHostEnvironment webHostEnvironment) : Controller
{
    private static readonly string[] ExtensionsImageAutorisees = [".jpg", ".jpeg", ".png", ".webp", ".gif"];
    private const long TailleMaxImageOctets = 5 * 1024 * 1024;

    [HttpGet("")]
    [Authorize(Policy = "Droit:CARTE.CONSULTER")]
    public async Task<IActionResult> Index(string? recherche, NiveauCarte? niveau, int? badgeId, int page = 1)
    {
        var resultat = await carteCompetenceService.RechercherAsync(new CarteCompetenceFiltre
        {
            Recherche = recherche,
            Niveau = niveau,
            BadgeId = badgeId,
            Page = page,
            TaillePage = 20,
        });

        ViewData["Recherche"] = recherche;
        ViewData["Niveau"] = niveau;
        ViewData["BadgeId"] = badgeId;
        ViewData["Page"] = page;
        ViewData["NombrePages"] = (int)Math.Ceiling(resultat.NombreTotal / 20.0);
        ViewData["Badges"] = await ListeBadgesAsync(badgeId);

        return View(resultat.Cartes);
    }

    [HttpGet("Import")]
    [Authorize(Policy = "Droit:CARTE.CREER")]
    public IActionResult Import()
    {
        return View();
    }

    [HttpPost("Import")]
    [Authorize(Policy = "Droit:CARTE.CREER")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Import(IFormFile fichier)
    {
        if (fichier is null || fichier.Length == 0)
        {
            ModelState.AddModelError(string.Empty, "Sélectionnez un fichier .xlsx à importer.");
            return View();
        }

        if (!Path.GetExtension(fichier.FileName).Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
        {
            ModelState.AddModelError(string.Empty, "Le fichier doit être au format .xlsx.");
            return View();
        }

        await using var flux = fichier.OpenReadStream();
        var rapport = await carteCompetenceService.ImporterAsync(flux);

        return View("ImportResultat", rapport);
    }

    [HttpGet("Create")]
    [Authorize(Policy = "Droit:CARTE.CREER")]
    public async Task<IActionResult> Create()
    {
        return View("Save", new CarteCompetenceFormModel { Badges = await ListeBadgesAsync(null) });
    }

    [HttpPost("Create")]
    [Authorize(Policy = "Droit:CARTE.CREER")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CarteCompetenceFormModel model)
    {
        if (!ModelState.IsValid)
        {
            model.Badges = await ListeBadgesAsync(model.BadgeId);
            return View("Save", model);
        }

        var (nomFichierImage, erreurUpload) = await EnregistrerImageAsync(model.ImageCarteAFichier);
        if (erreurUpload is not null)
        {
            ModelState.AddModelError(nameof(model.ImageCarteAFichier), erreurUpload);
            model.Badges = await ListeBadgesAsync(model.BadgeId);
            return View("Save", model);
        }

        var (success, errorMessage, carte) = await carteCompetenceService.CreateAsync(VersInput(model, nomFichierImage));

        if (!success)
        {
            ModelState.AddModelError(nameof(model.Code), errorMessage ?? "Impossible de créer cette carte.");
            model.Badges = await ListeBadgesAsync(model.BadgeId);
            return View("Save", model);
        }

        TempData["StatusMessage"] = "Carte de compétences créée.";
        return RedirectToAction(nameof(Details), new { id = carte!.Id });
    }

    [HttpGet("Edit/{id:int}")]
    [Authorize(Policy = "Droit:CARTE.MODIFIER")]
    public async Task<IActionResult> Edit(int id)
    {
        var carte = await carteCompetenceService.GetByIdAsync(id);
        if (carte is null)
        {
            return NotFound();
        }

        return View("Save", VersFormModel(carte, await ListeBadgesAsync(carte.BadgeId)));
    }

    [HttpPost("Edit/{id:int}")]
    [Authorize(Policy = "Droit:CARTE.MODIFIER")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, CarteCompetenceFormModel model)
    {
        if (!ModelState.IsValid)
        {
            model.Badges = await ListeBadgesAsync(model.BadgeId);
            return View("Save", model);
        }

        var (nomFichierImage, erreurUpload) = await EnregistrerImageAsync(model.ImageCarteAFichier);
        if (erreurUpload is not null)
        {
            ModelState.AddModelError(nameof(model.ImageCarteAFichier), erreurUpload);
            model.Badges = await ListeBadgesAsync(model.BadgeId);
            return View("Save", model);
        }

        // Une image nouvellement uploadee remplace l'existante ; sans nouvel upload, on
        // conserve le nom de fichier deja enregistre (ImageCarteAActuelle, poste en hidden).
        var nomFichierFinal = nomFichierImage ?? model.ImageCarteAActuelle;

        var (success, errorMessage) = await carteCompetenceService.UpdateAsync(id, VersInput(model, nomFichierFinal));

        if (!success)
        {
            ModelState.AddModelError(nameof(model.Code), errorMessage ?? "Impossible de modifier cette carte.");
            model.Badges = await ListeBadgesAsync(model.BadgeId);
            return View("Save", model);
        }

        TempData["StatusMessage"] = "Carte de compétences modifiée.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpGet("Details/{id:int}")]
    [Authorize(Policy = "Droit:CARTE.CONSULTER")]
    public async Task<IActionResult> Details(int id)
    {
        var carte = await carteCompetenceService.GetByIdAsync(id);
        if (carte is null)
        {
            return NotFound();
        }

        ViewData["Attributions"] = await carteCompetenceService.GetAttributionsPourCarteAsync(id);
        ViewData["Utilisateurs"] = await ListeUtilisateursAsync();

        return View(carte);
    }

    [HttpPost("Delete/{id:int}")]
    [Authorize(Policy = "Droit:CARTE.SUPPRIMER")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var (success, errorMessage) = await carteCompetenceService.DeleteAsync(id);
        TempData["StatusMessage"] = success ? "Carte de compétences supprimée." : errorMessage;
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("Details/{id:int}/Attribuer")]
    [Authorize(Policy = "Droit:CARTE.MODIFIER")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AttribuerDepuisCarte(int id, [FromForm] List<string>? utilisateurIds, string? contexte)
    {
        var attribuePar = userManager.GetUserId(User);
        if (attribuePar is null)
        {
            return Forbid();
        }

        var (success, errorMessage) = await carteCompetenceService.AttribuerAsync(
            [id], utilisateurIds ?? [], attribuePar, contexte);

        TempData["StatusMessage"] = success ? "Attribution enregistrée." : errorMessage;
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost("Attributions/{attributionId:int}/Desattribuer")]
    [Authorize(Policy = "Droit:CARTE.MODIFIER")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Desattribuer(int attributionId, int carteId)
    {
        var (success, errorMessage) = await carteCompetenceService.DesattribuerAsync(attributionId);
        TempData["StatusMessage"] = success ? "Attribution retirée." : errorMessage;
        return RedirectToAction(nameof(Details), new { id = carteId });
    }

    private async Task<List<SelectListItem>> ListeBadgesAsync(int? badgeIdSelectionne)
    {
        var badges = await carteCompetenceService.GetBadgesAsync();
        return badges.Select(b => new SelectListItem
        {
            Value = b.Id.ToString(),
            Text = $"{b.BadgeCode} — {b.BadgeNom}",
            Selected = b.Id == badgeIdSelectionne,
        }).ToList();
    }

    private async Task<List<(string Id, string NomComplet)>> ListeUtilisateursAsync()
    {
        return await Task.FromResult(userManager.Users
            .OrderBy(u => u.Nom)
            .ThenBy(u => u.Prenom)
            .ToList()
            .Select(u =>
            {
                var nomComplet = $"{u.Prenom} {u.Nom}".Trim();
                return (u.Id, string.IsNullOrWhiteSpace(nomComplet) ? (u.Email ?? u.Id) : nomComplet);
            })
            .ToList());
    }

    private async Task<(string? NomFichier, string? Erreur)> EnregistrerImageAsync(IFormFile? fichier)
    {
        if (fichier is null || fichier.Length == 0)
        {
            return (null, null);
        }

        var extension = Path.GetExtension(fichier.FileName);
        if (!ExtensionsImageAutorisees.Contains(extension, StringComparer.OrdinalIgnoreCase))
        {
            return (null, "Formats acceptés pour l'image de la carte : JPG, PNG, WEBP, GIF.");
        }

        if (fichier.Length > TailleMaxImageOctets)
        {
            return (null, "L'image ne doit pas dépasser 5 Mo.");
        }

        var dossierUploads = Path.Combine(webHostEnvironment.WebRootPath, "uploads", "cartes");
        Directory.CreateDirectory(dossierUploads);

        var nomFichier = $"{Guid.NewGuid():N}{extension.ToLowerInvariant()}";
        var cheminComplet = Path.Combine(dossierUploads, nomFichier);

        await using var flux = new FileStream(cheminComplet, FileMode.Create);
        await fichier.CopyToAsync(flux);

        return (nomFichier, null);
    }

    private static CarteCompetenceInput VersInput(CarteCompetenceFormModel model, string? nomFichierImage) => new()
    {
        Code = model.Code,
        BadgeId = model.BadgeId,
        Niveau = model.Niveau,
        TitreTheorie = model.TitreTheorie,
        Objectif1 = model.Objectif1,
        Objectif2 = model.Objectif2,
        Objectif3 = model.Objectif3,
        Objectif4 = model.Objectif4,
        Citation = model.Citation,
        AuteurCitation = model.AuteurCitation,
        ImageCarteA = nomFichierImage,
        TitreDefi = model.TitreDefi,
        ContextePro = model.ContextePro,
        ContextePerso = model.ContextePerso,
        TonDefi = model.TonDefi,
        Etape1 = model.Etape1,
        Etape2 = model.Etape2,
        Etape3 = model.Etape3,
        Etape4 = model.Etape4,
        Etape5 = model.Etape5,
        Tip1 = model.Tip1,
        Tip2 = model.Tip2,
        Tip3 = model.Tip3,
        Tip4 = model.Tip4,
        Tip5 = model.Tip5,
        CitationHumour = model.CitationHumour,
        LienVideo = model.LienVideo,
    };

    private static CarteCompetenceFormModel VersFormModel(CarteCompetence carte, List<SelectListItem> badges) => new()
    {
        Id = carte.Id,
        Code = carte.Code,
        BadgeId = carte.BadgeId,
        Niveau = carte.Niveau,
        TitreTheorie = carte.TitreTheorie,
        Objectif1 = carte.Objectif1,
        Objectif2 = carte.Objectif2,
        Objectif3 = carte.Objectif3,
        Objectif4 = carte.Objectif4,
        Citation = carte.Citation,
        AuteurCitation = carte.AuteurCitation,
        ImageCarteAActuelle = carte.ImageCarteA,
        TitreDefi = carte.TitreDefi,
        ContextePro = carte.ContextePro,
        ContextePerso = carte.ContextePerso,
        TonDefi = carte.TonDefi,
        Etape1 = carte.Etape1,
        Etape2 = carte.Etape2,
        Etape3 = carte.Etape3,
        Etape4 = carte.Etape4,
        Etape5 = carte.Etape5,
        Tip1 = carte.Tip1,
        Tip2 = carte.Tip2,
        Tip3 = carte.Tip3,
        Tip4 = carte.Tip4,
        Tip5 = carte.Tip5,
        CitationHumour = carte.CitationHumour,
        LienVideo = carte.LienVideo,
        Badges = badges,
    };

    public sealed class CarteCompetenceFormModel
    {
        public int? Id { get; set; }

        [Required(ErrorMessage = "Le code est obligatoire.")]
        [Display(Name = "Code")]
        public string Code { get; set; } = string.Empty;

        [Display(Name = "Badge")]
        public int? BadgeId { get; set; }

        [Required(ErrorMessage = "Le niveau est obligatoire.")]
        [Display(Name = "Niveau")]
        public NiveauCarte Niveau { get; set; }

        [Required(ErrorMessage = "Le titre (face Théorie) est obligatoire.")]
        [Display(Name = "Titre (Théorie)")]
        public string TitreTheorie { get; set; } = string.Empty;

        [Display(Name = "Objectif 1")]
        public string? Objectif1 { get; set; }

        [Display(Name = "Objectif 2")]
        public string? Objectif2 { get; set; }

        [Display(Name = "Objectif 3")]
        public string? Objectif3 { get; set; }

        [Display(Name = "Objectif 4")]
        public string? Objectif4 { get; set; }

        [Display(Name = "Citation")]
        public string? Citation { get; set; }

        [Display(Name = "Auteur de la citation")]
        public string? AuteurCitation { get; set; }

        public string? ImageCarteAActuelle { get; set; }

        [Display(Name = "Image (face Théorie)")]
        public IFormFile? ImageCarteAFichier { get; set; }

        [Display(Name = "Titre du défi")]
        public string? TitreDefi { get; set; }

        [Display(Name = "Contexte professionnel")]
        public string? ContextePro { get; set; }

        [Display(Name = "Contexte personnel")]
        public string? ContextePerso { get; set; }

        [Display(Name = "Ton du défi")]
        public string? TonDefi { get; set; }

        [Display(Name = "Étape 1")]
        public string? Etape1 { get; set; }

        [Display(Name = "Étape 2")]
        public string? Etape2 { get; set; }

        [Display(Name = "Étape 3")]
        public string? Etape3 { get; set; }

        [Display(Name = "Étape 4")]
        public string? Etape4 { get; set; }

        [Display(Name = "Étape 5")]
        public string? Etape5 { get; set; }

        [Display(Name = "Tip 1")]
        public string? Tip1 { get; set; }

        [Display(Name = "Tip 2")]
        public string? Tip2 { get; set; }

        [Display(Name = "Tip 3")]
        public string? Tip3 { get; set; }

        [Display(Name = "Tip 4")]
        public string? Tip4 { get; set; }

        [Display(Name = "Tip 5")]
        public string? Tip5 { get; set; }

        [Display(Name = "Citation humoristique")]
        public string? CitationHumour { get; set; }

        [Display(Name = "Lien vidéo")]
        [Url(ErrorMessage = "Le lien vidéo doit être une URL valide.")]
        public string? LienVideo { get; set; }

        public List<SelectListItem> Badges { get; set; } = [];
    }
}

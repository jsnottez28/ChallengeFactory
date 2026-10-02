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
    // Pas de conversion/redimensionnement cote serveur (cf. CLAUDE.md - aucune dependance
    // d'optimisation d'image payante ou a risque de stabilite) : l'image doit deja etre
    // optimisee avant l'upload, le serveur se contente de verifier qu'elle respecte le
    // standard et refuse tout fichier hors clous avec un message explicite.
    private static readonly string[] ExtensionsImageAutorisees = [".jpg", ".jpeg", ".png", ".webp", ".gif"];
    private const long TailleMaxImageOctets = 1024 * 1024;

    // Les cartes ne s'affichent jamais au-dela de 380x130px (cf. .carte-face-image dans
    // style-added.css, utilisee uniquement par _CarteFlipCard.cshtml) - 900px de cote le plus
    // long couvre large les ecrans retina/3x sans jamais justifier de deposer une image
    // source de plusieurs milliers de pixels.
    private const int TailleMaxImagePixels = 900;

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
            return (null, $"L'image ne doit pas dépasser {TailleMaxImageOctets / 1024} Ko une fois optimisée. Redimensionnez-la et compressez-la avant de l'importer (ex. squoosh.app, gratuit).");
        }

        await using var fluxMemoire = new MemoryStream();
        await fichier.CopyToAsync(fluxMemoire);
        var octets = fluxMemoire.ToArray();

        var dimensions = LireDimensionsImage(octets);
        if (dimensions is null)
        {
            return (null, "Impossible de lire les dimensions de cette image : le fichier est peut-être corrompu ou dans un format non standard.");
        }

        if (dimensions.Value.Largeur > TailleMaxImagePixels || dimensions.Value.Hauteur > TailleMaxImagePixels)
        {
            return (null, $"Cette image fait {dimensions.Value.Largeur}x{dimensions.Value.Hauteur}px : redimensionnez-la à {TailleMaxImagePixels}px de côté maximum avant de l'importer (ex. squoosh.app, gratuit). Les cartes ne s'affichent jamais plus grand que 380x130px, inutile de déposer une image plus grande.");
        }

        var dossierUploads = Path.Combine(webHostEnvironment.WebRootPath, "uploads", "cartes");
        Directory.CreateDirectory(dossierUploads);

        var nomFichier = $"{Guid.NewGuid():N}{extension.ToLowerInvariant()}";
        await System.IO.File.WriteAllBytesAsync(Path.Combine(dossierUploads, nomFichier), octets);

        return (nomFichier, null);
    }

    // Lit les dimensions d'une image PNG/JPEG/WebP/GIF directement depuis les octets d'en-tete,
    // sans decoder l'image entiere - aucune dependance (native ou commerciale) necessaire,
    // l'optimisation elle-meme reste a la charge de la personne qui depose l'image (cf.
    // TailleMaxImageOctets/TailleMaxImagePixels ci-dessus). Renvoie null si le format n'est pas
    // reconnu ou si l'en-tete est trop court/corrompu.
    private static (int Largeur, int Hauteur)? LireDimensionsImage(byte[] octets)
    {
        if (octets.Length >= 24 && octets[0] == 0x89 && octets[1] == 0x50 && octets[2] == 0x4E && octets[3] == 0x47)
        {
            // PNG : signature (8 octets) puis chunk IHDR (largeur/hauteur en big-endian, 4 octets chacun).
            var largeur = (octets[16] << 24) | (octets[17] << 16) | (octets[18] << 8) | octets[19];
            var hauteur = (octets[20] << 24) | (octets[21] << 16) | (octets[22] << 8) | octets[23];
            return (largeur, hauteur);
        }

        if (octets.Length >= 10 && octets[0] == 0x47 && octets[1] == 0x49 && octets[2] == 0x46)
        {
            // GIF : signature "GIF87a"/"GIF89a" (6 octets) puis largeur/hauteur en little-endian, 2 octets chacun.
            var largeur = octets[6] | (octets[7] << 8);
            var hauteur = octets[8] | (octets[9] << 8);
            return (largeur, hauteur);
        }

        if (octets.Length >= 21 && octets[0] == 0x52 && octets[1] == 0x49 && octets[2] == 0x46 && octets[3] == 0x46
            && octets[8] == 0x57 && octets[9] == 0x45 && octets[10] == 0x42 && octets[11] == 0x50)
        {
            return LireDimensionsWebp(octets);
        }

        if (octets.Length >= 2 && octets[0] == 0xFF && octets[1] == 0xD8)
        {
            return LireDimensionsJpeg(octets);
        }

        return null;
    }

    private static (int Largeur, int Hauteur)? LireDimensionsWebp(byte[] octets)
    {
        var fourCc = System.Text.Encoding.ASCII.GetString(octets, 12, 4);
        switch (fourCc)
        {
            case "VP8X":
                // Chunk VP8X : flags (1 octet) + reserve (3 octets), puis largeur-1/hauteur-1 sur
                // 3 octets little-endian chacun.
                if (octets.Length < 30)
                {
                    return null;
                }
                var largeurX = (octets[24] | (octets[25] << 8) | (octets[26] << 16)) + 1;
                var hauteurX = (octets[27] | (octets[28] << 8) | (octets[29] << 16)) + 1;
                return (largeurX, hauteurX);
            case "VP8 ":
                // Bitstream VP8 "lossy" : tag de frame (3 octets) + code de synchronisation
                // 0x9D 0x01 0x2A, puis largeur/hauteur sur 14 bits chacun (les 2 bits de poids
                // fort servent a l'echelle et sont ignores).
                if (octets.Length < 30 || octets[23] != 0x9D || octets[24] != 0x01 || octets[25] != 0x2A)
                {
                    return null;
                }
                var largeurL = (octets[26] | (octets[27] << 8)) & 0x3FFF;
                var hauteurL = (octets[28] | (octets[29] << 8)) & 0x3FFF;
                return (largeurL, hauteurL);
            case "VP8L":
                // Lossless : signature 0x2F puis largeur-1/hauteur-1 sur 14 bits chacun, empaquetes
                // sur 4 octets little-endian.
                if (octets.Length < 25 || octets[20] != 0x2F)
                {
                    return null;
                }
                var bits = octets[21] | (octets[22] << 8) | (octets[23] << 16) | (octets[24] << 24);
                var largeurLL = (bits & 0x3FFF) + 1;
                var hauteurLL = ((bits >> 14) & 0x3FFF) + 1;
                return (largeurLL, hauteurLL);
            default:
                return null;
        }
    }

    private static (int Largeur, int Hauteur)? LireDimensionsJpeg(byte[] octets)
    {
        var position = 2; // apres le marqueur SOI (0xFFD8)
        while (position + 4 <= octets.Length)
        {
            if (octets[position] != 0xFF)
            {
                position++;
                continue;
            }

            // Octets de bourrage 0xFF eventuels avant le vrai marqueur.
            while (position + 1 < octets.Length && octets[position + 1] == 0xFF)
            {
                position++;
            }

            var marqueur = octets[position + 1];
            position += 2;

            // Marqueurs sans segment de longueur (TEM, RST0-RST7, SOI, EOI).
            if (marqueur == 0x01 || (marqueur >= 0xD0 && marqueur <= 0xD7) || marqueur == 0xD8 || marqueur == 0xD9)
            {
                continue;
            }

            if (position + 2 > octets.Length)
            {
                return null;
            }

            var longueurSegment = (octets[position] << 8) | octets[position + 1];

            // Marqueurs SOFn (debut de frame) : 0xC0-0xCF sauf 0xC4 (DHT), 0xC8 (reserve), 0xCC (DAC).
            var estSof = marqueur >= 0xC0 && marqueur <= 0xCF && marqueur != 0xC4 && marqueur != 0xC8 && marqueur != 0xCC;
            if (estSof)
            {
                if (position + 7 > octets.Length)
                {
                    return null;
                }
                var hauteur = (octets[position + 3] << 8) | octets[position + 4];
                var largeur = (octets[position + 5] << 8) | octets[position + 6];
                return (largeur, hauteur);
            }

            if (marqueur == 0xDA)
            {
                // Debut du flux de donnees scanne : aucun marqueur SOF trouve avant.
                return null;
            }

            position += longueurSegment;
        }

        return null;
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

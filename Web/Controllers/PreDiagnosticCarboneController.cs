using System.ComponentModel.DataAnnotations;
using Application.Common.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers;

// Pre-diagnostic carbone : estimation rapide en approche monetaire, accessible sans
// compte, distincte du Bilan Carbone(R) complet reserve a une prestation avec un praticien
// - cf. IPreBilanCarboneService pour le detail de la demarche et du choix de vocabulaire.
[Route("pre-diagnostic-carbone")]
public class PreDiagnosticCarboneController(IPreBilanCarboneService preBilanService) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var facteurs = await preBilanService.GetFacteursDisponiblesAsync();
        ViewData["Facteurs"] = facteurs;
        return View(new PreDiagnosticCarboneFormModel());
    }

    [HttpPost("")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(PreDiagnosticCarboneFormModel model)
    {
        var facteurs = await preBilanService.GetFacteursDisponiblesAsync();
        ViewData["Facteurs"] = facteurs;

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var valeurs = new Dictionary<string, decimal>();
        foreach (var facteur in facteurs)
        {
            if (model.Valeurs.TryGetValue(facteur.Code, out var valeur) && valeur is > 0)
            {
                valeurs[facteur.Code] = valeur.Value;
            }
        }

        var (success, errorMessage, resultat) = await preBilanService.DeposerAsync(new PreBilanCarboneInput
        {
            Email = model.Email,
            Nom = model.Nom,
            Prenom = model.Prenom,
            Societe = model.Societe,
            Telephone = model.Telephone,
            SecteurActivite = model.SecteurActivite,
            EffectifEtp = model.EffectifEtp,
            ChiffreAffairesKEuros = model.ChiffreAffairesKEuros,
            ValeursParCodeFacteur = valeurs,
        });

        if (!success)
        {
            ModelState.AddModelError(string.Empty, errorMessage ?? "Une erreur est survenue.");
            return View(model);
        }

        return View("Resultat", resultat);
    }

    public class PreDiagnosticCarboneFormModel
    {
        [Required(ErrorMessage = "Merci d'indiquer votre email.")]
        [EmailAddress(ErrorMessage = "Adresse email invalide.")]
        [StringLength(256)]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [StringLength(150)]
        [Display(Name = "Nom")]
        public string? Nom { get; set; }

        [StringLength(150)]
        [Display(Name = "Prénom")]
        public string? Prenom { get; set; }

        [StringLength(200)]
        [Display(Name = "Société")]
        public string? Societe { get; set; }

        [StringLength(30)]
        [Display(Name = "Téléphone")]
        public string? Telephone { get; set; }

        [StringLength(150)]
        [Display(Name = "Secteur d'activité")]
        public string? SecteurActivite { get; set; }

        [Range(0, 1_000_000)]
        [Display(Name = "Effectif (ETP)")]
        public int? EffectifEtp { get; set; }

        [Range(0, 10_000_000)]
        [Display(Name = "Chiffre d'affaires (k€)")]
        public decimal? ChiffreAffairesKEuros { get; set; }

        [Required(ErrorMessage = "Merci d'accepter que vos coordonnées soient utilisées pour vous recontacter au sujet de ce pré-diagnostic.")]
        [Display(Name = "Consentement")]
        public bool ConsentementRgpd { get; set; }

        // Cle = FacteurEmissionInfo.Code, lu depuis Request.Form["Valeurs[CODE]"] via le
        // binder par defaut - meme principe que les autres formulaires dynamiques de la
        // plateforme (montants/quantites par categorie, jamais de liste fixe en dur cote
        // vue). decimal? (et non decimal) : un champ laisse vide soumet une chaine vide,
        // que le binder ASP.NET Core refuse pour un type valeur non-nullable ("The value
        // '' is invalid.", un par champ vide) - nullable, il la traite simplement comme
        // "non renseigne" (null), sans erreur de validation.
        public Dictionary<string, decimal?> Valeurs { get; set; } = [];
    }
}

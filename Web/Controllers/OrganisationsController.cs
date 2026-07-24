using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Web.Configuration;
using Web.Services;

namespace Web.Controllers;

[Route("Admin/[controller]")]
public class OrganisationsController(
    IOrganisationService organisationService,
    IOptions<ExternalApiOptions> externalApiOptions) : Controller
{
    [HttpGet("")]
    [Authorize(Policy = "Droit:ORGANISATION.CONSULTER")]
    public async Task<IActionResult> Index()
    {
        var organisations = await organisationService.GetAllAsync();
        return View(organisations);
    }

    [HttpGet("Create")]
    [Authorize(Policy = "Droit:ORGANISATION.CREER")]
    public IActionResult Create()
    {
        ViewBag.GeoCommunesApiUrl = externalApiOptions.Value.GeoCommunesBaseUrl;
        return View("Save", new OrganisationFormModel());
    }

    [HttpPost("Create")]
    [Authorize(Policy = "Droit:ORGANISATION.CREER")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(OrganisationFormModel model)
    {
        ViewBag.GeoCommunesApiUrl = externalApiOptions.Value.GeoCommunesBaseUrl;

        if (!ModelState.IsValid)
        {
            return View("Save", model);
        }

        await organisationService.CreateAsync(ToInput(model));
        TempData["StatusMessage"] = "Organisation créée.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("Edit/{id:int}")]
    [Authorize(Policy = "Droit:ORGANISATION.MODIFIER")]
    public async Task<IActionResult> Edit(int id)
    {
        var organisation = await organisationService.GetByIdAsync(id);
        if (organisation is null)
        {
            TempData["StatusMessage"] = "Organisation introuvable.";
            return RedirectToAction(nameof(Index));
        }

        ViewBag.GeoCommunesApiUrl = externalApiOptions.Value.GeoCommunesBaseUrl;

        return View("Save", new OrganisationFormModel
        {
            Id = organisation.Id,
            CodeAdherent = organisation.CodeAdherent,
            RaisonSociale = organisation.RaisonSociale,
            Adresse1 = organisation.Adresse1 ?? string.Empty,
            Adresse2 = organisation.Adresse2,
            CodePostal = organisation.CodePostal ?? string.Empty,
            Ville = organisation.Ville ?? string.Empty,
            TelephoneStandard = organisation.TelephoneStandard,
            EstActif = organisation.EstActif,
        });
    }

    [HttpPost("Edit/{id:int}")]
    [Authorize(Policy = "Droit:ORGANISATION.MODIFIER")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, OrganisationFormModel model)
    {
        ViewBag.GeoCommunesApiUrl = externalApiOptions.Value.GeoCommunesBaseUrl;

        if (!ModelState.IsValid)
        {
            return View("Save", model);
        }

        var updated = await organisationService.UpdateAsync(id, ToInput(model));
        if (!updated)
        {
            TempData["StatusMessage"] = "Organisation introuvable.";
            return RedirectToAction(nameof(Index));
        }

        TempData["StatusMessage"] = "Organisation modifiée.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("ToggleActive/{id:int}")]
    [Authorize(Policy = "Droit:ORGANISATION.MODIFIER")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleActive(int id)
    {
        var organisation = await organisationService.GetByIdAsync(id);
        if (organisation is null)
        {
            TempData["StatusMessage"] = "Organisation introuvable.";
            return RedirectToAction(nameof(Index));
        }

        var nouveauStatut = !organisation.EstActif;
        await organisationService.SetActiveAsync(id, nouveauStatut);

        TempData["StatusMessage"] = nouveauStatut
            ? "Organisation activée."
            : "Organisation désactivée.";
        return RedirectToAction(nameof(Index));
    }

    private static OrganisationInput ToInput(OrganisationFormModel model) => new()
    {
        CodeAdherent = model.CodeAdherent,
        RaisonSociale = model.RaisonSociale,
        Adresse1 = model.Adresse1,
        Adresse2 = model.Adresse2,
        CodePostal = model.CodePostal,
        Ville = model.Ville,
        TelephoneStandard = model.TelephoneStandard,
        EstActif = model.EstActif,
    };

    public sealed class OrganisationFormModel
    {
        public int? Id { get; set; }

        [Required(ErrorMessage = "Le code adhérent est obligatoire.")]
        [Display(Name = "Code adhérent")]
        public string CodeAdherent { get; set; } = string.Empty;

        [Required(ErrorMessage = "La raison sociale est obligatoire.")]
        [Display(Name = "Raison sociale")]
        public string RaisonSociale { get; set; } = string.Empty;

        [Required(ErrorMessage = "L'adresse est obligatoire.")]
        [StringLength(200, ErrorMessage = "L'adresse ne doit pas dépasser 200 caractères.")]
        [Display(Name = "Adresse")]
        public string Adresse1 { get; set; } = string.Empty;

        [StringLength(200, ErrorMessage = "Le complément d'adresse ne doit pas dépasser 200 caractères.")]
        [Display(Name = "Complément d'adresse")]
        public string? Adresse2 { get; set; }

        [Required(ErrorMessage = "Le code postal est obligatoire.")]
        [RegularExpression(@"^\d{5}$", ErrorMessage = "Le code postal doit contenir 5 chiffres.")]
        [Display(Name = "Code postal")]
        public string CodePostal { get; set; } = string.Empty;

        [Required(ErrorMessage = "La ville est obligatoire.")]
        [RegularExpression(@"^[A-Za-zÀ-ÖØ-öø-ÿ' \-]+$", ErrorMessage = "La ville ne doit pas contenir de chiffres ou de caractères invalides.")]
        [Display(Name = "Ville")]
        public string Ville { get; set; } = string.Empty;

        [RegularExpression(@"^(?:\+33|0)[1-9](?:[ .\-]?\d{2}){4}$", ErrorMessage = "Le téléphone doit être un numéro français valide (ex: 01 23 45 67 89).")]
        [Display(Name = "Téléphone standard")]
        public string? TelephoneStandard { get; set; }

        [Display(Name = "Organisation active")]
        public bool EstActif { get; set; } = true;
    }
}

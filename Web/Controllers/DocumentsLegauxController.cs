using System.ComponentModel.DataAnnotations;
using Application.Common.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers;

[Route("Administration/[controller]")]
public class DocumentsLegauxController(IDocumentLegalService documentLegalService) : Controller
{
    [HttpGet("")]
    [Authorize(Policy = "Droit:DOCUMENTLEGAL.CONSULTER")]
    public async Task<IActionResult> Index()
    {
        var versionsPubliees = new Dictionary<TypeDocumentLegal, DocumentLegal?>();
        foreach (var type in Enum.GetValues<TypeDocumentLegal>())
        {
            versionsPubliees[type] = await documentLegalService.GetVersionPublieeAsync(type);
        }

        return View(versionsPubliees);
    }

    [HttpGet("Historique/{type}")]
    [Authorize(Policy = "Droit:DOCUMENTLEGAL.CONSULTER")]
    public async Task<IActionResult> Historique(TypeDocumentLegal type)
    {
        var historique = await documentLegalService.GetHistoriqueAsync(type);
        ViewBag.Type = type;
        return View(historique);
    }

    [HttpGet("Create/{type}")]
    [Authorize(Policy = "Droit:DOCUMENTLEGAL.CREER")]
    public IActionResult Create(TypeDocumentLegal type)
    {
        return View(new DocumentLegalFormModel
        {
            Type = type,
            DateEffective = DateTime.UtcNow,
        });
    }

    [HttpPost("Create/{type}")]
    [Authorize(Policy = "Droit:DOCUMENTLEGAL.CREER")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TypeDocumentLegal type, DocumentLegalFormModel model)
    {
        model.Type = type;

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var (success, errorMessage, _) = await documentLegalService.CreerEtPublierAsync(new DocumentLegalInput
        {
            Type = model.Type,
            Version = model.Version,
            Contenu = model.Contenu,
            DateEffective = model.DateEffective,
        });

        if (!success)
        {
            ModelState.AddModelError(nameof(model.Version), errorMessage ?? "Impossible de publier cette version.");
            return View(model);
        }

        TempData["StatusMessage"] = "Nouvelle version publiee. Les comptes n'ayant pas accepte cette version seront invites (ou bloques) a le faire.";
        return RedirectToAction(nameof(Index));
    }

    public sealed class DocumentLegalFormModel
    {
        public TypeDocumentLegal Type { get; set; }

        [Required(ErrorMessage = "La version est obligatoire.")]
        [Display(Name = "Version")]
        public string Version { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le contenu est obligatoire.")]
        [Display(Name = "Contenu (Markdown)")]
        public string Contenu { get; set; } = string.Empty;

        [Required(ErrorMessage = "La date d'effet est obligatoire.")]
        [Display(Name = "Date d'effet")]
        public DateTime DateEffective { get; set; }
    }
}

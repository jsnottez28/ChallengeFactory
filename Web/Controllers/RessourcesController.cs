using System.ComponentModel.DataAnnotations;
using Application.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers;

[Route("Administration/[controller]")]
public class RessourcesController(IRessourceService ressourceService) : Controller
{
    [HttpGet("")]
    [Authorize(Policy = "Droit:RESSOURCE.CONSULTER")]
    public async Task<IActionResult> Index()
    {
        var ressources = await ressourceService.GetAllAsync();
        return View(ressources);
    }

    [HttpGet("Create")]
    [Authorize(Policy = "Droit:RESSOURCE.CREER")]
    public IActionResult Create()
    {
        return View("Save", new RessourceFormModel());
    }

    [HttpPost("Create")]
    [Authorize(Policy = "Droit:RESSOURCE.CREER")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(RessourceFormModel model)
    {
        if (!ModelState.IsValid)
        {
            return View("Save", model);
        }

        var (success, errorMessage, _) = await ressourceService.CreateAsync(new RessourceInput
        {
            Code = model.Code,
            Libelle = model.Libelle,
            Description = model.Description,
        });

        if (!success)
        {
            ModelState.AddModelError(nameof(model.Code), errorMessage ?? "Impossible de creer cette ressource.");
            return View("Save", model);
        }

        TempData["StatusMessage"] = "Ressource creee.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("Edit/{id:int}")]
    [Authorize(Policy = "Droit:RESSOURCE.MODIFIER")]
    public async Task<IActionResult> Edit(int id)
    {
        var ressource = await ressourceService.GetByIdAsync(id);
        if (ressource is null)
        {
            return NotFound();
        }

        return View("Save", new RessourceFormModel
        {
            Id = ressource.Id,
            Code = ressource.Code,
            Libelle = ressource.Libelle,
            Description = ressource.Description,
        });
    }

    [HttpPost("Edit/{id:int}")]
    [Authorize(Policy = "Droit:RESSOURCE.MODIFIER")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, RessourceFormModel model)
    {
        if (!ModelState.IsValid)
        {
            return View("Save", model);
        }

        var (success, errorMessage) = await ressourceService.UpdateAsync(id, new RessourceInput
        {
            Code = model.Code,
            Libelle = model.Libelle,
            Description = model.Description,
        });

        if (!success)
        {
            ModelState.AddModelError(nameof(model.Code), errorMessage ?? "Impossible de modifier cette ressource.");
            return View("Save", model);
        }

        TempData["StatusMessage"] = "Ressource modifiee.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("Delete/{id:int}")]
    [Authorize(Policy = "Droit:RESSOURCE.SUPPRIMER")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var (success, errorMessage) = await ressourceService.DeleteAsync(id);
        TempData["StatusMessage"] = success ? "Ressource supprimee." : errorMessage;
        return RedirectToAction(nameof(Index));
    }

    public sealed class RessourceFormModel
    {
        public int? Id { get; set; }

        [Required(ErrorMessage = "Le code est obligatoire.")]
        [Display(Name = "Code")]
        public string Code { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le libelle est obligatoire.")]
        [Display(Name = "Libelle")]
        public string Libelle { get; set; } = string.Empty;

        [Display(Name = "Description")]
        public string? Description { get; set; }
    }
}

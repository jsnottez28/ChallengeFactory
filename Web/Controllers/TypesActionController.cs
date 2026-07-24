using System.ComponentModel.DataAnnotations;
using Application.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers;

[Route("Administration/[controller]")]
public class TypesActionController(ITypeActionService typeActionService) : Controller
{
    [HttpGet("")]
    [Authorize(Policy = "Droit:TYPEACTION.CONSULTER")]
    public async Task<IActionResult> Index()
    {
        var typesAction = await typeActionService.GetAllAsync();
        return View(typesAction);
    }

    [HttpGet("Create")]
    [Authorize(Policy = "Droit:TYPEACTION.CREER")]
    public IActionResult Create()
    {
        return View("Save", new TypeActionFormModel());
    }

    [HttpPost("Create")]
    [Authorize(Policy = "Droit:TYPEACTION.CREER")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TypeActionFormModel model)
    {
        if (!ModelState.IsValid)
        {
            return View("Save", model);
        }

        var (success, errorMessage, _) = await typeActionService.CreateAsync(new TypeActionInput
        {
            Code = model.Code,
            Libelle = model.Libelle,
            Description = model.Description,
        });

        if (!success)
        {
            ModelState.AddModelError(nameof(model.Code), errorMessage ?? "Impossible de creer ce type d'action.");
            return View("Save", model);
        }

        TempData["StatusMessage"] = "Type d'action cree.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("Edit/{id:int}")]
    [Authorize(Policy = "Droit:TYPEACTION.MODIFIER")]
    public async Task<IActionResult> Edit(int id)
    {
        var typeAction = await typeActionService.GetByIdAsync(id);
        if (typeAction is null)
        {
            return NotFound();
        }

        return View("Save", new TypeActionFormModel
        {
            Id = typeAction.Id,
            Code = typeAction.Code,
            Libelle = typeAction.Libelle,
            Description = typeAction.Description,
        });
    }

    [HttpPost("Edit/{id:int}")]
    [Authorize(Policy = "Droit:TYPEACTION.MODIFIER")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, TypeActionFormModel model)
    {
        if (!ModelState.IsValid)
        {
            return View("Save", model);
        }

        var (success, errorMessage) = await typeActionService.UpdateAsync(id, new TypeActionInput
        {
            Code = model.Code,
            Libelle = model.Libelle,
            Description = model.Description,
        });

        if (!success)
        {
            ModelState.AddModelError(nameof(model.Code), errorMessage ?? "Impossible de modifier ce type d'action.");
            return View("Save", model);
        }

        TempData["StatusMessage"] = "Type d'action modifie.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("Delete/{id:int}")]
    [Authorize(Policy = "Droit:TYPEACTION.SUPPRIMER")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var (success, errorMessage) = await typeActionService.DeleteAsync(id);
        TempData["StatusMessage"] = success ? "Type d'action supprime." : errorMessage;
        return RedirectToAction(nameof(Index));
    }

    public sealed class TypeActionFormModel
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

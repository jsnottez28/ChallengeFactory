using System.ComponentModel.DataAnnotations;
using Application.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers;

[Route("Administration/[controller]")]
public class GroupesDroitController(IGroupeDroitService groupeDroitService) : Controller
{
    [HttpGet("")]
    [Authorize(Policy = "Droit:GROUPEDROIT.CONSULTER")]
    public async Task<IActionResult> Index()
    {
        var groupes = await groupeDroitService.GetAllAsync();
        return View(groupes);
    }

    [HttpGet("Create")]
    [Authorize(Policy = "Droit:GROUPEDROIT.CREER")]
    public IActionResult Create()
    {
        return View("Save", new GroupeDroitFormModel());
    }

    [HttpPost("Create")]
    [Authorize(Policy = "Droit:GROUPEDROIT.CREER")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(GroupeDroitFormModel model)
    {
        if (!ModelState.IsValid)
        {
            return View("Save", model);
        }

        var (success, errorMessage, groupe) = await groupeDroitService.CreateAsync(new GroupeDroitInput
        {
            Code = model.Code,
            Libelle = model.Libelle,
            Description = model.Description,
        });

        if (!success)
        {
            ModelState.AddModelError(nameof(model.Code), errorMessage ?? "Impossible de creer ce groupe.");
            return View("Save", model);
        }

        TempData["StatusMessage"] = "Groupe cree. Vous pouvez maintenant y rattacher des droits depuis l'ecran Droits.";
        return RedirectToAction(nameof(Edit), new { id = groupe!.Id });
    }

    [HttpGet("Edit/{id:int}")]
    [Authorize(Policy = "Droit:GROUPEDROIT.MODIFIER")]
    public async Task<IActionResult> Edit(int id)
    {
        var groupe = await groupeDroitService.GetByIdAsync(id);
        if (groupe is null)
        {
            return NotFound();
        }

        ViewBag.Groupe = groupe;
        return View("Save", new GroupeDroitFormModel
        {
            Id = groupe.Id,
            Code = groupe.Code,
            Libelle = groupe.Libelle,
            Description = groupe.Description,
        });
    }

    [HttpPost("Edit/{id:int}")]
    [Authorize(Policy = "Droit:GROUPEDROIT.MODIFIER")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, GroupeDroitFormModel model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Groupe = await groupeDroitService.GetByIdAsync(id);
            return View("Save", model);
        }

        var (success, errorMessage) = await groupeDroitService.UpdateAsync(id, new GroupeDroitInput
        {
            Code = model.Code,
            Libelle = model.Libelle,
            Description = model.Description,
        });

        if (!success)
        {
            ModelState.AddModelError(nameof(model.Code), errorMessage ?? "Impossible de modifier ce groupe.");
            ViewBag.Groupe = await groupeDroitService.GetByIdAsync(id);
            return View("Save", model);
        }

        TempData["StatusMessage"] = "Groupe modifie.";
        return RedirectToAction(nameof(Edit), new { id });
    }

    [HttpPost("Delete/{id:int}")]
    [Authorize(Policy = "Droit:GROUPEDROIT.SUPPRIMER")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var (success, errorMessage) = await groupeDroitService.DeleteAsync(id);
        TempData["StatusMessage"] = success ? "Groupe supprime." : errorMessage;
        return RedirectToAction(nameof(Index));
    }

    public sealed class GroupeDroitFormModel
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

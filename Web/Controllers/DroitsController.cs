using System.ComponentModel.DataAnnotations;
using Application.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers;

[Route("Administration/[controller]")]
public class DroitsController(
    IDroitService droitService,
    IRessourceService ressourceService,
    ITypeActionService typeActionService,
    IGroupeDroitService groupeDroitService) : Controller
{
    [HttpGet("")]
    [Authorize(Policy = "Droit:DROIT.CONSULTER")]
    public async Task<IActionResult> Index(int? ressourceId, int? groupeDroitId)
    {
        var droits = await droitService.GetAllAsync(ressourceId, groupeDroitId);

        ViewBag.Ressources = await ressourceService.GetAllAsync();
        ViewBag.Groupes = await groupeDroitService.GetAllAsync();
        ViewBag.RessourceId = ressourceId;
        ViewBag.GroupeDroitId = groupeDroitId;

        return View(droits);
    }

    [HttpGet("Create")]
    [Authorize(Policy = "Droit:DROIT.CREER")]
    public async Task<IActionResult> Create()
    {
        await ChargerListesAsync();
        return View("Save", new DroitFormModel());
    }

    [HttpPost("Create")]
    [Authorize(Policy = "Droit:DROIT.CREER")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(DroitFormModel model)
    {
        if (!ModelState.IsValid)
        {
            await ChargerListesAsync();
            return View("Save", model);
        }

        var (success, errorMessage, _) = await droitService.CreateAsync(new DroitInput
        {
            Code = model.Code,
            Libelle = model.Libelle,
            Description = model.Description,
            RessourceId = model.RessourceId!.Value,
            TypeActionId = model.TypeActionId!.Value,
            GroupeDroitId = model.GroupeDroitId,
        });

        if (!success)
        {
            ModelState.AddModelError(string.Empty, errorMessage ?? "Impossible de creer ce droit.");
            await ChargerListesAsync();
            return View("Save", model);
        }

        TempData["StatusMessage"] = "Droit cree.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("Edit/{id:int}")]
    [Authorize(Policy = "Droit:DROIT.MODIFIER")]
    public async Task<IActionResult> Edit(int id)
    {
        var droit = await droitService.GetByIdAsync(id);
        if (droit is null)
        {
            return NotFound();
        }

        await ChargerListesAsync();
        return View("Save", new DroitFormModel
        {
            Id = droit.Id,
            Code = droit.Code,
            Libelle = droit.Libelle,
            Description = droit.Description,
            RessourceId = droit.RessourceId,
            TypeActionId = droit.TypeActionId,
            GroupeDroitId = droit.GroupeDroitId,
        });
    }

    [HttpPost("Edit/{id:int}")]
    [Authorize(Policy = "Droit:DROIT.MODIFIER")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, DroitFormModel model)
    {
        if (!ModelState.IsValid)
        {
            await ChargerListesAsync();
            return View("Save", model);
        }

        var (success, errorMessage) = await droitService.UpdateAsync(id, new DroitInput
        {
            Code = model.Code,
            Libelle = model.Libelle,
            Description = model.Description,
            RessourceId = model.RessourceId!.Value,
            TypeActionId = model.TypeActionId!.Value,
            GroupeDroitId = model.GroupeDroitId,
        });

        if (!success)
        {
            ModelState.AddModelError(string.Empty, errorMessage ?? "Impossible de modifier ce droit.");
            await ChargerListesAsync();
            return View("Save", model);
        }

        TempData["StatusMessage"] = "Droit modifie.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("Delete/{id:int}")]
    [Authorize(Policy = "Droit:DROIT.SUPPRIMER")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var (success, errorMessage) = await droitService.DeleteAsync(id);
        TempData["StatusMessage"] = success ? "Droit supprime." : errorMessage;
        return RedirectToAction(nameof(Index));
    }

    private async Task ChargerListesAsync()
    {
        ViewBag.Ressources = await ressourceService.GetAllAsync();
        ViewBag.TypesAction = await typeActionService.GetAllAsync();
        ViewBag.Groupes = await groupeDroitService.GetAllAsync();
    }

    public sealed class DroitFormModel
    {
        public int? Id { get; set; }

        [Display(Name = "Code")]
        public string? Code { get; set; }

        [Required(ErrorMessage = "Le libelle est obligatoire.")]
        [Display(Name = "Libelle")]
        public string Libelle { get; set; } = string.Empty;

        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "La ressource est obligatoire.")]
        [Display(Name = "Ressource")]
        public int? RessourceId { get; set; }

        [Required(ErrorMessage = "Le type d'action est obligatoire.")]
        [Display(Name = "Type d'action")]
        public int? TypeActionId { get; set; }

        [Display(Name = "Groupe de droits")]
        public int? GroupeDroitId { get; set; }
    }
}

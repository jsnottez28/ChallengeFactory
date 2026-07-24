using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web.Data;

namespace Web.Controllers;

[Route("Administration/[controller]")]
public class RolesController(RoleManager<ApplicationRole> roleManager) : Controller
{
    [HttpGet("")]
    [Authorize(Policy = "Droit:ROLE.CONSULTER")]
    public async Task<IActionResult> Index()
    {
        var roles = await roleManager.Roles
            .OrderBy(role => role.Name)
            .ToListAsync();

        return View(roles);
    }

    [HttpGet("Create")]
    [Authorize(Policy = "Droit:ROLE.CREER")]
    public IActionResult Create()
    {
        return View(new RoleFormModel());
    }

    [HttpPost("Create")]
    [Authorize(Policy = "Droit:ROLE.CREER")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(RoleFormModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        if (await roleManager.RoleExistsAsync(model.Name))
        {
            ModelState.AddModelError(nameof(model.Name), "Un role avec ce nom existe deja.");
            return View(model);
        }

        var result = await roleManager.CreateAsync(new ApplicationRole
        {
            Name = model.Name,
            Description = model.Description,
        });

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        TempData["StatusMessage"] = "Role cree.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("Rename/{id}")]
    [Authorize(Policy = "Droit:ROLE.MODIFIER")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Rename(string id, string name, string? description)
    {
        var role = await roleManager.FindByIdAsync(id);
        if (role is null)
        {
            TempData["StatusMessage"] = "Role introuvable.";
            return RedirectToAction(nameof(Index));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            TempData["StatusMessage"] = "Le nom du role est obligatoire.";
            return RedirectToAction(nameof(Index));
        }

        role.Name = name;
        role.Description = description;
        await roleManager.UpdateAsync(role);

        TempData["StatusMessage"] = "Role modifie.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("Delete/{id}")]
    [Authorize(Policy = "Droit:ROLE.SUPPRIMER")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id)
    {
        var role = await roleManager.FindByIdAsync(id);
        if (role is null)
        {
            TempData["StatusMessage"] = "Role introuvable.";
            return RedirectToAction(nameof(Index));
        }

        var result = await roleManager.DeleteAsync(role);
        TempData["StatusMessage"] = result.Succeeded
            ? "Role supprime."
            : string.Join(" ", result.Errors.Select(error => error.Description));

        return RedirectToAction(nameof(Index));
    }

    public sealed class RoleFormModel
    {
        [Required(ErrorMessage = "Le nom du role est obligatoire.")]
        [Display(Name = "Nom")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Description")]
        public string? Description { get; set; }
    }
}

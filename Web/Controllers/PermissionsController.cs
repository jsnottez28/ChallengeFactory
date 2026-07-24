using Application.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web.Data;

namespace Web.Controllers;

[Route("Administration/Permissions")]
public class PermissionsController(IPermissionService permissionService, RoleManager<ApplicationRole> roleManager) : Controller
{
    [HttpGet("")]
    [Authorize(Policy = "Droit:PERMISSION.CONSULTER")]
    public async Task<IActionResult> Index()
    {
        var roles = await roleManager.Roles
            .OrderBy(role => role.Name)
            .ToListAsync();

        return View(roles);
    }

    [HttpGet("{roleId}")]
    [Authorize(Policy = "Droit:PERMISSION.CONSULTER")]
    public async Task<IActionResult> Role(string roleId)
    {
        var permissions = await permissionService.GetRolePermissionsAsync(roleId);
        if (permissions is null)
        {
            return NotFound();
        }

        return View(permissions);
    }

    [HttpPost("{roleId}")]
    [Authorize(Policy = "Droit:PERMISSION.MODIFIER")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Role(string roleId, [FromForm] List<int>? droitIds, [FromForm] List<int>? groupeDroitIds)
    {
        var (success, errorMessage) = await permissionService.UpdateRolePermissionsAsync(
            roleId,
            droitIds ?? [],
            groupeDroitIds ?? []);

        TempData["StatusMessage"] = success ? "Permissions enregistrees." : errorMessage;
        return RedirectToAction(nameof(Role), new { roleId });
    }
}

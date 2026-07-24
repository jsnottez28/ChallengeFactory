using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web.Data;

namespace Web.Controllers;

[Route("Administration/[controller]")]
public class UtilisateursController(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager) : Controller
{
    [HttpGet("{userId}/Roles")]
    [Authorize(Policy = "Droit:UTILISATEUR.CONSULTER")]
    public async Task<IActionResult> Roles(string userId)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return NotFound();
        }

        var toutesLesRoles = await roleManager.Roles
            .OrderBy(role => role.Name)
            .ToListAsync();

        var rolesActuelles = await userManager.GetRolesAsync(user);

        var displayName = string.Join(" ", new[] { user.Prenom, user.Nom }.Where(value => !string.IsNullOrWhiteSpace(value)));

        var model = new UserRolesViewModel
        {
            UserId = user.Id,
            DisplayName = string.IsNullOrWhiteSpace(displayName) ? (user.Email ?? user.UserName ?? "Utilisateur") : displayName,
            Email = user.Email ?? "-",
            Roles = toutesLesRoles.Select(role => new RoleCheckboxViewModel
            {
                RoleId = role.Id,
                RoleName = role.Name ?? role.Id,
                EstCoche = rolesActuelles.Contains(role.Name ?? role.Id),
            }).ToList(),
        };

        return View(model);
    }

    [HttpPost("{userId}/Roles")]
    [Authorize(Policy = "Droit:UTILISATEUR.MODIFIER")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Roles(string userId, [FromForm] List<string>? roleNames)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return NotFound();
        }

        var rolesSouhaitees = roleNames ?? [];
        var rolesActuelles = await userManager.GetRolesAsync(user);

        var rolesAAjouter = rolesSouhaitees.Except(rolesActuelles).ToList();
        var rolesARetirer = rolesActuelles.Except(rolesSouhaitees).ToList();

        if (rolesAAjouter.Count > 0)
        {
            await userManager.AddToRolesAsync(user, rolesAAjouter);
        }

        if (rolesARetirer.Count > 0)
        {
            await userManager.RemoveFromRolesAsync(user, rolesARetirer);
        }

        TempData["StatusMessage"] = "Roles de l'utilisateur mis a jour.";
        return RedirectToAction(nameof(Roles), new { userId });
    }

    public sealed class UserRolesViewModel
    {
        public string UserId { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public List<RoleCheckboxViewModel> Roles { get; set; } = [];
    }

    public sealed class RoleCheckboxViewModel
    {
        public string RoleId { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
        public bool EstCoche { get; set; }
    }
}

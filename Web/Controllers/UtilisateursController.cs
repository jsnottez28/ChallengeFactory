using Application.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web.Data;

namespace Web.Controllers;

[Route("Administration/[controller]")]
public class UtilisateursController(
    UserManager<ApplicationUser> userManager,
    RoleManager<ApplicationRole> roleManager,
    ICarteCompetenceService carteCompetenceService) : Controller
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

    [HttpGet("{userId}/Cartes")]
    [Authorize(Policy = "Droit:CARTE.CONSULTER")]
    public async Task<IActionResult> Cartes(string userId)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return NotFound();
        }

        var displayName = string.Join(" ", new[] { user.Prenom, user.Nom }.Where(value => !string.IsNullOrWhiteSpace(value)));

        var toutesLesCartes = (await carteCompetenceService.RechercherAsync(new CarteCompetenceFiltre { TaillePage = int.MaxValue })).Cartes;
        var attributions = await carteCompetenceService.GetAttributionsPourUtilisateurAsync(userId);
        var cartesDejaAttribuees = attributions.Where(a => a.EstActif).Select(a => a.CarteCompetenceId).ToHashSet();

        var model = new UserCartesViewModel
        {
            UserId = user.Id,
            DisplayName = string.IsNullOrWhiteSpace(displayName) ? (user.Email ?? user.UserName ?? "Utilisateur") : displayName,
            Cartes = toutesLesCartes.Select(c => new CarteCheckboxViewModel
            {
                CarteId = c.Id,
                Code = c.Code,
                TitreTheorie = c.TitreTheorie,
                EstCoche = cartesDejaAttribuees.Contains(c.Id),
            }).ToList(),
            Attributions = attributions.Where(a => a.EstActif).OrderByDescending(a => a.AttribueLe).ToList(),
        };

        return View(model);
    }

    [HttpPost("{userId}/Cartes/Attribuer")]
    [Authorize(Policy = "Droit:CARTE.MODIFIER")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AttribuerCartes(string userId, [FromForm] List<int>? carteIds, string? contexte)
    {
        var attribuePar = userManager.GetUserId(User);
        if (attribuePar is null)
        {
            return Forbid();
        }

        var (success, errorMessage) = await carteCompetenceService.AttribuerAsync(
            carteIds ?? [], [userId], attribuePar, contexte);

        TempData["StatusMessage"] = success ? "Attribution enregistrée." : errorMessage;
        return RedirectToAction(nameof(Cartes), new { userId });
    }

    [HttpPost("{userId}/Cartes/Desattribuer/{attributionId:int}")]
    [Authorize(Policy = "Droit:CARTE.MODIFIER")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DesattribuerCarte(string userId, int attributionId)
    {
        var (success, errorMessage) = await carteCompetenceService.DesattribuerAsync(attributionId);
        TempData["StatusMessage"] = success ? "Attribution retirée." : errorMessage;
        return RedirectToAction(nameof(Cartes), new { userId });
    }

    public sealed class UserCartesViewModel
    {
        public string UserId { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public List<CarteCheckboxViewModel> Cartes { get; set; } = [];
        public List<CarteAttributionInfo> Attributions { get; set; } = [];
    }

    public sealed class CarteCheckboxViewModel
    {
        public int CarteId { get; set; }
        public string Code { get; set; } = string.Empty;
        public string TitreTheorie { get; set; } = string.Empty;
        public bool EstCoche { get; set; }
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

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Web.Data;

namespace Web.Pages.Admin;

[Authorize]
public class UtilisateursModel(
    UserManager<ApplicationUser> userManager,
    RoleManager<ApplicationRole> roleManager,
    IAuthorizationService authorizationService) : PageModel
{
    public List<UserRow> Users { get; private set; } = [];
    public List<string> Roles { get; private set; } = [];

    [TempData]
    public string? StatusMessage { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var authResult = await authorizationService.AuthorizeAsync(User, "Droit:UTILISATEUR.CONSULTER");
        if (!authResult.Succeeded)
        {
            return Forbid();
        }

        await LoadAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostUpdateRoleAsync(string userId, string? roleName)
    {
        var authResult = await authorizationService.AuthorizeAsync(User, "Droit:UTILISATEUR.MODIFIER");
        if (!authResult.Succeeded)
        {
            return Forbid();
        }

        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
        {
            StatusMessage = "Utilisateur introuvable.";
            return RedirectToPage();
        }

        var currentRoles = await userManager.GetRolesAsync(user);
        if (currentRoles.Count > 0)
        {
            await userManager.RemoveFromRolesAsync(user, currentRoles);
        }

        if (!string.IsNullOrWhiteSpace(roleName))
        {
            var roleExists = await roleManager.RoleExistsAsync(roleName);
            if (!roleExists)
            {
                StatusMessage = "Rôle introuvable.";
                return RedirectToPage();
            }

            await userManager.AddToRoleAsync(user, roleName);
        }

        StatusMessage = "Rôle utilisateur mis à jour.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostUpdateStatutAsync(string userId, StatutUtilisateur statut)
    {
        var authResult = await authorizationService.AuthorizeAsync(User, "Droit:UTILISATEUR.MODIFIER");
        if (!authResult.Succeeded)
        {
            return Forbid();
        }

        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
        {
            StatusMessage = "Utilisateur introuvable.";
            return RedirectToPage();
        }

        user.Statut = statut;
        await userManager.UpdateAsync(user);

        StatusMessage = statut == StatutUtilisateur.Actif
            ? "Compte validé : l'utilisateur peut désormais se connecter."
            : "Statut du compte mis à jour.";
        return RedirectToPage();
    }

    private async Task LoadAsync()
    {
        Roles = await roleManager.Roles
            .OrderBy(role => role.Name)
            .Select(role => role.Name!)
            .ToListAsync();

        var users = await userManager.Users
            .OrderBy(user => user.Statut == StatutUtilisateur.Modere ? 0 : 1)
            .ThenBy(user => user.Nom)
            .ThenBy(user => user.Prenom)
            .ThenBy(user => user.Email)
            .ToListAsync();

        Users = [];
        foreach (var user in users)
        {
            var fullName = string.Join(" ", new[] { user.Prenom, user.Nom }.Where(value => !string.IsNullOrWhiteSpace(value)));
            var displayName = !string.IsNullOrWhiteSpace(fullName) ? fullName : user.Email ?? user.UserName ?? "Utilisateur";
            var roles = await userManager.GetRolesAsync(user);

            Users.Add(new UserRow(
                user.Id,
                displayName,
                user.Email ?? "-",
                roles.FirstOrDefault(),
                user.Statut,
                user.EmailConfirmed));
        }
    }

    public sealed record UserRow(string Id, string DisplayName, string Email, string? RoleName, StatutUtilisateur Statut, bool EmailConfirmed);
}

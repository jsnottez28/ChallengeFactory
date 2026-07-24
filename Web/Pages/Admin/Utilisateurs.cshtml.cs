using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Web.Data;

namespace Web.Pages.Admin;

public class UtilisateursModel(
    UserManager<ApplicationUser> userManager,
    RoleManager<ApplicationRole> roleManager) : PageModel
{
    public List<UserRow> Users { get; private set; } = [];
    public List<string> Roles { get; private set; } = [];

    [TempData]
    public string? StatusMessage { get; set; }

    public async Task OnGetAsync()
    {
        await LoadAsync();
    }

    public async Task<IActionResult> OnPostUpdateRoleAsync(string userId, string? roleName)
    {
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

    private async Task LoadAsync()
    {
        Roles = await roleManager.Roles
            .OrderBy(role => role.Name)
            .Select(role => role.Name!)
            .ToListAsync();

        var users = await userManager.Users
            .OrderBy(user => user.Nom)
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
                roles.FirstOrDefault()));
        }
    }

    public sealed record UserRow(string Id, string DisplayName, string Email, string? RoleName);
}

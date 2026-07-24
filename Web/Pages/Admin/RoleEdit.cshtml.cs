using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Web.Pages.Admin;

public class RoleEditModel : PageModel
{
    private readonly RoleManager<IdentityRole> _roleManager;

    public RoleEditModel(RoleManager<IdentityRole> roleManager)
    {
        _roleManager = roleManager;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    [TempData]
    public string? StatusMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(string? id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            StatusMessage = "Rôle introuvable.";
            return RedirectToPage("/Admin/Roles");
        }

        var role = await _roleManager.FindByIdAsync(id);
        if (role is null)
        {
            StatusMessage = "Rôle introuvable.";
            return RedirectToPage("/Admin/Roles");
        }

        Input = new InputModel
        {
            Id = role.Id,
            Name = role.Name ?? string.Empty,
            NormalizedName = role.NormalizedName ?? string.Empty,
            ConcurrencyStamp = role.ConcurrencyStamp
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var role = await _roleManager.FindByIdAsync(Input.Id);
        if (role is null)
        {
            StatusMessage = "Rôle introuvable.";
            return RedirectToPage("/Admin/Roles");
        }

        var roleName = Input.Name.Trim();
        if (string.IsNullOrWhiteSpace(roleName))
        {
            ModelState.AddModelError($"{nameof(Input)}.{nameof(InputModel.Name)}", "Le nom du rôle est obligatoire.");
            return Page();
        }

        var normalizedRoleName = _roleManager.NormalizeKey(roleName);
        var duplicateExists = await _roleManager.Roles
            .AnyAsync(existingRole => existingRole.Id != role.Id && existingRole.NormalizedName == normalizedRoleName);
        if (duplicateExists)
        {
            ModelState.AddModelError($"{nameof(Input)}.{nameof(InputModel.Name)}", "Un autre rôle avec ce nom existe déjà.");
            return Page();
        }

        role.Name = roleName;
        role.NormalizedName = normalizedRoleName;

        var result = await _roleManager.UpdateAsync(role);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return Page();
        }

        StatusMessage = $"Le rôle \"{roleName}\" a été modifié.";
        return RedirectToPage("/Admin/Roles");
    }

    public sealed class InputModel
    {
        [Required]
        public string Id { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le nom du rôle est obligatoire.")]
        [StringLength(256, ErrorMessage = "Le nom du rôle ne peut pas dépasser 256 caractères.")]
        [Display(Name = "Nom")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Nom normalisé")]
        public string? NormalizedName { get; set; }

        [Display(Name = "Concurrency stamp")]
        public string? ConcurrencyStamp { get; set; }
    }
}

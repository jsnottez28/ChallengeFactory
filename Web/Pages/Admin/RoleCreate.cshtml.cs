using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Web.Pages.Admin;

public class RoleCreateModel : PageModel
{
    private readonly RoleManager<IdentityRole> _roleManager;

    public RoleCreateModel(RoleManager<IdentityRole> roleManager)
    {
        _roleManager = roleManager;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    [TempData]
    public string? StatusMessage { get; set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var roleName = Input.Name.Trim();
        if (string.IsNullOrWhiteSpace(roleName))
        {
            ModelState.AddModelError($"{nameof(Input)}.{nameof(InputModel.Name)}", "Le nom du rôle est obligatoire.");
            return Page();
        }

        if (await _roleManager.RoleExistsAsync(roleName))
        {
            ModelState.AddModelError($"{nameof(Input)}.{nameof(InputModel.Name)}", "Un rôle avec ce nom existe déjà.");
            return Page();
        }

        var result = await _roleManager.CreateAsync(new IdentityRole(roleName));
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return Page();
        }

        StatusMessage = $"Le rôle \"{roleName}\" a été créé.";
        return RedirectToPage("/Admin/Roles");
    }

    public sealed class InputModel
    {
        [Required(ErrorMessage = "Le nom du rôle est obligatoire.")]
        [StringLength(256, ErrorMessage = "Le nom du rôle ne peut pas dépasser 256 caractères.")]
        [Display(Name = "Nom")]
        public string Name { get; set; } = string.Empty;
    }
}

using System.Security.Claims;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Web.Pages.Admin;

public class DroitCreateModel : PageModel
{
    private readonly RoleManager<IdentityRole> _roleManager;

    public DroitCreateModel(RoleManager<IdentityRole> roleManager)
    {
        _roleManager = roleManager;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new()
    {
        ClaimType = "permission"
    };

    public List<SelectListItem> Roles { get; private set; } = [];

    public IReadOnlyList<OptionItem> Resources { get; } =
    [
        new("global-admin", "Administration Globale"),
        new("congress", "Congrès"),
        new("organization", "Organisation"),
        new("user-request", "Demande utilisateur"),
        new("user", "Utilisateur")
    ];

    public IReadOnlyList<OptionItem> Actions { get; } =
    [
        new("create", "Créer"),
        new("read", "Lire"),
        new("update", "Modifier"),
        new("delete", "Supprimer")
    ];

    [TempData]
    public string? StatusMessage { get; set; }

    public async Task OnGetAsync()
    {
        await LoadRolesAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await LoadRolesAsync();

        if (string.IsNullOrWhiteSpace(Input.ClaimValue) &&
            !string.IsNullOrWhiteSpace(Input.Resource) &&
            !string.IsNullOrWhiteSpace(Input.Action))
        {
            Input.ClaimValue = $"{Input.Resource}.{Input.Action}";
            ModelState.Remove($"{nameof(Input)}.{nameof(InputModel.ClaimValue)}");
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var role = await _roleManager.FindByIdAsync(Input.RoleId);
        if (role is null)
        {
            ModelState.AddModelError($"{nameof(Input)}.{nameof(InputModel.RoleId)}", "Le rôle sélectionné est introuvable.");
            return Page();
        }

        var claimType = Input.ClaimType.Trim();
        var claimValue = Input.ClaimValue.Trim();
        if (string.IsNullOrWhiteSpace(claimType) || string.IsNullOrWhiteSpace(claimValue))
        {
            ModelState.AddModelError(string.Empty, "Le type et la valeur de claim sont obligatoires.");
            return Page();
        }

        var existingClaims = await _roleManager.GetClaimsAsync(role);
        if (existingClaims.Any(claim => claim.Type == claimType && claim.Value == claimValue))
        {
            ModelState.AddModelError(string.Empty, "Ce droit existe déjà pour ce rôle.");
            return Page();
        }

        var result = await _roleManager.AddClaimAsync(role, new Claim(claimType, claimValue));
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return Page();
        }

        StatusMessage = $"Le droit \"{claimType} / {claimValue}\" a été ajouté au rôle \"{role.Name}\".";
        return RedirectToPage("/Admin/Droits");
    }

    private async Task LoadRolesAsync()
    {
        Roles = await _roleManager.Roles
            .OrderBy(role => role.Name)
            .Select(role => new SelectListItem
            {
                Value = role.Id,
                Text = role.Name ?? role.NormalizedName ?? role.Id
            })
            .ToListAsync();
    }

    public sealed class InputModel
    {
        [Required(ErrorMessage = "Le rôle est obligatoire.")]
        [Display(Name = "Rôle")]
        public string RoleId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le type de claim est obligatoire.")]
        [Display(Name = "Type")]
        public string ClaimType { get; set; } = "permission";

        [Display(Name = "Ressource")]
        public string Resource { get; set; } = string.Empty;

        [Display(Name = "Action")]
        public string Action { get; set; } = string.Empty;

        [Required(ErrorMessage = "La valeur de claim est obligatoire.")]
        [Display(Name = "Valeur")]
        public string ClaimValue { get; set; } = string.Empty;
    }

    public sealed record OptionItem(string Code, string Label);
}

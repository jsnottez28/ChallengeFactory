using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Web.Pages.Admin;

public class DroitEditModel : PageModel
{
    private readonly ApplicationDbContext _dbContext;
    private readonly RoleManager<IdentityRole> _roleManager;

    public DroitEditModel(ApplicationDbContext dbContext, RoleManager<IdentityRole> roleManager)
    {
        _dbContext = dbContext;
        _roleManager = roleManager;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

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

    public async Task<IActionResult> OnGetAsync(int id)
    {
        await LoadRolesAsync();

        var claim = await _dbContext.Set<IdentityRoleClaim<string>>()
            .FirstOrDefaultAsync(roleClaim => roleClaim.Id == id);
        if (claim is null)
        {
            StatusMessage = "Droit introuvable.";
            return RedirectToPage("/Admin/Droits");
        }

        var claimValue = claim.ClaimValue ?? string.Empty;
        var (resource, action) = ParseClaimValue(claimValue);

        Input = new InputModel
        {
            Id = claim.Id,
            RoleId = claim.RoleId,
            ClaimType = claim.ClaimType ?? string.Empty,
            ClaimValue = claimValue,
            Resource = resource,
            Action = action
        };

        return Page();
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

        var claim = await _dbContext.Set<IdentityRoleClaim<string>>()
            .FirstOrDefaultAsync(roleClaim => roleClaim.Id == Input.Id);
        if (claim is null)
        {
            StatusMessage = "Droit introuvable.";
            return RedirectToPage("/Admin/Droits");
        }

        var roleExists = await _roleManager.Roles.AnyAsync(role => role.Id == Input.RoleId);
        if (!roleExists)
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

        var duplicateExists = await _dbContext.Set<IdentityRoleClaim<string>>()
            .AnyAsync(roleClaim =>
                roleClaim.Id != Input.Id &&
                roleClaim.RoleId == Input.RoleId &&
                roleClaim.ClaimType == claimType &&
                roleClaim.ClaimValue == claimValue);
        if (duplicateExists)
        {
            ModelState.AddModelError(string.Empty, "Ce droit existe déjà pour ce rôle.");
            return Page();
        }

        claim.RoleId = Input.RoleId;
        claim.ClaimType = claimType;
        claim.ClaimValue = claimValue;

        await _dbContext.SaveChangesAsync();

        StatusMessage = $"Le droit \"{claimType} / {claimValue}\" a été modifié.";
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

    private (string Resource, string Action) ParseClaimValue(string claimValue)
    {
        if (string.IsNullOrWhiteSpace(claimValue))
        {
            return (string.Empty, string.Empty);
        }

        foreach (var resource in Resources)
        {
            foreach (var action in Actions)
            {
                if (string.Equals(claimValue, $"{resource.Code}.{action.Code}", StringComparison.OrdinalIgnoreCase))
                {
                    return (resource.Code, action.Code);
                }
            }
        }

        return (string.Empty, string.Empty);
    }

    public sealed class InputModel
    {
        [Required]
        public int Id { get; set; }

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

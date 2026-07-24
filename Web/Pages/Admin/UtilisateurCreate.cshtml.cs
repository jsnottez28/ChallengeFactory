using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Web.Data;

namespace Web.Pages.Admin;

public class UtilisateurCreateModel(RoleManager<ApplicationRole> roleManager) : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; } = new();

    [BindProperty]
    public int Step { get; set; } = 1;

    public List<string> Roles { get; private set; } = [];

    [TempData]
    public string? StatusMessage { get; set; }

    public async Task OnGetAsync()
    {
        Step = 1;
        await LoadRolesAsync();
    }

    public async Task<IActionResult> OnPostAsync(string submitAction)
    {
        await LoadRolesAsync();
        Step = Step is >= 1 and <= 3 ? Step : 1;

        if (submitAction == "previous")
        {
            Step = Step > 1 ? Step - 1 : 1;
            ModelState.Clear();
            return Page();
        }

        if (submitAction == "next")
        {
            ApplyStepValidationScope();

            if (!ModelState.IsValid)
            {
                return Page();
            }

            Step = Step switch
            {
                1 => 2,
                2 => 3,
                _ => 3
            };
            ModelState.Clear();
            return Page();
        }

        if (!ModelState.IsValid)
        {
            Step = 3;
            return Page();
        }

        // Template volontairement non persistant.
        // A brancher ensuite avec UserManager<ApplicationUser>.CreateAsync(...)
        // puis UserManager<ApplicationUser>.AddToRoleAsync(...) si un role est choisi.
        StatusMessage = "Template valide : la creation utilisateur pourra etre branchee ici.";
        Step = 3;
        return Page();
    }

    private async Task LoadRolesAsync()
    {
        Roles = await roleManager.Roles
            .OrderBy(role => role.Name)
            .Select(role => role.Name!)
            .ToListAsync();
    }

    private void ApplyStepValidationScope()
    {
        if (Step == 1)
        {
            RemoveModelStateEntries(
                nameof(Input.Prenom),
                nameof(Input.Nom),
                nameof(Input.Structure),
                nameof(Input.CodeAdherent));
            return;
        }

        if (Step == 2)
        {
            RemoveModelStateEntries(
                nameof(Input.UserName),
                nameof(Input.Email),
                nameof(Input.PhoneNumber),
                nameof(Input.Password),
                nameof(Input.RoleName),
                nameof(Input.EmailConfirmed),
                nameof(Input.LockoutEnabled),
                nameof(Input.SendInvitation));
        }
    }

    private void RemoveModelStateEntries(params string[] propertyNames)
    {
        foreach (var propertyName in propertyNames)
        {
            ModelState.Remove($"{nameof(Input)}.{propertyName}");
        }
    }

    public sealed class InputModel
    {
        [Display(Name = "Nom utilisateur")]
        public string? UserName { get; set; }

        [Display(Name = "Prenom")]
        public string? Prenom { get; set; }

        [Display(Name = "Nom")]
        public string? Nom { get; set; }

        [Required(ErrorMessage = "L'email est obligatoire.")]
        [EmailAddress(ErrorMessage = "L'email n'est pas valide.")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Role")]
        public string? RoleName { get; set; }

        [Phone(ErrorMessage = "Le telephone n'est pas valide.")]
        [Display(Name = "Telephone")]
        public string? PhoneNumber { get; set; }

        [Display(Name = "Email confirme")]
        public bool EmailConfirmed { get; set; }

        [Display(Name = "Verrouillage autorise")]
        public bool LockoutEnabled { get; set; } = true;

        [Display(Name = "Structure")]
        public string? Structure { get; set; }

        [Display(Name = "Code adherent")]
        public string? CodeAdherent { get; set; }


        [DataType(DataType.Password)]
        [Display(Name = "Mot de passe temporaire")]
        public string? Password { get; set; }

        [Display(Name = "Envoyer une invitation par email")]
        public bool SendInvitation { get; set; } = true;
    }
}

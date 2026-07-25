using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Web.Data;

namespace Web.Pages.Compte;

// Page publique (pas de [Authorize]) : le lien d'invitation par email doit fonctionner
// pour un utilisateur qui n'a jamais pu se connecter (compte cree via import Cohorte,
// sans mot de passe utilisable - voir ICohorteService.ImporterMembresAsync).
public class DefinirMotDePasseModel(UserManager<ApplicationUser> userManager, ApplicationDbContext dbContext) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public string? Token { get; set; }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public bool TokenValide { get; private set; }
    public bool MotDePasseDefini { get; private set; }

    public async Task OnGetAsync()
    {
        TokenValide = await TrouverInvitationValideAsync() is not null;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var invitation = await TrouverInvitationValideAsync();
        TokenValide = invitation is not null;

        if (invitation is null)
        {
            return Page();
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var utilisateur = await userManager.FindByIdAsync(invitation.UtilisateurId);
        if (utilisateur is null)
        {
            ModelState.AddModelError(string.Empty, "Compte introuvable.");
            return Page();
        }

        var resultat = await userManager.AddPasswordAsync(utilisateur, Input.MotDePasse);
        if (!resultat.Succeeded)
        {
            foreach (var erreur in resultat.Errors)
            {
                ModelState.AddModelError(string.Empty, erreur.Description);
            }

            return Page();
        }

        invitation.UtiliseLe = DateTime.UtcNow;
        invitation.EstActif = false;
        await dbContext.SaveChangesAsync();

        MotDePasseDefini = true;
        return Page();
    }

    private async Task<InvitationCompte?> TrouverInvitationValideAsync()
    {
        if (string.IsNullOrWhiteSpace(Token))
        {
            return null;
        }

        return await dbContext.InvitationsComptes.FirstOrDefaultAsync(i =>
            i.Token == Token && i.EstActif && i.UtiliseLe == null && i.ExpireLe > DateTime.UtcNow);
    }

    public sealed class InputModel
    {
        [Required(ErrorMessage = "Le mot de passe est obligatoire.")]
        [DataType(DataType.Password)]
        [Display(Name = "Mot de passe")]
        public string MotDePasse { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Confirmer le mot de passe")]
        [Compare(nameof(MotDePasse), ErrorMessage = "Les mots de passe ne correspondent pas.")]
        public string ConfirmerMotDePasse { get; set; } = string.Empty;
    }
}

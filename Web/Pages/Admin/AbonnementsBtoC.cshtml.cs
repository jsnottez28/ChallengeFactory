using Application.Common.Interfaces;
using Domain.Entities;
using Infrastructure.ExternalServices.Email;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Web.Data;

namespace Web.Pages.Admin;

// Gestion des abonnements BtoC : statut_acces_plateforme (StatutUtilisateur, reutilise
// tel quel) au niveau du compte, independamment de ses inscriptions a telle ou telle
// Cohorte - voir ICohorteService et CLAUDE.md, "Point d'implementation transverse".
[Authorize]
public class AbonnementsBtoCModel(UserManager<ApplicationUser> userManager, IAuthorizationService authorizationService, IEmailService emailService) : PageModel
{
    public List<UserRow> Users { get; private set; } = [];

    [BindProperty(SupportsGet = true)]
    public string? UtilisateurId { get; set; }

    [TempData]
    public string? StatusMessage { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var authResult = await authorizationService.AuthorizeAsync(User, "Droit:COHORTE.MODIFIER");
        if (!authResult.Succeeded)
        {
            return Forbid();
        }

        await LoadAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostValiderPaiementAsync(string userId)
    {
        var authResult = await authorizationService.AuthorizeAsync(User, "Droit:COHORTE.MODIFIER");
        if (!authResult.Succeeded)
        {
            return Forbid();
        }

        var utilisateur = await userManager.FindByIdAsync(userId);
        if (utilisateur is null)
        {
            StatusMessage = "Utilisateur introuvable.";
            return RedirectToPage();
        }

        var statutPrecedent = utilisateur.Statut;
        utilisateur.Statut = StatutUtilisateur.Actif;
        await userManager.UpdateAsync(utilisateur);

        if (statutPrecedent != StatutUtilisateur.Actif && !string.IsNullOrWhiteSpace(utilisateur.Email))
        {
            var lienConnexion = Url.Page("/Account/Login", pageHandler: null, values: new { area = "Identity" }, protocol: Request.Scheme)
                ?? "/Identity/Account/Login";
            var nomUtilisateur = utilisateur.Prenom ?? utilisateur.Email;
            var corpsEmail = EmailTemplates.AccesValide(nomUtilisateur, lienConnexion);
            await emailService.EnvoyerAsync(utilisateur.Email, "Votre accès Challenges Factory est activé", corpsEmail);
        }

        StatusMessage = "Paiement validé : l'accès est désormais actif pour tous les Challenges BtoC en cours de cet utilisateur.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostSuspendreAccesAsync(string userId)
    {
        var authResult = await authorizationService.AuthorizeAsync(User, "Droit:COHORTE.MODIFIER");
        if (!authResult.Succeeded)
        {
            return Forbid();
        }

        var utilisateur = await userManager.FindByIdAsync(userId);
        if (utilisateur is null)
        {
            StatusMessage = "Utilisateur introuvable.";
            return RedirectToPage();
        }

        var statutPrecedent = utilisateur.Statut;
        utilisateur.Statut = StatutUtilisateur.Inactif;
        await userManager.UpdateAsync(utilisateur);

        if (statutPrecedent != StatutUtilisateur.Inactif && !string.IsNullOrWhiteSpace(utilisateur.Email))
        {
            var nomUtilisateur = utilisateur.Prenom ?? utilisateur.Email;
            var corpsEmail = EmailTemplates.AccesSuspendu(nomUtilisateur);
            await emailService.EnvoyerAsync(utilisateur.Email, "Votre accès Challenges Factory a été suspendu", corpsEmail);
        }

        StatusMessage = "Accès suspendu pour tous les Challenges BtoC en cours de cet utilisateur.";
        return RedirectToPage();
    }

    private async Task LoadAsync()
    {
        var utilisateurs = await userManager.Users
            .Where(u => u.Mode == ModePlateforme.BtoC)
            .OrderBy(u => u.Statut == StatutUtilisateur.Modere ? 0 : 1)
            .ThenBy(u => u.Nom)
            .ThenBy(u => u.Prenom)
            .ToListAsync();

        Users = utilisateurs.Select(u =>
        {
            var nomComplet = string.Join(" ", new[] { u.Prenom, u.Nom }.Where(v => !string.IsNullOrWhiteSpace(v)));
            return new UserRow(
                u.Id,
                string.IsNullOrWhiteSpace(nomComplet) ? (u.Email ?? u.UserName ?? "Utilisateur") : nomComplet,
                u.Email ?? "-",
                u.Statut);
        }).ToList();
    }

    public sealed record UserRow(string Id, string DisplayName, string Email, StatutUtilisateur Statut);
}

using Domain.Entities;
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
public class AbonnementsBtoCModel(UserManager<ApplicationUser> userManager, IAuthorizationService authorizationService) : PageModel
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

    public Task<IActionResult> OnPostValiderPaiementAsync(string userId)
        => MettreAJourStatutAsync(userId, StatutUtilisateur.Actif, "Paiement validé : l'accès est désormais actif pour tous les Challenges BtoC en cours de cet utilisateur.");

    public Task<IActionResult> OnPostSuspendreAccesAsync(string userId)
        => MettreAJourStatutAsync(userId, StatutUtilisateur.Inactif, "Accès suspendu pour tous les Challenges BtoC en cours de cet utilisateur.");

    private async Task<IActionResult> MettreAJourStatutAsync(string userId, StatutUtilisateur statut, string message)
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

        utilisateur.Statut = statut;
        await userManager.UpdateAsync(utilisateur);

        StatusMessage = message;
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

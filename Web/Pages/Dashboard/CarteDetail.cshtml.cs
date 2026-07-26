using Application.Common.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Web.Data;

namespace Web.Pages.Dashboard;

[Authorize]
public class CarteDetailModel(ICarteApprenantService carteApprenantService, UserManager<ApplicationUser> userManager) : PageModel
{
    public CarteCompetence? Carte { get; private set; }

    // Strictement les notes de l'utilisateur courant sur CETTE carte - jamais celles d'un
    // autre utilisateur, meme un Gestionnaire (cf. prompt "Notes personnelles sur les
    // cartes", section 2.1 : commentaires strictement prives).
    public List<CommentaireCarteInfo> MesCommentaires { get; private set; } = [];

    [BindProperty]
    public string NouveauCommentaire { get; set; } = string.Empty;

    [TempData]
    public string? StatusMessage { get; set; }

    // Renvoie NotFound (jamais la carte) si elle n'est pas attribuee a l'utilisateur
    // courant : c'est le point d'application du controle d'acces serveur, empeche tout
    // acces par Id devine/force dans l'URL.
    public async Task<IActionResult> OnGetAsync(int id)
    {
        var userId = userManager.GetUserId(User);
        if (userId is null)
        {
            return Forbid();
        }

        Carte = await carteApprenantService.GetCarteAttribueeAsync(userId, id);
        if (Carte is null)
        {
            return NotFound();
        }

        MesCommentaires = await carteApprenantService.GetMesCommentairesAsync(userId, id);
        return Page();
    }

    public async Task<IActionResult> OnPostAjouterCommentaireAsync(int id)
    {
        var userId = userManager.GetUserId(User);
        if (userId is null)
        {
            return Forbid();
        }

        var (success, errorMessage, _) = await carteApprenantService.AjouterCommentaireAsync(userId, id, NouveauCommentaire);
        StatusMessage = success ? null : errorMessage;
        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostModifierCommentaireAsync(int id, int commentaireId, string contenu)
    {
        var userId = userManager.GetUserId(User);
        if (userId is null)
        {
            return Forbid();
        }

        var (success, errorMessage) = await carteApprenantService.ModifierCommentaireAsync(commentaireId, userId, contenu);
        StatusMessage = success ? "Note modifiée." : errorMessage;
        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostSupprimerCommentaireAsync(int id, int commentaireId)
    {
        var userId = userManager.GetUserId(User);
        if (userId is null)
        {
            return Forbid();
        }

        var (success, errorMessage) = await carteApprenantService.SupprimerCommentaireAsync(commentaireId, userId);
        StatusMessage = success ? "Note supprimée." : errorMessage;
        return RedirectToPage(new { id });
    }
}

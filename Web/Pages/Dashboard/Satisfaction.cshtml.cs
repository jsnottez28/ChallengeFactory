using Application.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Web.Data;

namespace Web.Pages.Dashboard;

// Formulaire de l'enquete de satisfaction (critere 7 Qualiopi), atteint depuis l'email
// envoye automatiquement a la cloture d'une Cohorte (cf. CohorteService.
// NotifierDemandeSatisfactionAsync).
[Authorize]
public class SatisfactionModel(ISatisfactionService satisfactionService, ICohorteService cohorteService, UserManager<ApplicationUser> userManager) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public int CohorteId { get; set; }

    [BindProperty]
    public int Score { get; set; } = -1;

    [BindProperty]
    public int NoteContenus { get; set; } = -1;

    [BindProperty]
    public int NoteAccompagnement { get; set; } = -1;

    [BindProperty]
    public int NoteAdequationAttentes { get; set; } = -1;

    [BindProperty]
    public string? Commentaire { get; set; }

    public string? ChallengeTitre { get; private set; }

    public bool ADejaRepondu { get; private set; }

    public bool CohorteIntrouvable { get; private set; }

    [TempData]
    public string? StatusMessage { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var userId = userManager.GetUserId(User);
        if (userId is null)
        {
            return Forbid();
        }

        await ChargerAsync(userId);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var userId = userManager.GetUserId(User);
        if (userId is null)
        {
            return Forbid();
        }

        if (Score is < 0 or > 10)
        {
            ModelState.AddModelError(nameof(Score), "Merci de choisir une note entre 0 et 10.");
        }

        if (NoteContenus is < 0 or > 10)
        {
            ModelState.AddModelError(nameof(NoteContenus), "Merci de choisir une note entre 0 et 10.");
        }

        if (NoteAccompagnement is < 0 or > 10)
        {
            ModelState.AddModelError(nameof(NoteAccompagnement), "Merci de choisir une note entre 0 et 10.");
        }

        if (NoteAdequationAttentes is < 0 or > 10)
        {
            ModelState.AddModelError(nameof(NoteAdequationAttentes), "Merci de choisir une note entre 0 et 10.");
        }

        if (!ModelState.IsValid)
        {
            await ChargerAsync(userId);
            return Page();
        }

        var (success, errorMessage) = await satisfactionService.EnregistrerReponseAsync(
            CohorteId, userId, Score, NoteContenus, NoteAccompagnement, NoteAdequationAttentes, Commentaire);
        if (!success)
        {
            ModelState.AddModelError(string.Empty, errorMessage ?? "Impossible d'enregistrer votre réponse.");
            await ChargerAsync(userId);
            return Page();
        }

        StatusMessage = "Merci pour votre retour !";
        return RedirectToPage(new { CohorteId });
    }

    private async Task ChargerAsync(string userId)
    {
        var cohorte = await cohorteService.GetResumeAsync(CohorteId);
        if (cohorte is null)
        {
            CohorteIntrouvable = true;
            return;
        }

        ChallengeTitre = cohorte.ChallengeTitre;
        ADejaRepondu = await satisfactionService.ADejaReponduAsync(CohorteId, userId);
    }
}

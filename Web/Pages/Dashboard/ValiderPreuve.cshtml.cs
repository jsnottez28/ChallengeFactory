using Application.Common.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Web.Data;

namespace Web.Pages.Dashboard;

[Authorize]
public class ValiderPreuveModel(IPreuveService preuveService, UserManager<ApplicationUser> userManager) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public int PreuveId { get; set; }

    [BindProperty(SupportsGet = true)]
    public int CohorteId { get; set; }

    [BindProperty]
    public DecisionValidationPair Decision { get; set; }

    [BindProperty]
    public string? Commentaire { get; set; }

    public PreuveApercuPourPairInfo? Apercu { get; private set; }

    [TempData]
    public string? StatusMessage { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var userId = userManager.GetUserId(User);
        if (userId is null)
        {
            return Forbid();
        }

        Apercu = await preuveService.GetApercuPourPairAsync(PreuveId, userId);
        if (Apercu is null)
        {
            return NotFound();
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var userId = userManager.GetUserId(User);
        if (userId is null)
        {
            return Forbid();
        }

        var apercuAvantVote = await preuveService.GetApercuPourPairAsync(PreuveId, userId);
        if (apercuAvantVote is null)
        {
            return NotFound();
        }

        var lienSuiviPreuve = Url.Page("/Dashboard/MaPreuve", null, new { CohorteId = apercuAvantVote.CohorteId, ChallengeEtapeId = apercuAvantVote.ChallengeEtapeId }, Request.Scheme)
            ?? "/Dashboard/MaPreuve";
        var (success, errorMessage) = await preuveService.ValiderParPairAsync(PreuveId, userId, Decision, Commentaire, lienSuiviPreuve);

        if (!success)
        {
            Apercu = await preuveService.GetApercuPourPairAsync(PreuveId, userId);
            ModelState.AddModelError(string.Empty, errorMessage ?? "Impossible d'enregistrer votre décision.");
            return Page();
        }

        StatusMessage = "Votre décision a été enregistrée. Merci pour votre contribution !";
        return RedirectToPage("/Dashboard/PreuvesAValider", new { CohorteId });
    }
}

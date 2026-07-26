using Application.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Web.Data;

namespace Web.Pages.Dashboard;

[Authorize]
public class MaPreuveModel(IPreuveService preuveService, ICohorteService cohorteService, UserManager<ApplicationUser> userManager) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public int CohorteId { get; set; }

    [BindProperty(SupportsGet = true)]
    public int ChallengeEtapeId { get; set; }

    [BindProperty]
    public string? Description { get; set; }

    [BindProperty]
    public List<IFormFile>? Fichiers { get; set; }

    [BindProperty]
    public List<int>? FichierIdsARetirer { get; set; }

    public PreuveDetailInfo? Preuve { get; private set; }

    public string? TitreEtape { get; private set; }

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

        var fichiersInput = new List<FichierPreuveInput>();
        foreach (var fichier in Fichiers ?? [])
        {
            if (fichier.Length == 0)
            {
                continue;
            }

            fichiersInput.Add(new FichierPreuveInput
            {
                NomFichier = Path.GetFileName(fichier.FileName),
                Contenu = fichier.OpenReadStream(),
                TailleOctets = fichier.Length,
            });
        }

        var (success, errorMessage, _) = await preuveService.DeposerOuModifierAsync(
            userId, CohorteId, ChallengeEtapeId, Description, fichiersInput, FichierIdsARetirer);

        StatusMessage = success ? "Preuve enregistrée." : errorMessage;

        if (!success)
        {
            await ChargerAsync(userId);
            return Page();
        }

        return RedirectToPage(new { CohorteId, ChallengeEtapeId });
    }

    private async Task ChargerAsync(string userId)
    {
        Preuve = await preuveService.GetMaPreuveAsync(userId, ChallengeEtapeId);

        var parcours = await cohorteService.GetMesParcoursEnCoursAsync(userId);
        TitreEtape = parcours.FirstOrDefault(p => p.CohorteId == CohorteId)?.TitreEtape ?? Preuve?.TitreEtape;
    }
}

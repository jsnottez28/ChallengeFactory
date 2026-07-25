using Application.Common.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Web.Data;

namespace Web.Pages.Dashboard;

[Authorize]
public class CartesModel(ICarteApprenantService carteApprenantService, UserManager<ApplicationUser> userManager) : PageModel
{
    public List<CarteBibliothequeInfo> MesCartes { get; private set; } = [];
    public List<string> ChallengesOrigine { get; private set; } = [];

    [BindProperty(SupportsGet = true)]
    public OrigineAttribution? Origine { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? ChallengeTitre { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var userId = userManager.GetUserId(User);
        if (userId is null)
        {
            return Forbid();
        }

        var toutesLesCartes = await carteApprenantService.GetMesCartesAsync(userId);

        ChallengesOrigine = toutesLesCartes
            .Where(c => c.ChallengeTitre is not null)
            .Select(c => c.ChallengeTitre!)
            .Distinct()
            .OrderBy(titre => titre)
            .ToList();

        MesCartes = toutesLesCartes
            .Where(c => Origine is null || c.OrigineType == Origine)
            .Where(c => string.IsNullOrWhiteSpace(ChallengeTitre) || c.ChallengeTitre == ChallengeTitre)
            .ToList();

        return Page();
    }
}

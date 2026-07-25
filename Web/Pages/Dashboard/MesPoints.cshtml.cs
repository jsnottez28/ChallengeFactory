using Application.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Web.Data;

namespace Web.Pages.Dashboard;

// Uniquement mes propres totaux et historique - jamais de comparaison a un autre membre
// sur cette page (cf. CLAUDE.md, "l'equipe avant l'individu", et prompt section 5).
[Authorize]
public class MesPointsModel(IPreuveService preuveService, UserManager<ApplicationUser> userManager) : PageModel
{
    public PointsResumeInfo Resume { get; private set; } = new();
    public List<PointsEvenementInfo> Historique { get; private set; } = [];
    public List<BadgeSocialInfo> Badges { get; private set; } = [];

    public async Task<IActionResult> OnGetAsync()
    {
        var userId = userManager.GetUserId(User);
        if (userId is null)
        {
            return Forbid();
        }

        Resume = await preuveService.GetMesPointsAsync(userId);
        Historique = await preuveService.GetMonHistoriquePointsAsync(userId);
        Badges = await preuveService.GetMesBadgesAsync(userId);

        return Page();
    }
}

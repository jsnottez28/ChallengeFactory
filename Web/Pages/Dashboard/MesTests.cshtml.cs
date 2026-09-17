using Application.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Web.Data;

namespace Web.Pages.Dashboard;

// Consultation seule : le stagiaire ne peut jamais lancer/relancer un test depuis cette
// page, il l'a deja passe via le lien fourni sur le defi individuel d'une etape (ou envoye
// manuellement) - cf. DiscResultat / TestDisc.cshtml. Regroupe les resultats de tous les
// tests psychotechniques passes (aujourd'hui : DISC, RIASEC).
[Authorize]
public class MesTestsModel(IDiscService discService, IRiasecService riasecService, UserManager<ApplicationUser> userManager) : PageModel
{
    public DiscResultatInfo? DiscResultat { get; private set; }

    public RiasecResultatInfo? RiasecResultat { get; private set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var userId = userManager.GetUserId(User);
        if (userId is null)
        {
            return Forbid();
        }

        DiscResultat = await discService.GetDernierResultatAsync(userId);
        RiasecResultat = await riasecService.GetDernierResultatAsync(userId);

        return Page();
    }
}

using Application.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Web.Data;

namespace Web.Pages.Dashboard;

// Jamais de lien statique public vers un fichier de Preuve (cf. IPreuveFichierStockageService) :
// cette action verifie l'acces (auteur, pair membre de la cohorte, ou droit
// PREUVE.VALIDER) avant de streamer le contenu.
[Authorize]
public class PreuveFichierTelechargerModel(
    IPreuveService preuveService,
    UserManager<ApplicationUser> userManager,
    IAuthorizationService authorizationService) : PageModel
{
    public async Task<IActionResult> OnGetAsync(int fichierId)
    {
        var userId = userManager.GetUserId(User);
        if (userId is null)
        {
            return Forbid();
        }

        var aLeDroit = (await authorizationService.AuthorizeAsync(User, "Droit:PREUVE.VALIDER")).Succeeded;

        var resultat = await preuveService.TelechargerFichierAsync(fichierId, userId, aLeDroit);
        if (resultat is null)
        {
            return NotFound();
        }

        return File(resultat.Value.Contenu, "application/octet-stream", resultat.Value.NomFichier);
    }
}

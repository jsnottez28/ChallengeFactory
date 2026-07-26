using Application.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Web.Data;

namespace Web.Pages.Dashboard;

// Jamais de lien statique public vers un fichier de Preuve (cf. IPreuveFichierStockageService) :
// cette action verifie l'acces (auteur, pair membre de la cohorte, ou droit
// PREUVE.CONSULTER/PREUVE.VALIDER) avant de streamer le contenu. Les deux droits donnent
// acces en lecture (correction A.1 : un Gestionnaire en lecture seule, sans VALIDER,
// doit pouvoir consulter les fichiers deposes).
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

        var aLeDroitConsulter = (await authorizationService.AuthorizeAsync(User, "Droit:PREUVE.CONSULTER")).Succeeded;
        var aLeDroitValider = (await authorizationService.AuthorizeAsync(User, "Droit:PREUVE.VALIDER")).Succeeded;

        var resultat = await preuveService.TelechargerFichierAsync(fichierId, userId, aLeDroitConsulter || aLeDroitValider);
        if (resultat is null)
        {
            return NotFound();
        }

        return File(resultat.Value.Contenu, "application/octet-stream", resultat.Value.NomFichier);
    }
}

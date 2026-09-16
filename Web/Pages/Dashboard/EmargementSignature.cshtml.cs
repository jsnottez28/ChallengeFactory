using Application.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Web.Data;

namespace Web.Pages.Dashboard;

// Jamais de lien statique public vers une signature (meme principe que
// PreuveFichierTelecharger) : verifie que l'appelant est soit l'auteur de la signature,
// soit titulaire du droit COHORTE.CONSULTER, avant de streamer le contenu.
[Authorize]
public class EmargementSignatureModel(
    IEmargementService emargementService,
    UserManager<ApplicationUser> userManager,
    IAuthorizationService authorizationService) : PageModel
{
    public async Task<IActionResult> OnGetAsync(int emargementId)
    {
        var utilisateurId = userManager.GetUserId(User);
        if (utilisateurId is null)
        {
            return Forbid();
        }

        var aLeDroitConsulter = (await authorizationService.AuthorizeAsync(User, "Droit:COHORTE.CONSULTER")).Succeeded;

        var resultat = await emargementService.TelechargerSignatureAsync(emargementId, utilisateurId, aLeDroitConsulter);
        if (resultat is null)
        {
            return NotFound();
        }

        return File(resultat.Value.Contenu, "image/png", resultat.Value.NomFichier);
    }
}

using Application.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Web.Data;

namespace Web.Pages.Dashboard;

// Consultable par le beneficiaire lui-meme, ou par un Gestionnaire (droit COHORTE.CONSULTER)
// pour un autre membre via ?utilisateurId=... - utile a l'audit Qualiopi (la structure doit
// pouvoir retrouver/reimprimer l'attestation d'un beneficiaire).
[Authorize]
public class AttestationModel(
    IAttestationService attestationService,
    IAuthorizationService authorizationService,
    UserManager<ApplicationUser> userManager) : PageModel
{
    public AttestationInfo? Info { get; private set; }
    public bool Indisponible { get; private set; }

    public async Task<IActionResult> OnGetAsync(int cohorteId, string? utilisateurId)
    {
        var utilisateurCourantId = userManager.GetUserId(User)!;
        var utilisateurCible = utilisateurCourantId;

        if (!string.IsNullOrWhiteSpace(utilisateurId) && utilisateurId != utilisateurCourantId)
        {
            var aLeDroitConsulter = (await authorizationService.AuthorizeAsync(User, "Droit:COHORTE.CONSULTER")).Succeeded;
            if (!aLeDroitConsulter)
            {
                return Forbid();
            }

            utilisateurCible = utilisateurId;
        }

        Info = await attestationService.GetAttestationAsync(cohorteId, utilisateurCible);
        Indisponible = Info is null;

        return Page();
    }
}

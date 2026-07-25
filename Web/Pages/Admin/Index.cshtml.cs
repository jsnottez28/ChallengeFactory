using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Web.Security;

namespace Web.Pages.Admin;

// Hub de navigation vers les pages d'administration : pas de droit specifique propre,
// mais doit rester inaccessible a un compte sans aucun role (traite comme un simple
// utilisateur, cf. demande explicite) - chaque lien qu'elle contient reste par ailleurs
// deja protege individuellement (droit-visible / [Authorize] sur la page cible).
[Authorize]
public class AdminIndexModel : PageModel
{
    public IActionResult OnGet()
    {
        var estSuperAdministrateur = User.HasClaim(DroitsClaimsTransformation.ClaimType, DroitsClaimsTransformation.ClaimTous);
        var aDesRoles = User.Claims.Any(c => c.Type == ClaimTypes.Role);

        if (!estSuperAdministrateur && !aDesRoles)
        {
            return Forbid();
        }

        return Page();
    }
}

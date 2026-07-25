using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Web.Security;

namespace Web.Pages.Admin;

[Authorize]
public class ParametrageModel : PageModel
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

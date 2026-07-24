using System.Security.Claims;
using Application.Common.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Web.Data;

namespace Web.Security;

/// <summary>
/// Injecte, a chaque requete authentifiee, une claim "permission" par droit effectif de
/// l'utilisateur (union des RoleDroit directs et des Droit inclus via un RoleGroupeDroit),
/// pour que le systeme d'autorisation standard d'ASP.NET Core puisse s'appuyer dessus.
/// </summary>
public sealed class DroitsClaimsTransformation(
    IPermissionService permissionService,
    UserManager<ApplicationUser> userManager,
    RoleManager<ApplicationRole> roleManager,
    IMemoryCache cache) : IClaimsTransformation
{
    public const string ClaimType = "permission";

    /// <summary>Valeur de claim signifiant "tous les droits", posee pour un compte EstSuperAdministrateur.</summary>
    public const string ClaimTous = "*";

    private static readonly TimeSpan DureeCache = TimeSpan.FromMinutes(2);

    public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        if (principal.Identity is not { IsAuthenticated: true })
        {
            return principal;
        }

        // Le pipeline d'authentification peut appeler la transformation plusieurs fois
        // par requete : on evite d'empiler les memes claims a chaque appel.
        if (principal.HasClaim(claim => claim.Type == ClaimType))
        {
            return principal;
        }

        var user = await userManager.GetUserAsync(principal);
        if (user is null)
        {
            return principal;
        }

        if (user.EstSuperAdministrateur)
        {
            var identiteSuperAdmin = new ClaimsIdentity();
            identiteSuperAdmin.AddClaim(new Claim(ClaimType, ClaimTous));
            principal.AddIdentity(identiteSuperAdmin);
            return principal;
        }

        var roleNames = await userManager.GetRolesAsync(user);
        if (roleNames.Count == 0)
        {
            return principal;
        }

        var roleIds = await roleManager.Roles
            .Where(role => role.Name != null && roleNames.Contains(role.Name))
            .Select(role => role.Id)
            .ToListAsync();

        var cleCache = string.Join(",", roleIds.OrderBy(id => id, StringComparer.Ordinal));
        var droitCodes = await cache.GetOrCreateAsync(cleCache, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = DureeCache;
            return await permissionService.GetEffectiveDroitCodesAsync(roleIds);
        });

        if (droitCodes is null || droitCodes.Count == 0)
        {
            return principal;
        }

        var identiteDroits = new ClaimsIdentity();
        foreach (var code in droitCodes)
        {
            identiteDroits.AddClaim(new Claim(ClaimType, code));
        }

        principal.AddIdentity(identiteDroits);

        return principal;
    }
}

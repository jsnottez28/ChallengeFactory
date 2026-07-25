using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Web.Data;

namespace Web.Security;

/// <summary>
/// Ajoute au SignInManager standard une regle metier : un compte auto-inscrit (BtoC)
/// reste bloque tant qu'un administrateur ne l'a pas valide (ApplicationUser.Statut).
/// S'applique uniformement a toutes les voies de connexion (mot de passe, connexion
/// externe, 2FA) puisque Identity consulte CanSignInAsync avant chacune d'elles.
/// </summary>
public class ApplicationSignInManager : SignInManager<ApplicationUser>
{
    public ApplicationSignInManager(
        UserManager<ApplicationUser> userManager,
        Microsoft.AspNetCore.Http.IHttpContextAccessor contextAccessor,
        IUserClaimsPrincipalFactory<ApplicationUser> claimsFactory,
        Microsoft.Extensions.Options.IOptions<IdentityOptions> optionsAccessor,
        Microsoft.Extensions.Logging.ILogger<SignInManager<ApplicationUser>> logger,
        Microsoft.AspNetCore.Authentication.IAuthenticationSchemeProvider schemes,
        IUserConfirmation<ApplicationUser> confirmation)
        : base(userManager, contextAccessor, claimsFactory, optionsAccessor, logger, schemes, confirmation)
    {
    }

    public override async Task<bool> CanSignInAsync(ApplicationUser user)
    {
        if (!await base.CanSignInAsync(user))
        {
            return false;
        }

        // Le compte de secours (voir ApplicationUser.EstSuperAdministrateur) n'est jamais
        // bloque par la validation admin, pour ne pas se retrouver enferme dehors.
        return user.EstSuperAdministrateur || user.Statut == StatutUtilisateur.Actif;
    }
}

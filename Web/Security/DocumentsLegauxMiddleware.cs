using Application.Common.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Web.Data;

namespace Web.Security;

/// <summary>
/// Bloque l'acces a l'application (redirection vers l'ecran de confirmation) pour un compte
/// dont la session a debute apres la publication d'une nouvelle version de CGU/PPD non encore
/// acceptee. Si le compte etait deja connecte au moment de la publication, il n'est pas bloque
/// ici : voir le bandeau affiche par _LayoutBackend.cshtml pour ce cas-la (cf. RG-51).
/// </summary>
public sealed class DocumentsLegauxMiddleware(RequestDelegate next)
{
    private static readonly string[] CheminsExemptes =
    [
        "/Compte/AccepterDocuments",
        "/Identity/Account/Logout",
        "/cgu",
        "/confidentialite",
    ];

    public async Task InvokeAsync(HttpContext context, IDocumentLegalService documentLegalService, UserManager<ApplicationUser> userManager)
    {
        if (context.User.Identity is not { IsAuthenticated: true })
        {
            await next(context);
            return;
        }

        var chemin = context.Request.Path.Value ?? string.Empty;
        if (CheminsExemptes.Any(exempt => chemin.StartsWith(exempt, StringComparison.OrdinalIgnoreCase)))
        {
            await next(context);
            return;
        }

        var userId = userManager.GetUserId(context.User);
        if (userId is null)
        {
            await next(context);
            return;
        }

        var authenticateResult = await context.AuthenticateAsync(IdentityConstants.ApplicationScheme);
        var sessionDebutUtc = authenticateResult.Properties?.IssuedUtc ?? DateTimeOffset.UtcNow;

        var typesBloquants = await documentLegalService.GetTypesBloquantsAsync(userId, sessionDebutUtc);
        if (typesBloquants.Count > 0)
        {
            var returnUrl = Uri.EscapeDataString(chemin + context.Request.QueryString);
            context.Response.Redirect($"/Compte/AccepterDocuments?returnUrl={returnUrl}");
            return;
        }

        await next(context);
    }
}

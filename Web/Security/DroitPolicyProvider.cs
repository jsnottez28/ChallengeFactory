using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace Web.Security;

/// <summary>
/// Genere a la volee une policy d'autorisation pour tout nom de policy prefixe par "Droit:",
/// ex: [Authorize(Policy = "Droit:ORGANISATION.CREER")]. Evite d'avoir a enregistrer une
/// policy par droit au demarrage : le code du droit est extrait du nom de la policy.
/// </summary>
public sealed class DroitPolicyProvider(IOptions<AuthorizationOptions> options) : IAuthorizationPolicyProvider
{
    public const string Prefix = "Droit:";

    private readonly DefaultAuthorizationPolicyProvider _repliDefaut = new(options);

    public Task<AuthorizationPolicy> GetDefaultPolicyAsync() => _repliDefaut.GetDefaultPolicyAsync();

    public Task<AuthorizationPolicy?> GetFallbackPolicyAsync() => _repliDefaut.GetFallbackPolicyAsync();

    public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        if (policyName.StartsWith(Prefix, StringComparison.Ordinal))
        {
            var droitCode = policyName[Prefix.Length..];
            var policy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .AddRequirements(new DroitRequirement(droitCode))
                .Build();

            return Task.FromResult<AuthorizationPolicy?>(policy);
        }

        return _repliDefaut.GetPolicyAsync(policyName);
    }
}

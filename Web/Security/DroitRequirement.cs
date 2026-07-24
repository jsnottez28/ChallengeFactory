using Microsoft.AspNetCore.Authorization;

namespace Web.Security;

public sealed class DroitRequirement(string droitCode) : IAuthorizationRequirement
{
    public string DroitCode { get; } = droitCode;
}

public sealed class DroitAuthorizationHandler : AuthorizationHandler<DroitRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, DroitRequirement requirement)
    {
        if (context.User.HasClaim(DroitsClaimsTransformation.ClaimType, DroitsClaimsTransformation.ClaimTous) ||
            context.User.HasClaim(DroitsClaimsTransformation.ClaimType, requirement.DroitCode))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}

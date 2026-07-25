using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Web.Data;

namespace Integration.TestSupport;

internal static class TestUserManagerFactory
{
    // UserManager<ApplicationUser> a un constructeur lourd (validators, hasher, logger...)
    // mais tous les parametres au-dela du store ont un fallback interne si null - seul le
    // store est reellement necessaire pour les methodes utilisees en test (FindByIdAsync,
    // CreateAsync, FindByEmailAsync...).
    public static UserManager<ApplicationUser> Create(ApplicationDbContext dbContext)
    {
        var store = new UserStore<ApplicationUser, ApplicationRole, ApplicationDbContext>(dbContext);

        return new UserManager<ApplicationUser>(
            store,
            optionsAccessor: null,
            passwordHasher: new PasswordHasher<ApplicationUser>(),
            userValidators: [],
            passwordValidators: [],
            keyNormalizer: new UpperInvariantLookupNormalizer(),
            errors: new IdentityErrorDescriber(),
            services: null!,
            logger: new Microsoft.Extensions.Logging.Abstractions.NullLogger<UserManager<ApplicationUser>>());
    }
}

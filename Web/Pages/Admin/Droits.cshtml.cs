using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Web.Pages.Admin;

public class DroitsModel : PageModel
{
    private readonly ApplicationDbContext _dbContext;

    public DroitsModel(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public List<DroitItem> Droits { get; private set; } = [];

    public async Task OnGetAsync()
    {
        Droits = await _dbContext.Set<IdentityRoleClaim<string>>()
            .Join(
                _dbContext.Set<IdentityRole>(),
                claim => claim.RoleId,
                role => role.Id,
                (claim, role) => new DroitItem
                {
                    Id = claim.Id,
                    ClaimType = claim.ClaimType ?? string.Empty,
                    ClaimValue = claim.ClaimValue ?? string.Empty,
                    RoleName = role.Name ?? string.Empty
                })
            .OrderBy(item => item.ClaimType)
            .ThenBy(item => item.ClaimValue)
            .ThenBy(item => item.RoleName)
            .ToListAsync();
    }

    public sealed class DroitItem
    {
        public int Id { get; init; }
        public string ClaimType { get; init; } = string.Empty;
        public string ClaimValue { get; init; } = string.Empty;
        public string RoleName { get; init; } = string.Empty;
    }
}

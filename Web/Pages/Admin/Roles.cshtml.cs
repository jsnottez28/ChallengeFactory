using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Web.Pages.Admin;

public class RolesModel : PageModel
{
    private readonly RoleManager<IdentityRole> _roleManager;

    public RolesModel(RoleManager<IdentityRole> roleManager)
    {
        _roleManager = roleManager;
    }

    public List<RoleItem> Roles { get; private set; } = [];

    public async Task OnGetAsync()
    {
        Roles = await _roleManager.Roles
            .OrderBy(role => role.Name)
            .Select(role => new RoleItem
            {
                Id = role.Id,
                Name = role.Name ?? string.Empty,
                NormalizedName = role.NormalizedName ?? string.Empty
            })
            .ToListAsync();
    }

    public sealed class RoleItem
    {
        public string Id { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
        public string NormalizedName { get; init; } = string.Empty;
    }
}

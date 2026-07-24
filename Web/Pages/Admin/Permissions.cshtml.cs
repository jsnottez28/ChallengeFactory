using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Web.Data;
using Web.Data.Entities;

namespace Web.Pages.Admin;

public class PermissionsModel : PageModel
{
    private readonly ApplicationDbContext _dbContext;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public PermissionsModel(
        ApplicationDbContext dbContext,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        _dbContext = dbContext;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public List<UserPermissionRow> Users { get; private set; } = [];
    public UserPermissionDetail? SelectedUser { get; private set; }
    public List<OrganisationScopeRow> Organisations { get; private set; } = [];
    public List<ClaimRow> RoleClaims { get; private set; } = [];

    [BindProperty]
    public InputModel Input { get; set; } = new();

    [TempData]
    public string? StatusMessage { get; set; }

    public async Task OnGetAsync(string? userId)
    {
        await LoadAsync(userId);
    }

    public async Task<IActionResult> OnPostSaveScopesAsync()
    {
        var user = await _userManager.FindByIdAsync(Input.UserId);
        if (user is null)
        {
            StatusMessage = "Utilisateur introuvable.";
            return RedirectToPage();
        }

        var selectedOrganisationIds = Input.SelectedOrganisationIds.ToHashSet();
        var scopes = await _dbContext.Scopes
            .Where(scope => scope.ApplicationUserId == user.Id && scope.OrganisationId != null)
            .OrderBy(scope => scope.Id)
            .ToListAsync();

        var scopesByOrganisation = scopes
            .Where(scope => scope.OrganisationId.HasValue)
            .GroupBy(scope => scope.OrganisationId!.Value)
            .ToDictionary(group => group.Key, group => group.ToList());

        foreach (var organisationScopes in scopesByOrganisation)
        {
            var shouldBeActive = selectedOrganisationIds.Contains(organisationScopes.Key);
            var firstScope = organisationScopes.Value.First();

            firstScope.EstActif = shouldBeActive;

            foreach (var duplicateScope in organisationScopes.Value.Skip(1))
            {
                duplicateScope.EstActif = false;
            }
        }

        var existingOrganisationIds = scopesByOrganisation.Keys.ToHashSet();
        foreach (var organisationId in selectedOrganisationIds.Where(id => !existingOrganisationIds.Contains(id)))
        {
            var organisationExists = await _dbContext.Organisations.AnyAsync(organisation => organisation.Id == organisationId);
            if (!organisationExists)
            {
                continue;
            }

            _dbContext.Scopes.Add(new Scope
            {
                ApplicationUserId = user.Id,
                OrganisationId = organisationId,
                EstActif = true,
                CreeLe = DateTime.UtcNow
            });
        }

        await _dbContext.SaveChangesAsync();

        StatusMessage = "Périmètres d'accès mis à jour.";
        return RedirectToPage(new { userId = user.Id });
    }

    private async Task LoadAsync(string? selectedUserId)
    {
        var users = await _userManager.Users
            .OrderBy(user => user.Nom)
            .ThenBy(user => user.Prenom)
            .ThenBy(user => user.Email)
            .ToListAsync();

        var activeScopeCounts = await _dbContext.Scopes
            .Where(scope => scope.EstActif && scope.OrganisationId != null)
            .GroupBy(scope => scope.ApplicationUserId)
            .Select(group => new { UserId = group.Key, Count = group.Count() })
            .ToDictionaryAsync(item => item.UserId, item => item.Count);

        Users = [];
        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            Users.Add(new UserPermissionRow(
                user.Id,
                GetDisplayName(user),
                user.Email ?? "-",
                roles.ToList(),
                activeScopeCounts.GetValueOrDefault(user.Id)));
        }

        var selectedUser = users.FirstOrDefault(user => user.Id == selectedUserId) ?? users.FirstOrDefault();
        if (selectedUser is null)
        {
            return;
        }

        var selectedRoles = (await _userManager.GetRolesAsync(selectedUser)).ToList();
        SelectedUser = new UserPermissionDetail(
            selectedUser.Id,
            GetDisplayName(selectedUser),
            selectedUser.Email ?? "-",
            selectedRoles);

        var selectedOrganisationIds = await _dbContext.Scopes
            .Where(scope => scope.ApplicationUserId == selectedUser.Id && scope.EstActif && scope.OrganisationId != null)
            .Select(scope => scope.OrganisationId!.Value)
            .Distinct()
            .ToListAsync();

        Organisations = await _dbContext.Organisations
            .OrderBy(organisation => organisation.RaisonSociale)
            .ThenBy(organisation => organisation.CodeAdherent)
            .Select(organisation => new OrganisationScopeRow(
                organisation.Id,
                organisation.RaisonSociale,
                organisation.CodeAdherent,
                organisation.EstActif,
                selectedOrganisationIds.Contains(organisation.Id)))
            .ToListAsync();

        RoleClaims = await LoadRoleClaimsAsync(selectedRoles);

        Input = new InputModel
        {
            UserId = selectedUser.Id,
            SelectedOrganisationIds = selectedOrganisationIds
        };
    }

    private async Task<List<ClaimRow>> LoadRoleClaimsAsync(IEnumerable<string> roleNames)
    {
        var claims = new List<Claim>();

        foreach (var roleName in roleNames)
        {
            var role = await _roleManager.FindByNameAsync(roleName);
            if (role is null)
            {
                continue;
            }

            claims.AddRange(await _roleManager.GetClaimsAsync(role));
        }

        return claims
            .Select(claim => new ClaimRow(claim.Type, claim.Value))
            .Distinct()
            .OrderBy(claim => claim.Type)
            .ThenBy(claim => claim.Value)
            .ToList();
    }

    private static string GetDisplayName(ApplicationUser user)
    {
        var fullName = string.Join(" ", new[] { user.Prenom, user.Nom }.Where(value => !string.IsNullOrWhiteSpace(value)));
        return !string.IsNullOrWhiteSpace(fullName) ? fullName : user.Email ?? user.UserName ?? "Utilisateur";
    }

    public sealed class InputModel
    {
        public string UserId { get; set; } = string.Empty;
        public List<int> SelectedOrganisationIds { get; set; } = [];
    }

    public sealed record UserPermissionRow(
        string Id,
        string DisplayName,
        string Email,
        List<string> Roles,
        int ActiveScopeCount);

    public sealed record UserPermissionDetail(
        string Id,
        string DisplayName,
        string Email,
        List<string> Roles);

    public sealed record OrganisationScopeRow(
        int Id,
        string RaisonSociale,
        string CodeAdherent,
        bool EstActif,
        bool IsSelected);

    public sealed record ClaimRow(string Type, string Value);
}

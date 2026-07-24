using Application.Common.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Web.Data;

namespace Web.Services;

public sealed class PermissionService(ApplicationDbContext dbContext, RoleManager<ApplicationRole> roleManager) : IPermissionService
{
    public async Task<RolePermissions?> GetRolePermissionsAsync(string roleId)
    {
        var role = await roleManager.FindByIdAsync(roleId);
        if (role is null)
        {
            return null;
        }

        var droitIdsCoches = await dbContext.RoleDroits
            .Where(roleDroit => roleDroit.RoleId == roleId)
            .Select(roleDroit => roleDroit.DroitId)
            .ToListAsync();

        var groupeDroitIdsCoches = await dbContext.RoleGroupesDroit
            .Where(roleGroupe => roleGroupe.RoleId == roleId)
            .Select(roleGroupe => roleGroupe.GroupeDroitId)
            .ToListAsync();

        var groupes = await dbContext.GroupesDroit
            .OrderBy(groupe => groupe.Libelle)
            .Select(groupe => new GroupeDroitAvecStatut
            {
                GroupeDroit = groupe,
                EstCoche = groupeDroitIdsCoches.Contains(groupe.Id),
            })
            .ToListAsync();

        var droits = await dbContext.Droits
            .Include(droit => droit.Ressource)
            .Include(droit => droit.TypeAction)
            .Include(droit => droit.GroupeDroit)
            .OrderBy(droit => droit.Ressource.Libelle)
            .ThenBy(droit => droit.TypeAction.Libelle)
            .ToListAsync();

        var droitsAvecStatut = droits.Select(droit =>
        {
            var estIncluViaGroupe = droit.GroupeDroitId is { } groupeDroitId && groupeDroitIdsCoches.Contains(groupeDroitId);

            return new DroitAvecStatut
            {
                Droit = droit,
                EstCocheDirectement = droitIdsCoches.Contains(droit.Id),
                EstIncluViaGroupe = estIncluViaGroupe,
                NomGroupeInclus = estIncluViaGroupe ? droit.GroupeDroit?.Libelle : null,
            };
        }).ToList();

        return new RolePermissions
        {
            RoleId = role.Id,
            RoleName = role.Name ?? role.Id,
            Groupes = groupes,
            Droits = droitsAvecStatut,
        };
    }

    public async Task<(bool Success, string? ErrorMessage)> UpdateRolePermissionsAsync(string roleId, List<int> droitIds, List<int> groupeDroitIds)
    {
        var role = await roleManager.FindByIdAsync(roleId);
        if (role is null)
        {
            return (false, "Role introuvable.");
        }

        var droitIdsValides = await dbContext.Droits
            .Where(droit => droitIds.Contains(droit.Id))
            .Select(droit => droit.Id)
            .ToListAsync();

        var groupeDroitIdsValides = await dbContext.GroupesDroit
            .Where(groupe => groupeDroitIds.Contains(groupe.Id))
            .Select(groupe => groupe.Id)
            .ToListAsync();

        await using var transaction = await dbContext.Database.BeginTransactionAsync();

        var anciensRoleDroits = dbContext.RoleDroits.Where(roleDroit => roleDroit.RoleId == roleId);
        dbContext.RoleDroits.RemoveRange(anciensRoleDroits);

        var anciensRoleGroupes = dbContext.RoleGroupesDroit.Where(roleGroupe => roleGroupe.RoleId == roleId);
        dbContext.RoleGroupesDroit.RemoveRange(anciensRoleGroupes);

        await dbContext.SaveChangesAsync();

        dbContext.RoleDroits.AddRange(droitIdsValides.Select(droitId => new RoleDroit
        {
            RoleId = roleId,
            DroitId = droitId,
        }));

        dbContext.RoleGroupesDroit.AddRange(groupeDroitIdsValides.Select(groupeDroitId => new RoleGroupeDroit
        {
            RoleId = roleId,
            GroupeDroitId = groupeDroitId,
        }));

        await dbContext.SaveChangesAsync();
        await transaction.CommitAsync();

        return (true, null);
    }

    public async Task<List<string>> GetEffectiveDroitCodesAsync(IReadOnlyCollection<string> roleIds)
    {
        if (roleIds.Count == 0)
        {
            return [];
        }

        var codesDirects = await dbContext.RoleDroits
            .Where(roleDroit => roleIds.Contains(roleDroit.RoleId))
            .Select(roleDroit => roleDroit.Droit.Code)
            .ToListAsync();

        var codesViaGroupes = await dbContext.RoleGroupesDroit
            .Where(roleGroupe => roleIds.Contains(roleGroupe.RoleId))
            .SelectMany(roleGroupe => roleGroupe.GroupeDroit.Droits.Select(droit => droit.Code))
            .ToListAsync();

        return codesDirects
            .Concat(codesViaGroupes)
            .Distinct()
            .ToList();
    }
}

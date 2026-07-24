using Application.Common.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Web.Services;

public sealed class GroupeDroitService(ApplicationDbContext dbContext) : IGroupeDroitService
{
    public async Task<List<GroupeDroit>> GetAllAsync()
    {
        return await dbContext.GroupesDroit
            .Include(groupe => groupe.Droits)
            .OrderBy(groupe => groupe.Libelle)
            .ToListAsync();
    }

    public async Task<GroupeDroit?> GetByIdAsync(int id)
    {
        return await dbContext.GroupesDroit
            .Include(groupe => groupe.Droits)
                .ThenInclude(droit => droit.Ressource)
            .Include(groupe => groupe.Droits)
                .ThenInclude(droit => droit.TypeAction)
            .FirstOrDefaultAsync(groupe => groupe.Id == id);
    }

    public async Task<(bool Success, string? ErrorMessage, GroupeDroit? GroupeDroit)> CreateAsync(GroupeDroitInput input)
    {
        if (await dbContext.GroupesDroit.AnyAsync(groupe => groupe.Code == input.Code))
        {
            return (false, "Un groupe avec ce code existe deja.", null);
        }

        var groupeCree = new GroupeDroit
        {
            Code = input.Code,
            Libelle = input.Libelle,
            Description = input.Description,
        };

        dbContext.GroupesDroit.Add(groupeCree);
        await dbContext.SaveChangesAsync();

        return (true, null, groupeCree);
    }

    public async Task<(bool Success, string? ErrorMessage)> UpdateAsync(int id, GroupeDroitInput input)
    {
        var groupe = await dbContext.GroupesDroit.FirstOrDefaultAsync(g => g.Id == id);
        if (groupe is null)
        {
            return (false, "Groupe introuvable.");
        }

        if (await dbContext.GroupesDroit.AnyAsync(g => g.Code == input.Code && g.Id != id))
        {
            return (false, "Un groupe avec ce code existe deja.");
        }

        groupe.Code = input.Code;
        groupe.Libelle = input.Libelle;
        groupe.Description = input.Description;

        await dbContext.SaveChangesAsync();

        return (true, null);
    }

    public async Task<(bool Success, string? ErrorMessage)> DeleteAsync(int id)
    {
        var groupe = await dbContext.GroupesDroit.FirstOrDefaultAsync(g => g.Id == id);
        if (groupe is null)
        {
            return (false, "Groupe introuvable.");
        }

        // Les droits du groupe ne sont pas supprimes : leur GroupeDroitId repasse a null (SetNull en base).
        dbContext.GroupesDroit.Remove(groupe);
        await dbContext.SaveChangesAsync();

        return (true, null);
    }
}

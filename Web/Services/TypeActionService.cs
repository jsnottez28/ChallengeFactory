using Application.Common.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Web.Services;

public sealed class TypeActionService(ApplicationDbContext dbContext) : ITypeActionService
{
    public async Task<List<TypeAction>> GetAllAsync()
    {
        return await dbContext.TypesAction
            .OrderBy(typeAction => typeAction.Libelle)
            .ToListAsync();
    }

    public async Task<TypeAction?> GetByIdAsync(int id)
    {
        return await dbContext.TypesAction
            .FirstOrDefaultAsync(typeAction => typeAction.Id == id);
    }

    public async Task<(bool Success, string? ErrorMessage, TypeAction? TypeAction)> CreateAsync(TypeActionInput input)
    {
        if (await dbContext.TypesAction.AnyAsync(typeAction => typeAction.Code == input.Code))
        {
            return (false, "Un type d'action avec ce code existe deja.", null);
        }

        var typeActionCree = new TypeAction
        {
            Code = input.Code,
            Libelle = input.Libelle,
            Description = input.Description,
        };

        dbContext.TypesAction.Add(typeActionCree);
        await dbContext.SaveChangesAsync();

        return (true, null, typeActionCree);
    }

    public async Task<(bool Success, string? ErrorMessage)> UpdateAsync(int id, TypeActionInput input)
    {
        var typeAction = await dbContext.TypesAction.FirstOrDefaultAsync(t => t.Id == id);
        if (typeAction is null)
        {
            return (false, "Type d'action introuvable.");
        }

        if (await dbContext.TypesAction.AnyAsync(t => t.Code == input.Code && t.Id != id))
        {
            return (false, "Un type d'action avec ce code existe deja.");
        }

        typeAction.Code = input.Code;
        typeAction.Libelle = input.Libelle;
        typeAction.Description = input.Description;

        await dbContext.SaveChangesAsync();

        return (true, null);
    }

    public async Task<(bool Success, string? ErrorMessage)> DeleteAsync(int id)
    {
        var typeAction = await dbContext.TypesAction.FirstOrDefaultAsync(t => t.Id == id);
        if (typeAction is null)
        {
            return (false, "Type d'action introuvable.");
        }

        if (await dbContext.Droits.AnyAsync(droit => droit.TypeActionId == id))
        {
            return (false, "Impossible de supprimer ce type d'action : il est utilise par au moins un droit.");
        }

        dbContext.TypesAction.Remove(typeAction);
        await dbContext.SaveChangesAsync();

        return (true, null);
    }
}

using Application.Common.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Web.Services;

public sealed class RessourceService(ApplicationDbContext dbContext) : IRessourceService
{
    public async Task<List<Ressource>> GetAllAsync()
    {
        return await dbContext.Ressources
            .OrderBy(ressource => ressource.Libelle)
            .ToListAsync();
    }

    public async Task<Ressource?> GetByIdAsync(int id)
    {
        return await dbContext.Ressources
            .FirstOrDefaultAsync(ressource => ressource.Id == id);
    }

    public async Task<(bool Success, string? ErrorMessage, Ressource? Ressource)> CreateAsync(RessourceInput input)
    {
        if (await dbContext.Ressources.AnyAsync(ressource => ressource.Code == input.Code))
        {
            return (false, "Une ressource avec ce code existe deja.", null);
        }

        var ressourceCreee = new Ressource
        {
            Code = input.Code,
            Libelle = input.Libelle,
            Description = input.Description,
        };

        dbContext.Ressources.Add(ressourceCreee);
        await dbContext.SaveChangesAsync();

        return (true, null, ressourceCreee);
    }

    public async Task<(bool Success, string? ErrorMessage)> UpdateAsync(int id, RessourceInput input)
    {
        var ressource = await dbContext.Ressources.FirstOrDefaultAsync(r => r.Id == id);
        if (ressource is null)
        {
            return (false, "Ressource introuvable.");
        }

        if (await dbContext.Ressources.AnyAsync(r => r.Code == input.Code && r.Id != id))
        {
            return (false, "Une ressource avec ce code existe deja.");
        }

        ressource.Code = input.Code;
        ressource.Libelle = input.Libelle;
        ressource.Description = input.Description;

        await dbContext.SaveChangesAsync();

        return (true, null);
    }

    public async Task<(bool Success, string? ErrorMessage)> DeleteAsync(int id)
    {
        var ressource = await dbContext.Ressources.FirstOrDefaultAsync(r => r.Id == id);
        if (ressource is null)
        {
            return (false, "Ressource introuvable.");
        }

        if (await dbContext.Droits.AnyAsync(droit => droit.RessourceId == id))
        {
            return (false, "Impossible de supprimer cette ressource : elle est utilisee par au moins un droit.");
        }

        dbContext.Ressources.Remove(ressource);
        await dbContext.SaveChangesAsync();

        return (true, null);
    }
}

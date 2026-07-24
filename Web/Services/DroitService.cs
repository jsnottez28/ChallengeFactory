using Application.Common.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Web.Services;

public sealed class DroitService(ApplicationDbContext dbContext) : IDroitService
{
    public async Task<List<Droit>> GetAllAsync(int? ressourceId, int? groupeDroitId)
    {
        var requete = dbContext.Droits
            .Include(droit => droit.Ressource)
            .Include(droit => droit.TypeAction)
            .Include(droit => droit.GroupeDroit)
            .AsQueryable();

        if (ressourceId is { } ressourceIdValeur)
        {
            requete = requete.Where(droit => droit.RessourceId == ressourceIdValeur);
        }

        if (groupeDroitId is { } groupeDroitIdValeur)
        {
            requete = requete.Where(droit => droit.GroupeDroitId == groupeDroitIdValeur);
        }

        return await requete
            .OrderBy(droit => droit.Ressource.Libelle)
            .ThenBy(droit => droit.TypeAction.Libelle)
            .ToListAsync();
    }

    public async Task<Droit?> GetByIdAsync(int id)
    {
        return await dbContext.Droits
            .Include(droit => droit.Ressource)
            .Include(droit => droit.TypeAction)
            .Include(droit => droit.GroupeDroit)
            .FirstOrDefaultAsync(droit => droit.Id == id);
    }

    public async Task<(bool Success, string? ErrorMessage, Droit? Droit)> CreateAsync(DroitInput input)
    {
        var (success, errorMessage, code) = await ResoudreCodeAsync(input, droitIdExclu: null);
        if (!success)
        {
            return (false, errorMessage, null);
        }

        var droitCree = new Droit
        {
            Code = code!,
            Libelle = input.Libelle,
            Description = input.Description,
            RessourceId = input.RessourceId,
            TypeActionId = input.TypeActionId,
            GroupeDroitId = input.GroupeDroitId,
        };

        dbContext.Droits.Add(droitCree);
        await dbContext.SaveChangesAsync();

        return (true, null, droitCree);
    }

    public async Task<(bool Success, string? ErrorMessage)> UpdateAsync(int id, DroitInput input)
    {
        var droit = await dbContext.Droits.FirstOrDefaultAsync(d => d.Id == id);
        if (droit is null)
        {
            return (false, "Droit introuvable.");
        }

        var (success, errorMessage, code) = await ResoudreCodeAsync(input, droitIdExclu: id);
        if (!success)
        {
            return (false, errorMessage);
        }

        droit.Code = code!;
        droit.Libelle = input.Libelle;
        droit.Description = input.Description;
        droit.RessourceId = input.RessourceId;
        droit.TypeActionId = input.TypeActionId;
        droit.GroupeDroitId = input.GroupeDroitId;

        await dbContext.SaveChangesAsync();

        return (true, null);
    }

    public async Task<(bool Success, string? ErrorMessage)> DeleteAsync(int id)
    {
        var droit = await dbContext.Droits.FirstOrDefaultAsync(d => d.Id == id);
        if (droit is null)
        {
            return (false, "Droit introuvable.");
        }

        dbContext.Droits.Remove(droit);
        await dbContext.SaveChangesAsync();

        return (true, null);
    }

    private async Task<(bool Success, string? ErrorMessage, string? Code)> ResoudreCodeAsync(DroitInput input, int? droitIdExclu)
    {
        var couplePrisAilleurs = await dbContext.Droits.AnyAsync(droit =>
            droit.RessourceId == input.RessourceId &&
            droit.TypeActionId == input.TypeActionId &&
            (droitIdExclu == null || droit.Id != droitIdExclu));
        if (couplePrisAilleurs)
        {
            return (false, "Un droit existe deja pour cette ressource et ce type d'action.", null);
        }

        var code = input.Code;
        if (string.IsNullOrWhiteSpace(code))
        {
            var ressource = await dbContext.Ressources.FirstOrDefaultAsync(r => r.Id == input.RessourceId);
            var typeAction = await dbContext.TypesAction.FirstOrDefaultAsync(t => t.Id == input.TypeActionId);
            if (ressource is null || typeAction is null)
            {
                return (false, "Ressource ou type d'action introuvable.", null);
            }

            code = $"{ressource.Code}.{typeAction.Code}";
        }

        var codePrisAilleurs = await dbContext.Droits.AnyAsync(droit =>
            droit.Code == code &&
            (droitIdExclu == null || droit.Id != droitIdExclu));
        if (codePrisAilleurs)
        {
            return (false, "Un droit avec ce code existe deja.", null);
        }

        return (true, null, code);
    }
}

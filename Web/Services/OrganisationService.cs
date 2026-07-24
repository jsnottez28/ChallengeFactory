using Microsoft.EntityFrameworkCore;
using Web.Data.Entities;

namespace Web.Services;

public sealed class OrganisationService(ApplicationDbContext dbContext) : IOrganisationService
{
    public async Task<List<Organisation>> GetAllAsync()
    {
        return await dbContext.Organisations
            .OrderBy(organisation => organisation.RaisonSociale)
            .ToListAsync();
    }

    public async Task<Organisation?> GetByIdAsync(int id)
    {
        return await dbContext.Organisations
            .FirstOrDefaultAsync(organisation => organisation.Id == id);
    }

    public async Task<Organisation> CreateAsync(OrganisationInput input)
    {
        var organisation = new Organisation
        {
            CodeAdherent = input.CodeAdherent,
            RaisonSociale = input.RaisonSociale,
            Adresse1 = input.Adresse1,
            Adresse2 = input.Adresse2,
            CodePostal = input.CodePostal,
            Ville = input.Ville,
            TelephoneStandard = input.TelephoneStandard,
            EstActif = input.EstActif,
        };

        dbContext.Organisations.Add(organisation);
        await dbContext.SaveChangesAsync();

        return organisation;
    }

    public async Task<bool> UpdateAsync(int id, OrganisationInput input)
    {
        var organisation = await dbContext.Organisations
            .FirstOrDefaultAsync(organisation => organisation.Id == id);
        if (organisation is null)
        {
            return false;
        }

        organisation.CodeAdherent = input.CodeAdherent;
        organisation.RaisonSociale = input.RaisonSociale;
        organisation.Adresse1 = input.Adresse1;
        organisation.Adresse2 = input.Adresse2;
        organisation.CodePostal = input.CodePostal;
        organisation.Ville = input.Ville;
        organisation.TelephoneStandard = input.TelephoneStandard;
        organisation.EstActif = input.EstActif;

        await dbContext.SaveChangesAsync();

        return true;
    }

    public async Task<bool> SetActiveAsync(int id, bool estActif)
    {
        var organisation = await dbContext.Organisations
            .FirstOrDefaultAsync(organisation => organisation.Id == id);
        if (organisation is null)
        {
            return false;
        }

        organisation.EstActif = estActif;
        await dbContext.SaveChangesAsync();

        return true;
    }
}

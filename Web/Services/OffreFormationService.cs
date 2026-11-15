using Application.Common.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Web.Services;

public sealed class OffreFormationService(ApplicationDbContext dbContext) : IOffreFormationService
{
    public async Task<List<OffreFormation>> GetAllAsync()
    {
        return await dbContext.OffresFormation
            .OrderBy(o => o.Categorie)
            .ThenBy(o => o.Ordre)
            .ThenBy(o => o.Titre)
            .ToListAsync();
    }

    public async Task<OffreFormation?> GetByIdAsync(int id)
    {
        return await dbContext.OffresFormation.FirstOrDefaultAsync(o => o.Id == id);
    }

    public async Task<List<OffreFormation>> GetPubliquesAsync()
    {
        return await dbContext.OffresFormation
            .Include(o => o.Challenge)
            .Where(o => o.EstPublie)
            .OrderBy(o => o.Categorie)
            .ThenBy(o => o.Ordre)
            .ThenBy(o => o.Titre)
            .ToListAsync();
    }

    public async Task<(bool Success, string? ErrorMessage, OffreFormation? OffreFormation)> CreateAsync(OffreFormationInput input)
    {
        var erreur = Valider(input);
        if (erreur is not null)
        {
            return (false, erreur, null);
        }

        var offre = AppliquerInput(new OffreFormation(), input);
        dbContext.OffresFormation.Add(offre);
        await dbContext.SaveChangesAsync();
        return (true, null, offre);
    }

    public async Task<(bool Success, string? ErrorMessage)> UpdateAsync(int id, OffreFormationInput input)
    {
        var erreur = Valider(input);
        if (erreur is not null)
        {
            return (false, erreur);
        }

        var offre = await dbContext.OffresFormation.FirstOrDefaultAsync(o => o.Id == id);
        if (offre is null)
        {
            return (false, "Formation introuvable.");
        }

        AppliquerInput(offre, input);
        await dbContext.SaveChangesAsync();
        return (true, null);
    }

    public async Task<(bool Success, string? ErrorMessage)> DeleteAsync(int id)
    {
        var offre = await dbContext.OffresFormation.FirstOrDefaultAsync(o => o.Id == id);
        if (offre is null)
        {
            return (false, "Formation introuvable.");
        }

        dbContext.OffresFormation.Remove(offre);
        await dbContext.SaveChangesAsync();
        return (true, null);
    }

    private static string? Valider(OffreFormationInput input)
    {
        if (string.IsNullOrWhiteSpace(input.Titre))
        {
            return "Le titre est obligatoire.";
        }

        if (string.IsNullOrWhiteSpace(input.Duree))
        {
            return "La durée est obligatoire.";
        }

        if (string.IsNullOrWhiteSpace(input.Public))
        {
            return "Le public visé est obligatoire.";
        }

        return null;
    }

    private static OffreFormation AppliquerInput(OffreFormation offre, OffreFormationInput input)
    {
        offre.Titre = input.Titre;
        offre.Categorie = input.Categorie;
        offre.LienAvecOffre = input.LienAvecOffre;
        offre.Duree = input.Duree;
        offre.Public = input.Public;
        offre.Financement = input.Financement;
        offre.Prix = string.IsNullOrWhiteSpace(input.Prix) ? "Sur devis" : input.Prix;
        offre.DisponibleEnChallenge = input.DisponibleEnChallenge;
        offre.DisponibleEnPresentiel = input.DisponibleEnPresentiel;
        offre.Cible = input.Cible;
        offre.Statut = input.Statut;
        offre.ChallengeId = input.ChallengeId;
        offre.LienPersonnalise = input.LienPersonnalise;
        offre.Ordre = input.Ordre;
        offre.EstPublie = input.EstPublie;
        return offre;
    }
}

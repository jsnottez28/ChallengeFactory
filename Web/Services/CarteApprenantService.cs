using Application.Common.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Web.Services;

public sealed class CarteApprenantService(ApplicationDbContext dbContext) : ICarteApprenantService
{
    public async Task<List<CarteCompetence>> GetMesCartesAsync(string utilisateurId)
    {
        return await dbContext.CarteAttributions
            .Where(a => a.UtilisateurId == utilisateurId && a.EstActif)
            .Include(a => a.CarteCompetence)
                .ThenInclude(c => c.Badge)
            .OrderByDescending(a => a.AttribueLe)
            .Select(a => a.CarteCompetence)
            .ToListAsync();
    }

    public async Task<CarteCompetence?> GetCarteAttribueeAsync(string utilisateurId, int carteId)
    {
        // La jointure sur CarteAttribution.EstActif est ce qui empeche un apprenant
        // d'acceder a une carte en devinant/forcant son Id dans l'URL : sans ligne
        // d'attribution active, aucune carte n'est renvoyee, quel que soit l'Id demande.
        return await dbContext.CarteAttributions
            .Where(a => a.UtilisateurId == utilisateurId && a.EstActif && a.CarteCompetenceId == carteId)
            .Include(a => a.CarteCompetence)
                .ThenInclude(c => c.Badge)
            .Select(a => a.CarteCompetence)
            .FirstOrDefaultAsync();
    }
}

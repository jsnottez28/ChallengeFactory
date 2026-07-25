using Application.Common.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Web.Data;

namespace Web.Services;

public sealed class CarteApprenantService(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager) : ICarteApprenantService
{
    public async Task<List<CarteBibliothequeInfo>> GetMesCartesAsync(string utilisateurId)
    {
        if (!await AccesContenuAutoriseAsync(utilisateurId))
        {
            return [];
        }

        var attributions = await dbContext.CarteAttributions
            .Where(a => a.UtilisateurId == utilisateurId && a.EstActif)
            .Include(a => a.CarteCompetence)
                .ThenInclude(c => c.Badge)
            .Include(a => a.ChallengeEtape)
                .ThenInclude(e => e!.Challenge)
            .OrderByDescending(a => a.AttribueLe)
            .ToListAsync();

        return attributions.Select(a => new CarteBibliothequeInfo
        {
            Carte = a.CarteCompetence,
            OrigineType = a.OrigineType,
            ChallengeTitre = a.ChallengeEtape?.Challenge.Titre,
            NumeroEtape = a.ChallengeEtape?.NumeroEtape,
            AttribueLe = a.AttribueLe,
        }).ToList();
    }

    public async Task<CarteCompetence?> GetCarteAttribueeAsync(string utilisateurId, int carteId)
    {
        if (!await AccesContenuAutoriseAsync(utilisateurId))
        {
            return null;
        }

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

    // Un compte Suspendu ou En attente de validation (BtoC, statut_acces_plateforme) ne
    // doit avoir acces a aucun contenu apprenant - meme s'il a deja des cartes attribuees
    // par ailleurs. Verifie cote serveur, jamais seulement masque cote UI.
    private async Task<bool> AccesContenuAutoriseAsync(string utilisateurId)
    {
        var utilisateur = await userManager.FindByIdAsync(utilisateurId);
        return utilisateur is not null && (utilisateur.EstSuperAdministrateur || utilisateur.Statut == StatutUtilisateur.Actif);
    }
}

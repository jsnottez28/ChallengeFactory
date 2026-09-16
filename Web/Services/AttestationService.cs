using Application.Common.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Web.Data;

namespace Web.Services;

public sealed class AttestationService(ApplicationDbContext dbContext) : IAttestationService
{
    public async Task<AttestationInfo?> GetAttestationAsync(int cohorteId, string utilisateurId)
    {
        var cohorte = await dbContext.Cohortes
            .Include(c => c.Challenge)
            .Include(c => c.Organisation)
            .FirstOrDefaultAsync(c => c.Id == cohorteId);

        if (cohorte is null || cohorte.Statut != StatutCohorte.Terminee)
        {
            return null;
        }

        var membre = await dbContext.CohorteMembres
            .Include(m => m.Utilisateur)
            .FirstOrDefaultAsync(m => m.CohorteId == cohorteId && m.UtilisateurId == utilisateurId);

        if (membre is null)
        {
            return null;
        }

        var validations = await dbContext.CohorteEtapeValidations
            .Where(v => v.CohorteId == cohorteId)
            .OrderBy(v => v.NumeroEtape)
            .ToListAsync();

        var attributions = await dbContext.CarteAttributions
            .Include(a => a.CarteCompetence)
            .Where(a => a.CohorteId == cohorteId && a.UtilisateurId == utilisateurId && a.EstActif)
            .ToListAsync();

        var attributionIds = attributions.Select(a => a.Id).ToList();
        var emargementsSignes = await dbContext.Emargements
            .Where(e => attributionIds.Contains(e.CarteAttributionId) && e.SigneLe != null)
            .ToListAsync();

        return new AttestationInfo
        {
            ChallengeTitre = cohorte.Challenge.Titre,
            CohorteNom = cohorte.Nom,
            OrganisationNom = cohorte.Organisation?.RaisonSociale,
            BeneficiaireNomComplet = NomComplet(membre.Utilisateur),
            DateDebut = cohorte.DateLancement ?? validations.FirstOrDefault()?.ValideLe,
            DateFin = validations.LastOrDefault()?.ValideLe ?? DateTime.UtcNow,
            NombreEtapesValidees = validations.Count,
            NombreEtapesTotal = cohorte.Challenge.NombreEtapes,
            CartesAcquises = attributions
                .Select(a => a.CarteCompetence.TitreTheorie)
                .Distinct()
                .OrderBy(titre => titre)
                .ToList(),
            HeuresDisponibles = emargementsSignes.Count > 0,
            TotalHeuresPresence = emargementsSignes.Sum(e => e.HeuresPresence ?? 0),
            TotalHeuresTravailPersonnel = emargementsSignes.Sum(e => e.HeuresTravailPersonnel ?? 0),
        };
    }

    private static string NomComplet(ApplicationUser utilisateur)
    {
        var nomComplet = $"{utilisateur.Prenom} {utilisateur.Nom}".Trim();
        return nomComplet.Length > 0 ? nomComplet : (utilisateur.Email ?? utilisateur.UserName ?? utilisateur.Id);
    }
}

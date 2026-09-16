using Application.Common.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Web.Data;

namespace Web.Services;

public sealed class VisioService(ApplicationDbContext dbContext, IEmailService emailService) : IVisioService
{
    public async Task<(bool Success, string? ErrorMessage)> PlanifierAsync(int cohorteId, string gestionnaireId, DateTime? dateVisio, string? lienVisio)
    {
        var cohorte = await dbContext.Cohortes.FirstOrDefaultAsync(c => c.Id == cohorteId);
        if (cohorte is null)
        {
            return (false, "Cohorte introuvable.");
        }

        if (cohorte.Statut != StatutCohorte.Active)
        {
            return (false, "Seule une Cohorte active a une étape courante à planifier.");
        }

        var visio = await dbContext.EtapesVisio
            .FirstOrDefaultAsync(v => v.CohorteId == cohorteId && v.NumeroEtape == cohorte.EtapeCourante);

        if (visio is null)
        {
            visio = new EtapeVisio
            {
                CohorteId = cohorteId,
                NumeroEtape = cohorte.EtapeCourante,
                PlanifieParId = gestionnaireId,
                PlanifieLe = DateTime.UtcNow,
            };
            dbContext.EtapesVisio.Add(visio);
        }

        visio.DateVisio = dateVisio;
        visio.LienVisio = string.IsNullOrWhiteSpace(lienVisio) ? null : lienVisio.Trim();

        await dbContext.SaveChangesAsync();

        return (true, null);
    }

    public async Task<VisioEtapeInfo?> GetEtapeCouranteAsync(int cohorteId)
    {
        var cohorte = await dbContext.Cohortes.FirstOrDefaultAsync(c => c.Id == cohorteId);
        if (cohorte is null || cohorte.Statut != StatutCohorte.Active)
        {
            return null;
        }

        var visio = await dbContext.EtapesVisio
            .FirstOrDefaultAsync(v => v.CohorteId == cohorteId && v.NumeroEtape == cohorte.EtapeCourante);

        return new VisioEtapeInfo
        {
            NumeroEtape = cohorte.EtapeCourante,
            DateVisio = visio?.DateVisio,
            LienVisio = visio?.LienVisio,
            DernierEnvoiLe = visio?.DernierEnvoiLe,
        };
    }

    public async Task<(bool Success, string? ErrorMessage)> EnvoyerLienAsync(int cohorteId)
    {
        var cohorte = await dbContext.Cohortes
            .Include(c => c.Challenge)
            .Include(c => c.Membres).ThenInclude(m => m.Utilisateur)
            .FirstOrDefaultAsync(c => c.Id == cohorteId);

        if (cohorte is null)
        {
            return (false, "Cohorte introuvable.");
        }

        if (cohorte.Statut != StatutCohorte.Active)
        {
            return (false, "Seule une Cohorte active a une étape courante.");
        }

        var visio = await dbContext.EtapesVisio
            .FirstOrDefaultAsync(v => v.CohorteId == cohorteId && v.NumeroEtape == cohorte.EtapeCourante);

        if (visio is null || string.IsNullOrWhiteSpace(visio.LienVisio))
        {
            return (false, "Planifiez d'abord un lien de visio pour l'étape en cours.");
        }

        var etape = await dbContext.ChallengeEtapes
            .FirstOrDefaultAsync(e => e.ChallengeId == cohorte.ChallengeId && e.NumeroEtape == cohorte.EtapeCourante);

        var (sujet, corps) = ChallengeEmailTemplates.LienVisio(
            cohorte.Challenge.Titre,
            etape?.TitreEtape ?? $"Étape {cohorte.EtapeCourante}",
            visio.DateVisio,
            visio.LienVisio);

        foreach (var membre in cohorte.Membres)
        {
            if (!string.IsNullOrWhiteSpace(membre.Utilisateur.Email))
            {
                await emailService.EnvoyerAsync(membre.Utilisateur.Email, sujet, corps);
            }
        }

        visio.DernierEnvoiLe = DateTime.UtcNow;
        await dbContext.SaveChangesAsync();

        return (true, null);
    }
}

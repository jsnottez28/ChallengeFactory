using Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Web.Data;

namespace Web.Services;

public sealed class SatisfactionService(ApplicationDbContext dbContext) : ISatisfactionService
{
    public async Task<(bool Success, string? ErrorMessage)> EnregistrerReponseAsync(int cohorteId, string utilisateurId, int score, string? commentaire)
    {
        if (score < 0 || score > 10)
        {
            return (false, "La note doit être comprise entre 0 et 10.");
        }

        var cohorte = await dbContext.Cohortes.FirstOrDefaultAsync(c => c.Id == cohorteId);
        if (cohorte is null)
        {
            return (false, "Cohorte introuvable.");
        }

        var estMembre = await dbContext.CohorteMembres.AnyAsync(m => m.CohorteId == cohorteId && m.UtilisateurId == utilisateurId);
        if (!estMembre)
        {
            return (false, "Vous n'êtes pas membre de cette Cohorte.");
        }

        if (await ADejaReponduAsync(cohorteId, utilisateurId))
        {
            return (false, "Vous avez déjà répondu à cette enquête.");
        }

        dbContext.SatisfactionReponses.Add(new SatisfactionReponse
        {
            CohorteId = cohorteId,
            UtilisateurId = utilisateurId,
            Score = score,
            Commentaire = string.IsNullOrWhiteSpace(commentaire) ? null : commentaire.Trim(),
        });
        await dbContext.SaveChangesAsync();

        return (true, null);
    }

    public Task<bool> ADejaReponduAsync(int cohorteId, string utilisateurId) =>
        dbContext.SatisfactionReponses.AnyAsync(s => s.CohorteId == cohorteId && s.UtilisateurId == utilisateurId);

    public async Task<SatisfactionStats> GetStatsAsync(int cohorteId)
    {
        var reponses = await dbContext.SatisfactionReponses
            .Where(s => s.CohorteId == cohorteId)
            .OrderByDescending(s => s.RepondueLe)
            .ToListAsync();

        return new SatisfactionStats
        {
            NombreReponses = reponses.Count,
            ScoreMoyen = reponses.Count > 0 ? reponses.Average(s => s.Score) : null,
            DerniersCommentaires = reponses
                .Where(s => !string.IsNullOrWhiteSpace(s.Commentaire))
                .Select(s => s.Commentaire!)
                .Take(10)
                .ToList(),
        };
    }
}

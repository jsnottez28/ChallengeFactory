using Application.Common.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Web.Data;

namespace Web.Services;

public sealed class QuestionnaireMiParcoursService(ApplicationDbContext dbContext) : IQuestionnaireMiParcoursService
{
    public async Task<(bool Success, string? ErrorMessage)> EnregistrerReponseAsync(
        int cohorteId,
        string utilisateurId,
        PertinenceProjet projetToujoursPertinent,
        int noteAccompagnement,
        string? difficultesRencontrees,
        string? ajustementsSouhaites)
    {
        if (noteAccompagnement < 0 || noteAccompagnement > 10)
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
            return (false, "Vous avez déjà répondu à ce point d'étape.");
        }

        dbContext.QuestionnairesMiParcoursReponses.Add(new QuestionnaireMiParcoursReponse
        {
            CohorteId = cohorteId,
            UtilisateurId = utilisateurId,
            ProjetToujoursPertinent = projetToujoursPertinent,
            NoteAccompagnement = noteAccompagnement,
            DifficultesRencontrees = string.IsNullOrWhiteSpace(difficultesRencontrees) ? null : difficultesRencontrees.Trim(),
            AjustementsSouhaites = string.IsNullOrWhiteSpace(ajustementsSouhaites) ? null : ajustementsSouhaites.Trim(),
        });
        await dbContext.SaveChangesAsync();

        return (true, null);
    }

    public Task<bool> ADejaReponduAsync(int cohorteId, string utilisateurId) =>
        dbContext.QuestionnairesMiParcoursReponses.AnyAsync(r => r.CohorteId == cohorteId && r.UtilisateurId == utilisateurId);

    public async Task<QuestionnaireMiParcoursStats> GetStatsAsync(int cohorteId)
    {
        var reponses = await dbContext.QuestionnairesMiParcoursReponses
            .Where(r => r.CohorteId == cohorteId)
            .OrderByDescending(r => r.RepondueLe)
            .ToListAsync();

        return new QuestionnaireMiParcoursStats
        {
            NombreReponses = reponses.Count,
            NoteAccompagnementMoyenne = reponses.Count > 0 ? reponses.Average(r => r.NoteAccompagnement) : null,
            NombreProjetOui = reponses.Count(r => r.ProjetToujoursPertinent == PertinenceProjet.Oui),
            NombreProjetPartiellement = reponses.Count(r => r.ProjetToujoursPertinent == PertinenceProjet.Partiellement),
            NombreProjetNon = reponses.Count(r => r.ProjetToujoursPertinent == PertinenceProjet.Non),
            DernieresDifficultes = reponses
                .Where(r => !string.IsNullOrWhiteSpace(r.DifficultesRencontrees))
                .Select(r => r.DifficultesRencontrees!)
                .Take(10)
                .ToList(),
            DerniersAjustements = reponses
                .Where(r => !string.IsNullOrWhiteSpace(r.AjustementsSouhaites))
                .Select(r => r.AjustementsSouhaites!)
                .Take(10)
                .ToList(),
        };
    }
}

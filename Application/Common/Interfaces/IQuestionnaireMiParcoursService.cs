using Domain.Entities;

namespace Application.Common.Interfaces;

public sealed class QuestionnaireMiParcoursStats
{
    public int NombreReponses { get; set; }
    public double? NoteAccompagnementMoyenne { get; set; }
    public int NombreProjetOui { get; set; }
    public int NombreProjetPartiellement { get; set; }
    public int NombreProjetNon { get; set; }
    public List<string> DernieresDifficultes { get; set; } = [];
    public List<string> DerniersAjustements { get; set; } = [];
}

// Point d'etape a mi-parcours : envoye automatiquement a l'etape mediane de la Cohorte (cf.
// ICohorteService.LancerAsync/ValiderEtapeAsync), sans distinction par type de Challenge -
// une reponse par membre et par Cohorte, jamais de relance repetee (un seul point d'etape
// par parcours, meme principe d'unicite que SatisfactionReponse).
public interface IQuestionnaireMiParcoursService
{
    Task<(bool Success, string? ErrorMessage)> EnregistrerReponseAsync(
        int cohorteId,
        string utilisateurId,
        PertinenceProjet projetToujoursPertinent,
        int noteAccompagnement,
        string? difficultesRencontrees,
        string? ajustementsSouhaites);

    Task<bool> ADejaReponduAsync(int cohorteId, string utilisateurId);

    // Agrege par Cohorte - utilise en back-office (fiche Cohorte).
    Task<QuestionnaireMiParcoursStats> GetStatsAsync(int cohorteId);
}

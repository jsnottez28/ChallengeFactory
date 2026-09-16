namespace Application.Common.Interfaces;

public sealed class SatisfactionStats
{
    public int NombreReponses { get; set; }
    public double? ScoreMoyen { get; set; }
    public double? NoteContenusMoyenne { get; set; }
    public double? NoteAccompagnementMoyenne { get; set; }
    public double? NoteAdequationAttentesMoyenne { get; set; }
    public List<string> DerniersCommentaires { get; set; } = [];
}

// Critere 7 du Referentiel National Qualite (Qualiopi) : recueil des appreciations des
// beneficiaires. Une reponse par membre et par Cohorte, recueillie a la cloture (cf.
// ICohorteService.ValiderEtapeAsync / NotifierClotureAsync) - jamais de relance
// automatique repetee, une seule sollicitation par l'email de cloture. 4 notes (0-10
// chacune) pour couvrir plusieurs dimensions plutot qu'un seul NPS global, qui ne dit pas
// quoi ameliorer : recommandation, contenus, accompagnement, adequation aux attentes.
public interface ISatisfactionService
{
    // Idempotent cote metier : si l'utilisateur a deja repondu pour cette Cohorte, renvoie
    // une erreur explicite plutot que d'ecraser sa reponse (une enquete de satisfaction ne
    // se corrige pas a posteriori, cf. traçabilite d'audit).
    Task<(bool Success, string? ErrorMessage)> EnregistrerReponseAsync(
        int cohorteId, string utilisateurId, int score, int noteContenus, int noteAccompagnement, int noteAdequationAttentes, string? commentaire);

    Task<bool> ADejaReponduAsync(int cohorteId, string utilisateurId);

    // Agrege par Cohorte - utilise en back-office (fiche Cohorte) pour la preuve d'audit
    // Qualiopi (taux de satisfaction) et, a terme, pour les KPI de la mesure d'impact
    // (cf. CLAUDE.md, "Mesure d'impact & KPI").
    Task<SatisfactionStats> GetStatsAsync(int cohorteId);
}

namespace Application.Common.Interfaces;

public sealed class VisioEtapeInfo
{
    public int NumeroEtape { get; set; }
    public DateTime? DateVisio { get; set; }
    public string? LienVisio { get; set; }
    public DateTime? DernierEnvoiLe { get; set; }
}

// Rituel Synchrone (visio) propose a chaque etape d'un parcours, cf. CLAUDE.md "Boucle CBL
// hebdomadaire" - Temps 3 (Action). S'applique a l'identique a un parcours standard et a un
// bilan de competences (une seance = une etape) : le service ne connait que NumeroEtape,
// jamais de branche par type de Challenge.
public interface IVisioService
{
    // Cree ou met a jour la visio de l'etape courante de la Cohorte (date + lien externe
    // Zoom/Teams/Meet... saisi par le Gestionnaire - la plateforme n'heberge pas de visio).
    Task<(bool Success, string? ErrorMessage)> PlanifierAsync(int cohorteId, string gestionnaireId, DateTime? dateVisio, string? lienVisio);

    // Renvoie null si la Cohorte n'est pas active (pas d'etape courante a planifier).
    Task<VisioEtapeInfo?> GetEtapeCouranteAsync(int cohorteId);

    // Envoie (ou renvoie) le lien de connexion a tous les membres actuels de la Cohorte.
    // Echoue si aucun lien n'a ete planifie au prealable pour l'etape courante.
    Task<(bool Success, string? ErrorMessage)> EnvoyerLienAsync(int cohorteId);
}

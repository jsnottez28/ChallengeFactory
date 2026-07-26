using Domain.Entities;

namespace Web.Data;

// Visio planifiee pour introduire une etape aupres d'une Cohorte precise (jamais au
// niveau du Challenge modele - meme Challenge, plusieurs Cohortes = plusieurs visios
// independantes). Rattachee aux actions "Lancer la Cohorte" (etape 1) et "Valider
// l'etape en cours" (etape suivante), cf. ICohorteService - jamais une action admin
// separee. Une seule VisioEtape par (Cohorte, ChallengeEtape).
public class VisioEtape
{
    public int Id { get; set; }

    public int CohorteId { get; set; }
    public Cohorte Cohorte { get; set; } = null!;

    public int ChallengeEtapeId { get; set; }
    public ChallengeEtape ChallengeEtape { get; set; } = null!;

    public DateTime DateHeure { get; set; }
    public string LienConnexion { get; set; } = string.Empty;

    // Pre-rempli automatiquement (agenda fixe en 3 temps, cf. ICohorteService) mais
    // modifiable par le Gestionnaire avant sauvegarde - stocke tel que valide, pas
    // regenere dynamiquement a la lecture.
    public string Descriptif { get; set; } = string.Empty;

    public string PlanifieParId { get; set; } = string.Empty;
    public ApplicationUser PlanifiePar { get; set; } = null!;

    public DateTime DatePlanification { get; set; } = DateTime.UtcNow;
}

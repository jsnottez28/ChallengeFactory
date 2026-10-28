namespace Web.Data;

// Resultat d'un test de gestion des conflits Thomas-Kilmann (TKI) passe par un utilisateur.
// Contenu (30 enonces a choix force A/B, grille de scoring, descriptions des 5 modes)
// transcrit depuis la ressource fournie par Challenges Factory, reproduit cote serveur dans
// KilmannService - cf. IKilmannService. Une ligne par passage : les retests sont autorises,
// GetDernierResultatAsync renvoie le plus recent.
public class KilmannResultat
{
    public int Id { get; set; }

    public string UtilisateurId { get; set; } = string.Empty;
    public ApplicationUser Utilisateur { get; set; } = null!;

    public int ScoreRivaliser { get; set; }
    public int ScoreCollaborer { get; set; }
    public int ScoreCompromis { get; set; }
    public int ScoreEviter { get; set; }
    public int ScoreCeder { get; set; }

    // "Rivaliser", "Collaborer", "Compromis", "Eviter" ou "Ceder" - mode au score le plus eleve.
    public string ProfilDominant { get; set; } = string.Empty;

    public DateTime CompleteLe { get; set; } = DateTime.UtcNow;
}

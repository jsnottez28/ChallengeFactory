namespace Web.Data;

// Resultat d'un test psychotechnique RIASEC (Realiste / Investigateur / Artistique /
// Social / Entreprenant / Conventionnel, modele de Holland) passe par un utilisateur.
// Contenu (60 items) traduit depuis l'O*NET Interest Profiler Short Form (U.S. Department
// of Labor, National Center for O*NET Development), sous licence Creative Commons
// Attribution 4.0 (CC BY) - cf. RiasecService pour le detail et l'attribution. Une ligne
// par passage : les retests sont autorises, GetDernierResultatAsync renvoie le plus
// recent.
public class RiasecResultat
{
    public int Id { get; set; }

    public string UtilisateurId { get; set; } = string.Empty;
    public ApplicationUser Utilisateur { get; set; } = null!;

    public int ScoreR { get; set; }
    public int ScoreI { get; set; }
    public int ScoreA { get; set; }
    public int ScoreS { get; set; }
    public int ScoreE { get; set; }
    public int ScoreC { get; set; }

    // Code Holland a 3 lettres (ex. "SIA") : les 3 dimensions au score le plus eleve,
    // departagees par l'ordre R,I,A,S,E,C en cas d'egalite.
    public string CodeHolland { get; set; } = string.Empty;

    public DateTime CompleteLe { get; set; } = DateTime.UtcNow;
}

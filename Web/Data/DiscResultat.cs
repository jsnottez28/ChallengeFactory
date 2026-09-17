namespace Web.Data;

// Resultat d'un test psychotechnique DISC (Dominance / Influence / Stabilite /
// Conformite) passe par un utilisateur. Contenu et grille de scoring fournis par
// Challenges Factory (challenges-factory.com/Test/test-disc.html), reproduits cote
// serveur dans DiscService pour pouvoir lier le resultat au stagiaire, le stocker et
// l'envoyer par email a la fin (cf. IDiscService). Une ligne par passage : les retests
// sont autorises, GetDernierResultatAsync renvoie le plus recent.
public class DiscResultat
{
    public int Id { get; set; }

    public string UtilisateurId { get; set; } = string.Empty;
    public ApplicationUser Utilisateur { get; set; } = null!;

    public int ScoreD { get; set; }
    public int ScoreI { get; set; }
    public int ScoreS { get; set; }
    public int ScoreC { get; set; }

    // "D", "I", "S" ou "C" - dimension au score le plus eleve.
    public string ProfilDominant { get; set; } = string.Empty;

    public DateTime CompleteLe { get; set; } = DateTime.UtcNow;
}

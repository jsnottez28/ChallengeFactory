namespace Web.Data;

// Resultat d'un test psychotechnique RIASEC (Realiste / Investigateur / Artistique /
// Social / Entreprenant / Conventionnel, modele de Holland) passe par un utilisateur, au
// format a choix force par paires (comparaisons par paires). Contenu (60 items) traduit
// depuis l'O*NET Interest Profiler Short Form (U.S. Department of Labor, National Center
// for O*NET Development), sous licence Creative Commons Attribution 4.0 (CC BY) - cf.
// RiasecService pour le detail et l'attribution. Une ligne par passage : les retests sont
// autorises, GetDernierResultatAsync renvoie le plus recent.
public class RiasecResultat
{
    public int Id { get; set; }

    public string UtilisateurId { get; set; } = string.Empty;
    public ApplicationUser Utilisateur { get; set; } = null!;

    // Sur 10 (nombre de paires de base gagnees par cette dimension au round 1) - jamais
    // modifie par le round 2 de departage, qui n'affecte que le CodeHolland.
    public int ScoreR { get; set; }
    public int ScoreI { get; set; }
    public int ScoreA { get; set; }
    public int ScoreS { get; set; }
    public int ScoreE { get; set; }
    public int ScoreC { get; set; }

    // Code Holland a 3 lettres (ex. "SIA") : les 3 dimensions au score le plus eleve,
    // affinees par le round 2 de departage si declenche (cf. DepartageDimensionA/B/Gagnant),
    // sinon departagees par l'ordre fixe R,I,A,S,E,C en cas d'egalite.
    public string CodeHolland { get; set; } = string.Empty;

    // Echelle de fiabilite : 6 des 30 paires de base sont reposees a l'identique plus loin
    // dans le round 1 (jamais reformulees), sans en reveler la dimension. Nombre de ces
    // paires ou la meme option a ete choisie aux deux occurrences - jamais compte dans les
    // scores par dimension.
    public int NombrePairesCoherentes { get; set; }
    public int NombrePairesControle { get; set; }

    // Round 2 (departage adaptatif) : renseignes uniquement si les 2 dimensions les plus
    // proches en score au round 1 etaient assez proches pour meriter d'etre departagees
    // (cf. RiasecService.SeuilDepartage). Null si le round 1 seul suffisait a trancher.
    public string? DepartageDimensionA { get; set; }
    public string? DepartageDimensionB { get; set; }
    public string? DepartageGagnant { get; set; }

    public DateTime CompleteLe { get; set; } = DateTime.UtcNow;
}

namespace Application.Common.Interfaces;

// Option d'une paire a choix force : represente toujours un item reel de l'O*NET Interest
// Profiler Short Form (jamais de texte invente) - cf. RiasecService.
public sealed class RiasecOptionInfo
{
    public int NumeroQuestion { get; set; } // 1 a 60, identifie l'item reel
    public string Texte { get; set; } = string.Empty;
}

// Paire a choix force : la personne doit choisir OptionA ou OptionB, jamais laquelle
// mesure quelle dimension (presentation "en aveugle").
public sealed class RiasecPaireInfo
{
    public int PaireId { get; set; }
    public RiasecOptionInfo OptionA { get; set; } = null!;
    public RiasecOptionInfo OptionB { get; set; } = null!;
}

// Round 2 (departage adaptatif) : ne se declenche que si les 2 dimensions les plus
// proches en score apres le round 1 sont suffisamment proches pour meriter d'etre
// departagees (cf. RiasecService.SeuilDepartage). Les paires reutilisent uniquement des
// items "gagnants" du round 1 - jamais de contenu invente.
public sealed class RiasecRound2Info
{
    public List<RiasecPaireInfo> Paires { get; set; } = [];
    public string DimensionA { get; set; } = string.Empty;
    public string DimensionB { get; set; } = string.Empty;
}

public sealed class RiasecDimensionInfo
{
    public string Code { get; set; } = string.Empty; // "R"
    public string Nom { get; set; } = string.Empty; // "Réaliste"
    public string Description { get; set; } = string.Empty;
    public int Score { get; set; } // 0 a 10
    // Exemples de metiers frequemment associes a cette dimension dans le modele de
    // Holland - illustratif, pas une liste exhaustive ni un outil d'orientation
    // professionnelle a lui seul.
    public List<string> Metiers { get; set; } = [];
}

public sealed class RiasecResultatInfo
{
    public List<RiasecDimensionInfo> Dimensions { get; set; } = []; // ordre fixe R,I,A,S,E,C
    public string CodeHolland { get; set; } = string.Empty; // ex. "SIA"
    public int NombrePairesCoherentes { get; set; }
    public int NombrePairesControle { get; set; }
    // Renseignes uniquement si un round 2 de departage a ete declenche.
    public string? DepartageDimensionA { get; set; }
    public string? DepartageDimensionB { get; set; }
    public string? DepartageGagnant { get; set; }
    public DateTime CompleteLe { get; set; }
}

// Test psychotechnique RIASEC (modele de Holland), a choix force par paires : pour chaque
// paire, la personne choisit l'activite qui lui correspond le plus entre deux items de
// dimensions differentes - jamais une question a bonne/mauvaise reponse (c'est un test
// d'interets, pas une validation de competence). Contenu traduit depuis l'O*NET Interest
// Profiler Short Form (U.S. Department of Labor), sous licence CC BY 4.0 - cf.
// RiasecService. Deux mecanismes de qualite de mesure, tous deux construits uniquement a
// partir du contenu reel deja traduit (jamais de question inventee) :
//   - presentation "en aveugle" + items de controle repetes a l'identique (fiabilite) ;
//   - round 2 adaptatif qui refait s'affronter les items gagnants des deux dimensions les
//     plus proches en score, pour mieux les departager (comparaison par paires).
// Le score est toujours recalcule cote serveur a partir des reponses brutes.
public interface IRiasecService
{
    // Round 1 : 36 paires (30 de base + 6 de controle de coherence), en ordre fixe et
    // melange - jamais de dimension exposee cote client.
    List<RiasecPaireInfo> GetPairesRound1();

    // A partir des reponses du round 1 (cle = PaireId 1-36, valeur = NumeroQuestion
    // choisi), determine si un round 2 de departage est necessaire. Renvoie null si les
    // reponses sont incompletes/invalides, ou un RiasecRound2Info avec Paires vide si
    // aucun departage n'est necessaire (round 1 suffisamment tranche).
    RiasecRound2Info? PreparerRound2(Dictionary<int, int> reponsesRound1);

    // Dernier resultat en date pour cet utilisateur, ou null s'il n'a jamais passe le test.
    Task<RiasecResultatInfo?> GetDernierResultatAsync(string utilisateurId);

    // Soumission finale : reponsesRound1 (36 entrees attendues), et reponsesRound2 (les
    // paires effectivement proposees par PreparerRound2 - vide si aucun round 2).
    Task<(bool Success, string? ErrorMessage, RiasecResultatInfo? Resultat)> RepondreAsync(
        string utilisateurId, Dictionary<int, int> reponsesRound1, Dictionary<int, int> reponsesRound2);
}

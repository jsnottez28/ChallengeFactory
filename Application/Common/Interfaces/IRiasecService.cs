namespace Application.Common.Interfaces;

// SlotId identifie un emplacement d'affichage/reponse (1 a 66 : 60 items + 6 items de
// controle de coherence) - jamais la dimension mesuree, presentee "en aveugle" pour
// limiter le biais de desirabilite sociale (cf. RiasecService.Slots).
public sealed class RiasecQuestionInfo
{
    public int SlotId { get; set; }
    public string Texte { get; set; } = string.Empty;
}

public sealed class RiasecDimensionInfo
{
    public string Code { get; set; } = string.Empty; // "R"
    public string Nom { get; set; } = string.Empty; // "Réaliste"
    public string Description { get; set; } = string.Empty;
    public int Score { get; set; } // 0 a 10
}

public sealed class RiasecResultatInfo
{
    public List<RiasecDimensionInfo> Dimensions { get; set; } = []; // ordre fixe R,I,A,S,E,C
    public string CodeHolland { get; set; } = string.Empty; // ex. "SIA"
    public int NombrePairesCoherentes { get; set; }
    public int NombrePairesControle { get; set; }
    public DateTime CompleteLe { get; set; }
}

// Test psychotechnique RIASEC (modele de Holland) : 60 activites a cocher ("j'aimerais
// faire ça"), reparties en 6 dimensions de 10 items chacune - jamais une question a
// bonne/mauvaise reponse (c'est un test d'interets, pas une validation de competence).
// Contenu traduit depuis l'O*NET Interest Profiler Short Form (U.S. Department of Labor),
// sous licence CC BY 4.0 - cf. RiasecService. Le score est toujours recalcule cote serveur
// a partir des reponses brutes. Presentation "en aveugle" : ni la dimension mesuree par
// chaque item, ni l'ordre par dimension, ne sont exposes cote client - GetQuestions()
// renvoie un ordre fixe deja melange incluant des items de controle de coherence (cf.
// RiasecQuestionInfo, RiasecResultatInfo.NombrePairesCoherentes).
public interface IRiasecService
{
    // Contenu statique (66 emplacements : 60 items + 6 de controle) - pas d'acces base de
    // donnees.
    List<RiasecQuestionInfo> GetQuestions();

    // Dernier resultat en date pour cet utilisateur, ou null s'il n'a jamais passe le test.
    Task<RiasecResultatInfo?> GetDernierResultatAsync(string utilisateurId);

    // reponses : SlotId (cf. GetQuestions) des emplacements coches. Au moins une case
    // cochee est requise.
    Task<(bool Success, string? ErrorMessage, RiasecResultatInfo? Resultat)> RepondreAsync(string utilisateurId, HashSet<int> reponses);
}

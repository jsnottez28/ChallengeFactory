namespace Application.Common.Interfaces;

public sealed class RiasecQuestionInfo
{
    public int NumeroQuestion { get; set; } // 1 a 60
    public string Dimension { get; set; } = string.Empty; // "R","I","A","S","E","C"
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
    public DateTime CompleteLe { get; set; }
}

// Test psychotechnique RIASEC (modele de Holland) : 60 activites a cocher ("j'aimerais
// faire ça"), reparties en 6 dimensions de 10 items chacune - jamais une question a
// bonne/mauvaise reponse (c'est un test d'interets, pas une validation de competence).
// Contenu traduit depuis l'O*NET Interest Profiler Short Form (U.S. Department of Labor),
// sous licence CC BY 4.0 - cf. RiasecService. Le score est toujours recalcule cote serveur
// a partir des reponses brutes.
public interface IRiasecService
{
    // Contenu statique (60 questions) - pas d'acces base de donnees.
    List<RiasecQuestionInfo> GetQuestions();

    // Dernier resultat en date pour cet utilisateur, ou null s'il n'a jamais passe le test.
    Task<RiasecResultatInfo?> GetDernierResultatAsync(string utilisateurId);

    // reponses : cle = numero de question (1 a 60), presente uniquement pour les items
    // coches (valeur toujours true). Au moins une case cochee est requise.
    Task<(bool Success, string? ErrorMessage, RiasecResultatInfo? Resultat)> RepondreAsync(string utilisateurId, HashSet<int> reponses);
}

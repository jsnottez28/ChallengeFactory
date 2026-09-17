namespace Application.Common.Interfaces;

public sealed class DiscQuestionOptionInfo
{
    public string Lettre { get; set; } = string.Empty; // "a" a "d"
    public string Texte { get; set; } = string.Empty;
}

public sealed class DiscQuestionInfo
{
    public int NumeroQuestion { get; set; }
    public List<DiscQuestionOptionInfo> Options { get; set; } = [];
}

public sealed class DiscResultatInfo
{
    public int ScoreD { get; set; }
    public int ScoreI { get; set; }
    public int ScoreS { get; set; }
    public int ScoreC { get; set; }
    public string ProfilDominant { get; set; } = string.Empty; // "D", "I", "S" ou "C"
    public string ProfilDominantNom { get; set; } = string.Empty; // "Dominant", "Influent", ...
    public string ProfilDominantDescription { get; set; } = string.Empty;
    public List<string> ProfilDominantTraits { get; set; } = [];
    public DateTime CompleteLe { get; set; }
}

// Test psychotechnique DISC : 25 questions a choix unique parmi 4 adjectifs, jamais un
// QCM a bonne/mauvaise reponse (c'est un test de personnalite, pas une validation de
// competence - hors du champ du principe Manifeste sur le QCM). Le score est toujours
// recalcule cote serveur a partir des reponses brutes (cf. DiscService), jamais fait
// confiance a un score calcule cote client.
public interface IDiscService
{
    // Contenu statique (questions + options) - pas d'acces base de donnees.
    List<DiscQuestionInfo> GetQuestions();

    // Dernier resultat en date pour cet utilisateur, ou null s'il n'a jamais passe le test.
    Task<DiscResultatInfo?> GetDernierResultatAsync(string utilisateurId);

    // reponses : cle = numero de question (1 a 25), valeur = lettre choisie ("a" a "d").
    // Doit couvrir l'integralite des 25 questions. Enregistre le resultat, le lie au
    // stagiaire et lui envoie un email avec son profil.
    Task<(bool Success, string? ErrorMessage, DiscResultatInfo? Resultat)> RepondreAsync(string utilisateurId, Dictionary<int, string> reponses);
}

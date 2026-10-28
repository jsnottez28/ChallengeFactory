namespace Application.Common.Interfaces;

public sealed class KilmannQuestionOptionInfo
{
    public string Lettre { get; set; } = string.Empty; // "A" ou "B"
    public string Texte { get; set; } = string.Empty;
}

public sealed class KilmannQuestionInfo
{
    public int NumeroQuestion { get; set; }
    public List<KilmannQuestionOptionInfo> Options { get; set; } = [];
}

public sealed class KilmannModeInfo
{
    public string Code { get; set; } = string.Empty; // "Rivaliser", "Collaborer", "Compromis", "Eviter" ou "Ceder"
    public string Nom { get; set; } = string.Empty;
    public int Score { get; set; }
    public string Description { get; set; } = string.Empty;
    public List<string> UtilisationPositive { get; set; } = [];
    public List<string> UtilisationNegative { get; set; } = [];
}

public sealed class KilmannResultatInfo
{
    public List<KilmannModeInfo> Modes { get; set; } = [];
    public string ProfilDominant { get; set; } = string.Empty; // Code du mode au score le plus eleve
    public DateTime CompleteLe { get; set; }
}

// Test de gestion des conflits Thomas-Kilmann (TKI) : 30 enonces a choix force entre deux
// comportements (A/B), jamais un QCM a bonne/mauvaise reponse (test de personnalite/mode de
// gestion des conflits, pas une validation de competence - hors du champ du principe
// Manifeste sur le QCM). Le score est toujours recalcule cote serveur a partir des reponses
// brutes (cf. KilmannService), jamais fait confiance a un score calcule cote client.
public interface IKilmannService
{
    // Contenu statique (questions + options) - pas d'acces base de donnees.
    List<KilmannQuestionInfo> GetQuestions();

    // Calcule le resultat a partir des reponses brutes, sans aucun acces base de donnees ni
    // envoi d'email - utilise en interne par RepondreAsync (utilisateur connecte : persiste
    // et envoie un email) et directement par le parcours anonyme (resultat affiche a la
    // volee, jamais enregistre tant qu'aucun compte n'existe).
    // reponses : cle = numero de question (1 a 30), valeur = lettre choisie ("A" ou "B").
    (bool Success, string? ErrorMessage, KilmannResultatInfo? Resultat) Calculer(Dictionary<int, string> reponses);

    // Dernier resultat en date pour cet utilisateur, ou null s'il n'a jamais passe le test.
    Task<KilmannResultatInfo?> GetDernierResultatAsync(string utilisateurId);

    // Doit couvrir l'integralite des 30 questions. Enregistre le resultat, le lie au
    // stagiaire et lui envoie un email avec son profil.
    Task<(bool Success, string? ErrorMessage, KilmannResultatInfo? Resultat)> RepondreAsync(string utilisateurId, Dictionary<int, string> reponses);
}

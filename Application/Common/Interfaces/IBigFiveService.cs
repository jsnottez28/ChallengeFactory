namespace Application.Common.Interfaces;

// Un item du questionnaire (affirmation a evaluer sur une echelle de 1 a 5) - le sens de
// notation (normal/inverse) reste cote serveur, jamais expose au client. Jamais de texte
// invente : toujours un des 116 items reels traduits depuis l'IPIP-NEO-120 (cf.
// BigFiveService).
public sealed class BigFiveQuestionInfo
{
    public int NumeroQuestion { get; set; } // 1 a 116
    public string Texte { get; set; } = string.Empty;
}

public sealed class BigFiveFacetteInfo
{
    public string Code { get; set; } = string.Empty; // "N1"
    public string Nom { get; set; } = string.Empty; // "Anxiété"
    public string DomaineCode { get; set; } = string.Empty; // "N"
    public int Score { get; set; } // 4 a 20
}

public sealed class BigFiveDomaineInfo
{
    public string Code { get; set; } = string.Empty; // "N", "E", "O", "A" ou "C"
    public string Nom { get; set; } = string.Empty; // "Névrosisme"
    public int Score { get; set; }
    public int ScoreMin { get; set; } // 20 (Ouverture) ou 24 (les 4 autres)
    public int ScoreMax { get; set; } // 100 (Ouverture) ou 120 (les 4 autres)
    // Position du score dans l'intervalle [ScoreMin, ScoreMax], en pourcentage (0-100) -
    // permet un affichage comparable entre Ouverture (5 facettes) et les autres domaines (6
    // facettes) malgre des echelles de longueur differente.
    public int PourcentagePosition { get; set; }
    // "Faible", "Modéré" ou "Élevé" - tiers de l'intervalle theorique [ScoreMin, ScoreMax],
    // jamais une norme statistique construite sur un echantillon (cf. BigFiveService).
    public string Niveau { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty; // selon Niveau
    public List<BigFiveFacetteInfo> Facettes { get; set; } = [];
}

// Synthese interpretative du profil - dimensionnelle (jamais un "type" unique a la MBTI) :
// le Big Five decrit une position sur 5 echelles continues, pas une case fermee. Construite
// a partir des 5 scores de domaine plutot que d'un paragraphe type repete par domaine.
public sealed class BigFiveSyntheseInfo
{
    // Domaines E/O/A/C "Élevé", et Nevrosisme "Faible" (= stabilite emotionnelle) - jamais
    // une liste vide de sens, toujours ancree dans les scores reels.
    public List<string> PointsForts { get; set; } = [];
    // Nevrosisme "Élevé" uniquement - c'est le seul domaine du modele dont un score eleve
    // signale une plus grande reactivite au stress plutot qu'un trait "positif" ou
    // "negatif" en soi (cf. BigFiveService).
    public List<string> PointsVigilance { get; set; } = [];
    public string Resume { get; set; } = string.Empty;
}

public sealed class BigFiveResultatInfo
{
    public List<BigFiveDomaineInfo> Domaines { get; set; } = []; // ordre fixe N,E,O,A,C
    public BigFiveSyntheseInfo Synthese { get; set; } = new();
    public DateTime CompleteLe { get; set; }
}

// Test psychotechnique Big Five (modele OCEAN), a echelle de Likert en 5 points ("Très
// inexact" a "Très exact") par affirmation - jamais une question a bonne/mauvaise reponse
// (test de personnalite, pas une validation de competence, hors du champ du principe
// Manifeste sur le QCM). Contenu (116 items sur 120) traduit depuis l'IPIP-NEO-120 (Johnson,
// 2014), lui-meme issu de l'International Personality Item Pool (ipip.ori.org) - domaine
// public. La facette O6 (Liberalism, items sur la preference de vote) est exclue - donnee
// sensible RGPD (opinion politique), cf. BigFiveResultat. Le score est toujours recalcule
// cote serveur a partir des reponses brutes (cf. BigFiveService), jamais fait confiance a un
// score calcule cote client.
public interface IBigFiveService
{
    // Contenu statique (116 items) - pas d'acces base de donnees.
    List<BigFiveQuestionInfo> GetQuestions();

    // Dernier resultat en date pour cet utilisateur, ou null s'il n'a jamais passe le test.
    Task<BigFiveResultatInfo?> GetDernierResultatAsync(string utilisateurId);

    // reponses : cle = NumeroQuestion (1 a 116), valeur = note brute (1 a 5, "Très inexact"
    // a "Très exact"). Doit couvrir l'integralite des 116 items. Enregistre le resultat, le
    // lie a l'utilisateur et lui envoie un email avec son profil.
    Task<(bool Success, string? ErrorMessage, BigFiveResultatInfo? Resultat)> RepondreAsync(string utilisateurId, Dictionary<int, int> reponses);
}

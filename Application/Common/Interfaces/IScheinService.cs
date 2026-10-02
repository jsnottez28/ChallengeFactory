namespace Application.Common.Interfaces;

// Un item du questionnaire (affirmation a evaluer sur une echelle de 1 a 5) - toujours un
// des 45 items reels du test (cf. ScheinService), jamais de texte invente. L'ancre a
// laquelle appartient l'item reste cote serveur, jamais exposee au client (meme principe
// que BigFive/RIASEC).
public sealed class ScheinQuestionInfo
{
    public int NumeroQuestion { get; set; } // 1 a 45
    public string Texte { get; set; } = string.Empty;
}

public sealed class ScheinAncreInfo
{
    public string Code { get; set; } = string.Empty; // "TECH", "MG", "AUT", "SEC", "CRE", "CAU", "DEF", "VIE", "INTER"
    public string Nom { get; set; } = string.Empty; // "Ancre technique"
    public int Score { get; set; } // nominalement 5 a 25, +4 par choix prioritaire tombant sur cette ancre (cf. ScoreMax)
    public int ScoreMin { get; set; } // 5 (5 items x 1 point minimum)
    public int ScoreMax { get; set; } // 25 (5 items x 5 points maximum) - hors points bonus des choix prioritaires
    // Position du score dans l'intervalle [ScoreMin, ScoreMax], en pourcentage (0-100) - peut
    // depasser 100 si des choix prioritaires portent sur cette ancre (jamais tronque, pour ne
    // pas masquer un profil atypique - meme principe que BigFive).
    public int PourcentagePosition { get; set; }
    public string Description { get; set; } = string.Empty;
    public bool EstDominante { get; set; } // true si Score == le score le plus eleve des 9 ancres (egalites possibles)
}

public sealed class ScheinResultatInfo
{
    // Les 9 ancres, triees par score decroissant (egalites departagees par l'ordre TECH, MG,
    // AUT, SEC, CRE, CAU, DEF, VIE, INTER - l'ordre de la grille source).
    public List<ScheinAncreInfo> Ancres { get; set; } = [];
    // Resume interpretatif construit a partir de la ou des ancre(s) dominante(s) - jamais un
    // texte invariant, toujours derive du resultat reel (cf. ScheinService.ConstruireResume).
    public string Resume { get; set; } = string.Empty;
    public DateTime CompleteLe { get; set; }
}

// Test des ancres de carriere d'Edgar Schein (Schein, E. H. (1990), Career Anchors:
// Discovering Your Real Values, Pfeiffer & Company), adapte par Jean-Luc Cerdin (2007),
// S'expatrier en toute connaissance de cause, Eyrolles, avec l'ajout de la 9e ancre
// "internationale" - 45 affirmations sur une echelle de Likert en 5 points ("Tout a fait en
// desaccord" a "Tout a fait d'accord"), jamais une question a bonne/mauvaise reponse (test
// d'orientation de carriere, pas une validation de competence, hors du champ du principe
// Manifeste sur le QCM). Pas d'instrument officiel en acces libre par API (confirme par
// recherche - contrairement a l'IPIP-NEO ou au O*NET Interest Profiler, l'instrument
// original de Schein reste vendu par John Wiley & Sons). Les 45 affirmations et la grille de
// notation sont reprises de deux sources transmises par Jean-Sebastien Nottez, toutes deux
// presentees comme une adaptation/traduction de Schein (1990) par Cerdin (2007) -
// [LICENCE A CONFIRMER] : l'une (samadhicoaching.com, copyright 2019 Pascale Crustin) porte
// une mention de copyright explicite sur le document, sans autorisation de reutilisation
// commerciale verifiee aupres de l'auteure a ce stade.
//
// Chaque ancre est notee sur 5 affirmations (5 a 25 points). En complement du score Likert,
// une derniere etape reprend les memes 45 affirmations et demande de choisir les 3 qui
// correspondent le mieux a la personne : chaque choix ajoute 4 points a l'ancre de
// l'affirmation choisie (methode de notation decrite par Cerdin, 2007) - CONTRAIREMENT aux
// paires de fiabilite de BigFive/RIASEC, ce round MODIFIE reellement le score final, il ne
// sert pas uniquement de signal de qualite de passation.
//
// Le score est toujours recalcule cote serveur a partir des reponses brutes, jamais fait
// confiance a un score calcule cote client.
public interface IScheinService
{
    // Contenu statique (45 items) - pas d'acces base de donnees.
    List<ScheinQuestionInfo> GetQuestions();

    // Dernier resultat en date pour cet utilisateur, ou null s'il n'a jamais passe le test.
    Task<ScheinResultatInfo?> GetDernierResultatAsync(string utilisateurId);

    // reponses : cle = NumeroQuestion (1 a 45), valeur = note brute (1 a 5, "Tout a fait en
    // desaccord" a "Tout a fait d'accord"). Doit couvrir l'integralite des 45 items.
    // choixPrioritaires : les NumeroQuestion des 3 affirmations choisies comme les plus
    // vraies pour la personne (exactement 3, chacune ajoutant 4 points a son ancre).
    // Enregistre le resultat, le lie a l'utilisateur et lui envoie un email avec son profil.
    Task<(bool Success, string? ErrorMessage, ScheinResultatInfo? Resultat)> RepondreAsync(
        string utilisateurId, Dictionary<int, int> reponses, List<int> choixPrioritaires);
}

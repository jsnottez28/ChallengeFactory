namespace Application.Common.Interfaces;

// Un item du questionnaire (affirmation a evaluer sur une echelle de 1 a 5) - le sens de
// notation (normal/inverse) reste cote serveur, jamais expose au client. Jamais de texte
// invente : toujours un des 58 items reels traduits depuis l'IPIP-NEO-60 (cf.
// BigFiveService).
public sealed class BigFiveQuestionInfo
{
    public int NumeroQuestion { get; set; } // 1 a 58
    public string Texte { get; set; } = string.Empty;
}

// Option d'une paire de fiabilite : reprend toujours un des 58 items reels (jamais de texte
// invente) - cf. BigFiveService.GetPairesFiabilite.
public sealed class BigFiveFiabiliteOptionInfo
{
    public int NumeroQuestion { get; set; }
    public string Texte { get; set; } = string.Empty;
}

// Paire a choix force entre 2 facettes DIFFERENTES d'un meme domaine (ex. Anxiété vs
// Dépression, toutes deux du domaine Névrosisme) - la personne doit choisir OptionA ou
// OptionB, jamais laquelle mesure quelle facette (presentation "en aveugle"). Sert
// uniquement de signal de coherence (cf. BigFiveResultatInfo.NombrePairesCoherentes),
// jamais a modifier les scores de domaine/facette.
public sealed class BigFiveFiabilitePaireInfo
{
    public int PaireId { get; set; }
    public BigFiveFiabiliteOptionInfo OptionA { get; set; } = null!;
    public BigFiveFiabiliteOptionInfo OptionB { get; set; } = null!;
}

public sealed class BigFiveFacetteInfo
{
    public string Code { get; set; } = string.Empty; // "N1"
    public string Nom { get; set; } = string.Empty; // "Anxiété"
    public string DomaineCode { get; set; } = string.Empty; // "N"
    public decimal Score { get; set; } // nominalement 2 a 10
}

public sealed class BigFiveDomaineInfo
{
    public string Code { get; set; } = string.Empty; // "N", "E", "O", "A" ou "C"
    public string Nom { get; set; } = string.Empty; // "Névrosisme"
    public decimal Score { get; set; }
    public int ScoreMin { get; set; } // 10 (Ouverture) ou 12 (les 4 autres)
    public int ScoreMax { get; set; } // 50 (Ouverture) ou 60 (les 4 autres)
    // Position du score dans l'intervalle [ScoreMin, ScoreMax], en pourcentage (0-100) -
    // permet un affichage comparable entre Ouverture (5 facettes) et les autres domaines (6
    // facettes) malgre des echelles de longueur differente. Peut legerement deborder de
    // [0, 100] apres correction du biais d'acquiescement (cf. BigFiveService) - jamais
    // tronque, pour ne pas masquer un profil atypique.
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
    // Moyenne des 58 reponses brutes avant correction (proche de 3 = pas de tendance
    // particuliere a l'acquiescement) - cf. BigFiveResultat.IndiceAcquiescement.
    public decimal IndiceAcquiescement { get; set; }
    // Texte explicatif de l'indice d'acquiescement, genere selon son ecart a 3 (cf.
    // BigFiveService) - jamais invente a l'affichage, toujours derive de la vraie valeur.
    public string IndiceAcquiescementCommentaire { get; set; } = string.Empty;
    // Coherence entre l'echelle de Likert (58 items) et le round de fiabilite a choix force
    // (5 paires, une par domaine) : nombre de paires ou le choix force confirme la tendance
    // deja exprimee sur l'echelle de Likert pour les 2 memes items - signal de qualite de
    // passation, jamais utilise pour modifier les scores de domaine/facette (cf.
    // BigFiveService.CalculerCoherenceFiabilite).
    public int NombrePairesCoherentes { get; set; }
    public int NombrePairesControle { get; set; }
    public DateTime CompleteLe { get; set; }
}

// Test psychotechnique Big Five (modele OCEAN), a echelle de Likert en 5 points ("Très
// inexact" a "Très exact") par affirmation - jamais une question a bonne/mauvaise reponse
// (test de personnalite, pas une validation de competence, hors du champ du principe
// Manifeste sur le QCM). Contenu (58 items sur 60) traduit depuis l'IPIP-NEO-60
// (Maples-Keller et al., 2017), lui-meme issu de l'International Personality Item Pool
// (ipip.ori.org) - domaine public. La facette O6 (Liberalism, items sur la preference de
// vote et la croyance religieuse) est exclue - donnees sensibles RGPD (opinion politique,
// conviction religieuse), cf. BigFiveResultat.
//
// Les scores sont corriges du biais d'acquiescence (tendance a repondre de facon uniforme,
// independamment du contenu de chaque item) selon le principe des scores ipsatises
// (Soto, John, Gosling & Potter, 2008, Appendix A, cite dans l'article source de l'IPIP-NEO) :
// chaque reponse brute est recentree sur la moyenne des reponses de la personne avant d'etre
// recodee et sommee - cf. BigFiveService.CalculerValeurCorrigee.
//
// En complement (jamais en remplacement) du scoring Likert, un round de fiabilite a choix
// force (5 paires, une par domaine, croisant 2 facettes differentes du meme domaine) sert a
// detecter la desirabilite sociale (repondre pour "bien paraitre" plutot que sincerement) :
// contrairement a une echelle de Likert ou l'on peut repondre "5" a tout pour se donner le
// beau role, un choix force entre deux affirmations toutes deux positivement connotees
// oblige a un arbitrage reel. Le choix force ne modifie JAMAIS les scores de domaine/
// facette (pas de methode d'appariement par desirabilite validee scientifiquement pour ces
// items precis, contrairement aux paires RIASEC issues de l'O*NET) - il sert uniquement de
// signal de coherence affiche a la personne (cf. CalculerCoherenceFiabilite).
//
// Le score est toujours recalcule cote serveur a partir des reponses brutes, jamais fait
// confiance a un score calcule cote client.
public interface IBigFiveService
{
    // Contenu statique (58 items) - pas d'acces base de donnees.
    List<BigFiveQuestionInfo> GetQuestions();

    // 5 paires de fiabilite fixes (une par domaine) - toujours les memes items reels, jamais
    // de contenu invente ni de dependance aux reponses au round Likert (contrairement au
    // round 2 adaptatif de RIASEC).
    List<BigFiveFiabilitePaireInfo> GetPairesFiabilite();

    // Dernier resultat en date pour cet utilisateur, ou null s'il n'a jamais passe le test.
    Task<BigFiveResultatInfo?> GetDernierResultatAsync(string utilisateurId);

    // reponses : cle = NumeroQuestion (1 a 58), valeur = note brute (1 a 5, "Très inexact" a
    // "Très exact"). Doit couvrir l'integralite des 58 items. reponsesFiabilite : cle =
    // PaireId (cf. GetPairesFiabilite), valeur = NumeroQuestion choisi - doit couvrir les 5
    // paires. Enregistre le resultat, le lie a l'utilisateur et lui envoie un email avec son
    // profil.
    Task<(bool Success, string? ErrorMessage, BigFiveResultatInfo? Resultat)> RepondreAsync(
        string utilisateurId, Dictionary<int, int> reponses, Dictionary<int, int> reponsesFiabilite);
}

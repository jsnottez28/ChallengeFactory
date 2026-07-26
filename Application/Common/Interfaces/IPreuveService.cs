using Domain.Entities;

namespace Application.Common.Interfaces;

// Un fichier a enregistrer, converti depuis IFormFile au niveau du controleur (Application
// ne depend jamais d'ASP.NET Core - meme principe que la construction d'URL qui reste
// toujours au niveau appelant, cf. ICohorteService).
public sealed class FichierPreuveInput
{
    public string NomFichier { get; set; } = string.Empty;
    public Stream Contenu { get; set; } = Stream.Null;
    public long TailleOctets { get; set; }
}

public sealed class ValidationRecue
{
    public string ValideurNomComplet { get; set; } = string.Empty;
    public bool EstGestionnaire { get; set; }
    public string Decision { get; set; } = string.Empty;
    public string? Commentaire { get; set; }
    public DateTime Date { get; set; }
}

public sealed class PreuveFichierInfo
{
    public int Id { get; set; }
    public TypeFichierPreuve TypeFichier { get; set; }
    public string NomFichier { get; set; } = string.Empty;
    public long TailleOctets { get; set; }
}

// Vue complete, reservee a l'auteur de la Preuve (cf. prompt section 6) : fichiers,
// description, statut, et le fil COMPLET de tous les retours recus (pairs + Gestionnaire).
public sealed class PreuveDetailInfo
{
    public int Id { get; set; }
    public int ChallengeEtapeId { get; set; }
    public string TitreEtape { get; set; } = string.Empty;
    public string? Description { get; set; }
    public StatutPreuve Statut { get; set; }
    public DateTime DateDepot { get; set; }
    public List<PreuveFichierInfo> Fichiers { get; set; } = [];
    public List<ValidationRecue> Retours { get; set; } = [];
}

// Vue reservee au pair validateur (cf. prompt section 3 et 6) : uniquement les fichiers
// et la description - jamais le statut agrege, jamais les decisions/commentaires des
// autres pairs (independance du jugement).
public sealed class PreuveApercuPourPairInfo
{
    public int Id { get; set; }
    public int CohorteId { get; set; }
    public int ChallengeEtapeId { get; set; }
    public string AuteurNomComplet { get; set; } = string.Empty;
    public string TitreEtape { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<PreuveFichierInfo> Fichiers { get; set; } = [];

    // Uniquement pour re-afficher le formulaire pre-rempli si ce pair a deja vote et
    // vient corriger son propre avis (ce n'est PAS l'avis d'un autre pair).
    public DecisionValidationPair? MaDecisionPrecedente { get; set; }
    public string? MonCommentairePrecedent { get; set; }
}

public sealed class PreuveAValiderInfo
{
    public int PreuveId { get; set; }
    public string AuteurNomComplet { get; set; } = string.Empty;
    public string TitreEtape { get; set; } = string.Empty;
    public DateTime DateDepot { get; set; }
}

public sealed class PreuveEtapeInfo
{
    public int PreuveId { get; set; }
    public string AuteurNomComplet { get; set; } = string.Empty;
    public StatutPreuve Statut { get; set; }
    public DateTime DateDepot { get; set; }
    public int NombreDecisionsPairs { get; set; }
    public int NombreDecisionsValidePairs { get; set; }
}

public sealed class ResumeClotureEtapeInfo
{
    public int NumeroEtape { get; set; }
    public int NombreValideesParLesPairs { get; set; }
    public int NombreValideesDefinitivementDeja { get; set; }
    public int NombreSoumisesRestantes { get; set; }
    public int NombreSansPreuveDeposee { get; set; }
}

public sealed class PointsEvenementInfo
{
    public DateTime Date { get; set; }
    public TypePoints TypePoints { get; set; }
    public int Montant { get; set; }
    public MotifPoints Motif { get; set; }
    public int? NumeroEtape { get; set; }
}

public sealed class PointsParEtapeInfo
{
    public int NumeroEtape { get; set; }
    public string TitreEtape { get; set; } = string.Empty;
    public int XPSavoir { get; set; }
    public int PointsKarma { get; set; }
    public int PointsAssiduite { get; set; }
}

// Vue apprenant "Mes points et badges" (section 6) : jamais de comparaison a un autre
// membre sur cet ecran - uniquement les totaux et l'historique de CET utilisateur.
public sealed class PointsResumeInfo
{
    public int TotalXPSavoir { get; set; }
    public int TotalPointsKarma { get; set; }
    public int TotalPointsAssiduite { get; set; }
    public List<PointsParEtapeInfo> DetailParEtape { get; set; } = [];
}

public sealed class BadgeSocialInfo
{
    public TypeBadgeSocial TypeBadge { get; set; }
    public int NumeroEtape { get; set; }
    public string TitreEtape { get; set; } = string.Empty;
    public string ChallengeTitre { get; set; } = string.Empty;
    public DateTime DateAttribution { get; set; }
}

// Vue gestionnaire "Contributions de la cohorte" (section 7) - JAMAIS exposee a un
// apprenant (ni cet endpoint, ni une variante publique). Le taux de revue permet de
// reperer un membre qui ne participe pas a l'entraide, sans en faire un classement public.
public sealed class ContributionMembreInfo
{
    public string UtilisateurId { get; set; } = string.Empty;
    public string NomComplet { get; set; } = string.Empty;
    public int XPSavoir { get; set; }
    public int PointsKarma { get; set; }
    public int PointsAssiduite { get; set; }
    public int PreuvesEvalueesParCeMembre { get; set; }
    public int PreuvesAttenduesPourCeMembre { get; set; }
}

public interface IPreuveService
{
    // ---- Cote apprenant : depot ----

    // Cree la Preuve si elle n'existe pas encore pour (utilisateurId, challengeEtapeId),
    // sinon la modifie (ajoute/retire des fichiers, met a jour la description). Toute
    // modification remet le statut a Soumise et supprime les validations pairs deja
    // donnees (elles doivent revalider la version modifiee) - les validations
    // Gestionnaire restent (fil d'historique, cf. section 6). Refuse si la Preuve est
    // deja ValideeDefinitivement, ou si l'utilisateur n'est pas membre actif de la
    // cohorteId sur son etape courante.
    Task<(bool Success, string? ErrorMessage, int? PreuveId)> DeposerOuModifierAsync(
        string utilisateurId,
        int cohorteId,
        int challengeEtapeId,
        string? description,
        List<FichierPreuveInput> fichiersAAjouter,
        List<int>? fichierIdsARetirer);

    Task<PreuveDetailInfo?> GetMaPreuveAsync(string utilisateurId, int challengeEtapeId);

    // ---- Cote apprenant : validation par les pairs ----

    // File de travail : toutes les Preuves des AUTRES membres de la cohorte que ce pair
    // n'a pas encore personnellement evaluees (independamment du statut agrege), hors
    // Preuves deja finalisees (ValideeDefinitivement / NonValideeALaCloture).
    Task<List<PreuveAValiderInfo>> GetPreuvesAValiderAsync(string valideurId, int cohorteId);

    Task<PreuveApercuPourPairInfo?> GetApercuPourPairAsync(int preuveId, string valideurId);

    // Refuse si valideurId == auteur de la Preuve (controle serveur, jamais seulement
    // cote UI). Recalcule le statut agrege (seuil 50%, reversion possible) sauf si deja
    // ValideeDefinitivement (statut fige). Genere toujours des Points_Karma pour le pair,
    // que la decision soit Valide ou ARevoir. lienSuiviPreuve (construit par l'appelant)
    // alimente les notifications in-app (decision recue, et passage a ValideeParLesPairs)
    // et l'email "preuve validee par les pairs" (envoye une seule fois par franchissement
    // du seuil, jamais a chaque recalcul si le ratio oscille).
    Task<(bool Success, string? ErrorMessage)> ValiderParPairAsync(
        int preuveId, string valideurId, DecisionValidationPair decision, string? commentaire, string lienSuiviPreuve);

    // ---- Cote gestionnaire (droit PREUVE.VALIDER) ----

    Task<List<PreuveEtapeInfo>> GetPreuvesEtapeAsync(int cohorteId);

    // Vue complete (fichiers, description, fil des retours pairs + Gestionnaire) pour un
    // Gestionnaire/Coach/Chef de Projet (droit PREUVE.CONSULTER) - meme forme que
    // GetMaPreuveAsync mais sans restriction d'auteur, puisque l'acces est deja controle
    // par le droit au niveau du controleur (cf. correction A.1 : la liste "Preuves de
    // l'etape" n'exposait avant que des compteurs agreges, jamais les fichiers/le detail).
    Task<PreuveDetailInfo?> GetDetailPourGestionnaireAsync(int preuveId);

    Task<ResumeClotureEtapeInfo> GetResumeAvantClotureAsync(int cohorteId);

    // Valide -> ValideeDefinitivement immediat + XP_Savoir (une seule fois par Preuve).
    // Refuse -> repasse a Soumise, commentaire obligatoire. Refuse si la Preuve est deja
    // ValideeDefinitivement (figee). Notifie l'auteur dans les deux cas (lienSuiviPreuve).
    Task<(bool Success, string? ErrorMessage)> ValiderParGestionnaireAsync(
        int preuveId, string valideurId, DecisionValidationGestionnaire decision, string? commentaire, string lienSuiviPreuve);

    // Appele par ICohorteService.ValiderEtapeAsync au moment de la cloture d'une etape :
    // finalise les Preuves de cette etape - ValideeParLesPairs -> ValideeDefinitivement
    // (+XP_Savoir si pas deja attribue +Points_Assiduite), Soumise -> NonValideeALaCloture.
    // Jamais de double attribution pour une Preuve deja ValideeDefinitivement via 4.1.
    Task ClorePreuvesEtapeAsync(int cohorteId, int numeroEtape);

    // Calcule (Points_Karma depuis la precedente cloture d'etape, ou depuis le debut pour
    // la 1ere) le(s) membre(s) au plus haut total et leur attribue le badge Super Helper.
    // Idempotent. N'attribue rien si le total maximal est 0.
    Task AttribuerBadgeSuperHelperAsync(int cohorteId, int numeroEtapeCloturee);

    Task<List<ContributionMembreInfo>> GetContributionsCohorteAsync(int cohorteId);

    // ---- Points et badges (apprenant, jamais de comparaison) ----

    Task<PointsResumeInfo> GetMesPointsAsync(string utilisateurId);

    Task<List<PointsEvenementInfo>> GetMonHistoriquePointsAsync(string utilisateurId);

    Task<List<BadgeSocialInfo>> GetMesBadgesAsync(string utilisateurId);

    // ---- Fichiers ----

    // Controle d'acces : auteur de la Preuve, pair membre de la meme cohorte, ou
    // Gestionnaire/Coach/Chef de Projet (droit PREUVE.CONSULTER ou PREUVE.VALIDER) sur
    // n'importe quelle Cohorte dont il a la charge - jamais une URL publique directe.
    Task<(Stream Contenu, string NomFichier)?> TelechargerFichierAsync(int fichierId, string utilisateurId, bool aLeDroitAdmin);
}

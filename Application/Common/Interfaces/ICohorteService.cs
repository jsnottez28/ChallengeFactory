using Domain.Entities;

namespace Application.Common.Interfaces;

public sealed class CohorteInput
{
    public int ChallengeId { get; set; }
    public string Nom { get; set; } = string.Empty;
    public DateTime? DateLancement { get; set; }

    // Rempli uniquement en mode BtoB (cloisonnement, cf. CLAUDE.md).
    public int? OrganisationId { get; set; }
}

// DTO plutot que l'entite Cohorte : celle-ci vit dans Web.Data (references directes vers
// ApplicationUser/Organisation) et Application n'a pas de reference au projet Web.
public sealed class CohorteResume
{
    public int Id { get; set; }
    public int ChallengeId { get; set; }
    public string ChallengeTitre { get; set; } = string.Empty;
    public ModePlateforme ChallengeMode { get; set; }
    public string Nom { get; set; } = string.Empty;
    public DateTime? DateLancement { get; set; }
    public int EtapeCourante { get; set; }
    public int NombreEtapes { get; set; }
    public StatutCohorte Statut { get; set; }
    public int NombreMembres { get; set; }
    public int? OrganisationId { get; set; }
    public string? OrganisationNom { get; set; }
}

public sealed class DemandeEmbarquementInfo
{
    public int CohorteId { get; set; }
    public int ChallengeId { get; set; }
    public string ChallengeTitre { get; set; } = string.Empty;
    public int NombreDemandeurs { get; set; }
    public DateTime DateCreation { get; set; }
}

public sealed class CohorteMembreInfo
{
    public int Id { get; set; }
    public string UtilisateurId { get; set; } = string.Empty;
    public string NomComplet { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public MethodeAjoutMembre MethodeAjout { get; set; }
    public DateTime DateAjout { get; set; }
    public StatutUtilisateur StatutAcces { get; set; }
}

public sealed class CohorteEtapeValidationInfo
{
    public int NumeroEtape { get; set; }
    public string ValideParNomComplet { get; set; } = string.Empty;
    public DateTime ValideLe { get; set; }
}

// Visio planifiee pour introduire une etape d'une Cohorte (prompt "Visio planifiee par
// etape", section 1). Toujours creee via LancerAsync (etape 1) ou ValiderEtapeAsync
// (etape suivante), jamais via une action admin dediee.
public sealed class VisioEtapeInfo
{
    public int Id { get; set; }
    public int ChallengeEtapeId { get; set; }
    public int NumeroEtape { get; set; }
    public string TitreEtape { get; set; } = string.Empty;
    public DateTime DateHeure { get; set; }
    public string LienConnexion { get; set; } = string.Empty;
    public string Descriptif { get; set; } = string.Empty;
    public string PlanifieParNomComplet { get; set; } = string.Empty;
}

// Cote apprenant : une Cohorte a laquelle l'utilisateur appartient, tant qu'elle est
// Active. Disparait de "Mon parcours en cours" des qu'elle passe Terminee (les cartes
// restent visibles dans la bibliotheque, mais plus ici - voir prompt section 7.1).
public sealed class ParcoursEnCoursInfo
{
    public int CohorteId { get; set; }
    public string ChallengeTitre { get; set; } = string.Empty;
    public int ChallengeEtapeId { get; set; }
    public int NumeroEtape { get; set; }
    public string TitreEtape { get; set; } = string.Empty;
    public string? DefiIndividuel { get; set; }
    public List<CarteCompetence> Cartes { get; set; } = [];
}

public sealed class MembreImportInput
{
    public string Email { get; set; } = string.Empty;
    public string? Prenom { get; set; }
    public string? Nom { get; set; }
}

public sealed class ImportMembresRapport
{
    public int ComptesCrees { get; set; }
    public int ComptesExistantsRattaches { get; set; }
    public int DejaMembres { get; set; }
    public List<string> Erreurs { get; set; } = [];
}

public interface ICohorteService
{
    Task<List<CohorteResume>> GetAllAsync();

    Task<CohorteResume?> GetResumeAsync(int id);

    Task<List<CohorteMembreInfo>> GetMembresAsync(int id);

    Task<List<CohorteEtapeValidationInfo>> GetHistoriqueValidationsAsync(int id);

    // Le Challenge doit etre Publie pour pouvoir servir de modele a une Cohorte.
    Task<(bool Success, string? ErrorMessage, int? CohorteId)> CreateAsync(CohorteInput input);

    Task<(bool Success, string? ErrorMessage)> AjouterMembreManuelAsync(int cohorteId, string utilisateurId);

    // Cree les comptes manquants (invitation par token, jamais de mot de passe en clair),
    // rattache directement les comptes deja existants. N'envoie jamais d'email de
    // confirmation d'inscription - uniquement l'email d'activation pour un compte cree.
    // construireLienActivation(token) construit l'URL complete (cf. Url.Page côté
    // controleur) - le service ne construit jamais d'URL lui-meme.
    Task<ImportMembresRapport> ImporterMembresAsync(int cohorteId, List<MembreImportInput> membres, string gestionnaireId, Func<string, string> construireLienActivation);

    Task<(bool Success, string? ErrorMessage)> AutoInscrireAsync(int cohorteId, string utilisateurId);

    Task<(bool Success, string? ErrorMessage)> RenvoyerInvitationAsync(string utilisateurId, Func<string, string> construireLienActivation);

    // Ferme les inscriptions, passe Active, EtapeCourante = 1, attribue les cartes de
    // l'etape 1 et notifie tous les membres actuels (lienMonParcours construit par
    // l'appelant, cf. Url.Page - le service ne construit jamais d'URL lui-meme).
    // dateHeureVisio/lienConnexionVisio sont OBLIGATOIRES (prompt "Visio planifiee par
    // etape", section 1.2) : renvoie une erreur explicite si absents, plutot que de
    // lancer la Cohorte sans visio planifiee. descriptifVisio est optionnel : si vide,
    // le descriptif auto-genere (GenererDescriptifVisioAsync) est utilise tel quel.
    Task<(bool Success, string? ErrorMessage)> LancerAsync(
        int cohorteId, string gestionnaireId, string lienMonParcours,
        DateTime? dateHeureVisio, string? lienConnexionVisio, string? descriptifVisio);

    // Cree une ligne d'audit, puis avance EtapeCourante (+attribution+email etape) ou
    // cloture la Cohorte (+email de cloture) si c'etait la derniere etape.
    // dateHeureVisio/lienConnexionVisio sont OBLIGATOIRES sauf si cette validation cloture
    // la Cohorte (derniere etape : pas d'etape suivante a introduire, donc pas de visio
    // demandee - prompt section 1.2).
    Task<(bool Success, string? ErrorMessage)> ValiderEtapeAsync(
        int cohorteId, string gestionnaireId, string lienMonParcours, string lienBibliotheque,
        DateTime? dateHeureVisio, string? lienConnexionVisio, string? descriptifVisio);

    // ---- Visios par etape ----

    // Texte par defaut (agenda fixe en 3 temps, prompt section 1.3) pour l'etape donnee -
    // a afficher en apercu EDITABLE avant confirmation de LancerAsync/ValiderEtapeAsync
    // (le Gestionnaire peut l'ajuster librement avant de valider).
    Task<string> GenererDescriptifVisioAsync(int challengeEtapeId);

    // Visios deja planifiees pour une Cohorte (passees et a venir), triees par date.
    Task<List<VisioEtapeInfo>> GetVisiosAsync(int cohorteId);

    // La prochaine visio a venir pour une Cohorte (cote apprenant, "Mon parcours en
    // cours") - null si aucune visio a venir.
    Task<VisioEtapeInfo?> GetProchaineVisioAsync(int cohorteId);

    // Cote apprenant : renvoie [] si le compte n'a pas acces au contenu (Suspendu/En
    // attente de validation, cf. statut_acces_plateforme) - controle serveur, jamais
    // seulement masque cote UI.
    Task<List<ParcoursEnCoursInfo>> GetMesParcoursEnCoursAsync(string utilisateurId);

    // Uniquement si EnPreparation (jamais Lancee) : tant qu'elle n'a pas ete Lancee, aucune
    // carte n'a ete attribuee ni aucune etape validee via cette Cohorte, donc rien a
    // perdre du cote tracabilite - les membres deja ajoutes sont retires (cascade).
    Task<(bool Success, string? ErrorMessage)> SupprimerAsync(int id);

    // ---- Embarquement (prompt section H) ----

    // Cote apprenant (BtoC uniquement, meme restriction que AutoInscrireAsync) : si une
    // Cohorte Proposee existe deja pour ce Challenge, l'apprenant y est simplement ajoute ;
    // sinon une nouvelle Cohorte Proposee est creee avec lui comme premier membre. Ne rend
    // JAMAIS la Cohorte visible/utilisable directement (reste soumise a validation humaine,
    // cf. ValiderEmbarquementAsync).
    Task<(bool Success, string? ErrorMessage, int? CohorteId)> DemanderEmbarquementAsync(int challengeId, string utilisateurId);

    // Cote Gestionnaire : liste des demandes d'embarquement en attente (Cohortes Proposee),
    // avec le nombre de demandeurs actuels.
    Task<List<DemandeEmbarquementInfo>> GetDemandesEmbarquementAsync();

    // Proposee -> EnPreparation, avec une date de lancement obligatoire : la Cohorte devient
    // alors visible/ouverte a l'inscription publique comme n'importe quelle Cohorte
    // EnPreparation. Notifie ET envoie un email a chaque demandeur deja membre (ils sont
    // deja inscrits sur cette Cohorte depuis DemanderEmbarquementAsync, rien a re-inscrire).
    Task<(bool Success, string? ErrorMessage)> ValiderEmbarquementAsync(int cohorteId, string nom, DateTime dateLancement, string lienFormations);

    // Supprime la demande (jamais de Cohorte "fantome" qui trainerait en Proposee) et
    // notifie chaque demandeur (reutilise INotificationService, cf. prompt section B).
    Task<(bool Success, string? ErrorMessage)> RefuserEmbarquementAsync(int cohorteId, string lienCatalogue);
}

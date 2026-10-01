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

    // Collectif (par defaut) vs BilanCompetencesIndividuel - cf. FormatChallenge. Conditionne
    // l'affichage de l'entree "Personnaliser les cartes" sur la fiche Cohorte (reservee aux
    // parcours individuels, cf. CohorteMembreCarteSupplementaire).
    public FormatChallenge Format { get; set; } = FormatChallenge.Collectif;

    // Id du ChallengeEtape correspondant a EtapeCourante (null si la Cohorte n'a pas encore
    // d'etape courante valide, ex. jamais lancee) - evite d'avoir a refaire la resolution
    // (ChallengeId, NumeroEtape) -> ChallengeEtapeId cote vue/controleur.
    public int? ChallengeEtapeCouranteId { get; set; }
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

// Une carte personnalisee deja ajoutee pour un membre - affichee dans le picker
// (CohorteService.GetContextePersonnalisationCartesAsync) pour tracabilite (qui, quand).
public sealed class CarteSupplementaireInfo
{
    public int CarteCompetenceId { get; set; }
    public string CarteCode { get; set; } = string.Empty;
    public string CarteTitre { get; set; } = string.Empty;
    public string AjouteeParNomComplet { get; set; } = string.Empty;
    public DateTime AjouteeLe { get; set; }
}

// Une ligne de la liste des etapes d'un Challenge, pour le point d'entree de navigation de
// la personnalisation d'un membre (cf. CohortesController.PersonnalisationMembre) - permet au
// Coach de voir d'un coup d'oeil quelles etapes ont deja ete personnalisees, y compris les
// etapes suivantes pas encore atteintes par la Cohorte.
public sealed class PersonnalisationEtapeInfo
{
    public int ChallengeEtapeId { get; set; }
    public int NumeroEtape { get; set; }
    public string TitreEtape { get; set; } = string.Empty;
    public bool EstEtapeCourante { get; set; }
    public int NombreCartesPersonnalisees { get; set; }
    public bool AInstructionsPersonnalisees { get; set; }
}

// Contexte complet pour l'ecran de personnalisation des cartes d'un membre a une etape
// donnee (cf. CohortesController.CartesSupplementairesMembre). FormatAutorise reste porte
// par le DTO (et pas seulement verifie cote service a l'enregistrement) pour permettre a la
// vue d'afficher un message bloquant explicite si jamais ce point d'entree est atteint sur
// un Challenge Collectif (ne doit normalement pas arriver, le lien n'est affiche que si
// FormatAutorise - mais defense en profondeur).
public sealed class PersonnalisationCartesContexte
{
    public int CohorteId { get; set; }
    public int CohorteMembreId { get; set; }
    public string MembreNomComplet { get; set; } = string.Empty;
    public int ChallengeEtapeId { get; set; }
    public int NumeroEtape { get; set; }
    public string EtapeTitre { get; set; } = string.Empty;
    public bool FormatAutorise { get; set; }
    public List<int> CartesTemplateIds { get; set; } = [];
    public List<CarteSupplementaireInfo> CartesSupplementaires { get; set; } = [];

    // Texte partage du template (ChallengeEtape.DefiIndividuel), affiche en lecture seule pour
    // contexte - jamais modifie directement par cet ecran (cf. DefinirInstructionsPersonnaliseesMembreAsync).
    public string? DefiIndividuelPartage { get; set; }

    // Surcharge actuelle pour ce membre, si elle existe (cf. CohorteMembreEtapePersonnalisation) -
    // null si aucune surcharge n'est encore definie (l'apprenant voit alors DefiIndividuelPartage).
    public string? DefiIndividuelPersonnalise { get; set; }
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

    // Cartes ajoutees specifiquement pour ce membre (cf. CohorteMembreCarteSupplementaire) -
    // toujours vide sur un Challenge Collectif. Affichees separement de Cartes dans la vue
    // (section distincte "Vos cartes personnalisees"), jamais fusionnees dans la meme liste,
    // pour que l'apprenant comprenne que ce sont des ajouts specifiques a son parcours.
    public List<CarteCompetence> CartesPersonnalisees { get; set; } = [];
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
    // l'appelant, cf. Url.Page - le service ne construit jamais d'URL lui-meme). Envoie
    // aussi le questionnaire mi-parcours si l'etape 1 est deja l'etape mediane (Challenge a
    // tres peu d'etapes), cf. lienQuestionnaireMiParcours.
    Task<(bool Success, string? ErrorMessage)> LancerAsync(int cohorteId, string gestionnaireId, string lienMonParcours, string lienQuestionnaireMiParcours);

    // Refuse si l'etape en cours a des cartes attribuees et qu'au moins un emargement
    // n'est pas encore signe (suivi de l'execution Qualiopi - force a utiliser le circuit
    // emargement plutot que de le laisser optionnel, cf. IEmargementService). Sinon, cree
    // une ligne d'audit, puis avance EtapeCourante (+attribution+email etape, et envoi du
    // questionnaire mi-parcours si cette etape est l'etape mediane du Challenge, cf.
    // IQuestionnaireMiParcoursService) ou cloture la Cohorte (+email de cloture, qui
    // inclut desormais le lien vers l'attestation de fin de parcours, cf.
    // IAttestationService, + email de demande de satisfaction, cf. ISatisfactionService) si
    // c'etait la derniere etape.
    Task<(bool Success, string? ErrorMessage)> ValiderEtapeAsync(int cohorteId, string gestionnaireId, string lienMonParcours, string lienBibliotheque, string lienSatisfaction, string lienQuestionnaireMiParcours, string lienAttestation);

    // Cote apprenant : renvoie [] si le compte n'a pas acces au contenu (Suspendu/En
    // attente de validation, cf. statut_acces_plateforme) - controle serveur, jamais
    // seulement masque cote UI.
    Task<List<ParcoursEnCoursInfo>> GetMesParcoursEnCoursAsync(string utilisateurId);

    // ---- Personnalisation des cartes (parcours Bilan de competences individuel uniquement) ----

    // Renvoie null si le membre n'existe pas / n'appartient pas a cette Cohorte. Liste TOUTES
    // les etapes du Challenge (pas seulement l'etape courante de la Cohorte) : permet au Coach
    // de preparer/ajuster les etapes suivantes en avance, pas seulement celle en cours.
    Task<List<PersonnalisationEtapeInfo>?> GetEtapesPersonnalisationAsync(int cohorteId, int cohorteMembreId);

    // Renvoie null si le membre ou l'etape n'existe pas / n'appartient pas a cette Cohorte.
    // FormatAutorise = false si le Challenge de la Cohorte n'est pas
    // BilanCompetencesIndividuel - a verifier cote vue avant d'afficher le formulaire
    // d'edition (le formulaire de recherche/ajout reste utilisable en lecture pour inspection,
    // mais DefinirCartesSupplementairesMembreAsync refusera toute ecriture).
    Task<PersonnalisationCartesContexte?> GetContextePersonnalisationCartesAsync(int cohorteId, int cohorteMembreId, int challengeEtapeId);

    // Remplace l'ensemble des cartes supplementaires de ce membre pour cette etape (meme
    // logique "remplacement total" que DefinirCartesEtapeAsync sur le template partage,
    // volontairement SANS verrou d'architecture : contrairement au template, la
    // personnalisation individuelle doit rester modifiable meme une fois la Cohorte Active,
    // puisque le diagnostic d'un bilan de competences peut evoluer en cours de parcours).
    // Echoue si le Challenge de la Cohorte n'est pas BilanCompetencesIndividuel (verification
    // serveur, jamais seulement masque cote UI - cf. CLAUDE.md, principe Manifeste "l'equipe
    // avant l'individu").
    Task<(bool Success, string? ErrorMessage)> DefinirCartesSupplementairesMembreAsync(int cohorteMembreId, int challengeEtapeId, List<int> carteCompetenceIds, string gestionnaireId);

    // Remplace, pour ce membre uniquement, le Defi individuel affiche (cf.
    // CohorteMembreEtapePersonnalisation) - le texte partage du template
    // (ChallengeEtape.DefiIndividuel) n'est jamais modifie, et continue de s'afficher pour tous
    // les autres membres/Cohortes issus du meme Challenge. instructionsPersonnalisees null ou
    // vide supprime la surcharge (l'apprenant revoit alors le texte partage). Meme garde-fou
    // que DefinirCartesSupplementairesMembreAsync : echoue si le Challenge n'est pas
    // BilanCompetencesIndividuel.
    Task<(bool Success, string? ErrorMessage)> DefinirInstructionsPersonnaliseesMembreAsync(int cohorteMembreId, int challengeEtapeId, string? instructionsPersonnalisees, string gestionnaireId);

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

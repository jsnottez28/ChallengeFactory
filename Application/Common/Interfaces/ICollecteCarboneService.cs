using Domain.Entities;

namespace Application.Common.Interfaces;

// Un element de la collecte d'un Bilan Carbone(R) : une donnee a obtenir du client, sur un
// poste/sous-poste du tableur ABC V9.0, avec la piece justificative qui la porte et le
// referent qui la detient. Sert a la fois a la check-list (vue client et Gestionnaire) et
// au tableur de collecte .xlsx (une ligne par element). Les champs Valeur/TypeDonnee/
// Source ne sont remplis que pour une donnee deja connue (pre-remplie depuis le
// Pre-diagnostic).
public sealed class ElementCollecteInfo
{
    // Cle stable (Poste+SousPoste+Donnee), generee par ChecklistCollecteBuilder - sert a
    // retrouver quelles lignes le client a cochees comme "saisies" (cf.
    // DossierCarbone.ElementsCochesJson), jamais un index de liste (qui changerait si
    // l'ordre ou le nombre de lignes evolue).
    public string Cle { get; set; } = string.Empty;
    public bool EstCoche { get; set; }

    public PosteCollecte Poste { get; set; }
    public string SousPoste { get; set; } = string.Empty;
    public string Donnee { get; set; } = string.Empty;
    public string Unite { get; set; } = string.Empty;
    public string PieceJustificative { get; set; } = string.Empty;
    public string Referent { get; set; } = string.Empty;

    // Pourquoi cet element est demande (ex. "Réponse : climatisation = Oui",
    // "Pré-diagnostic : Électricité renseignée") - affiche au Gestionnaire, utile pour
    // expliquer la check-list au client.
    public string Declencheur { get; set; } = string.Empty;

    public decimal? Valeur { get; set; }

    // Une des 5 valeurs de la liste "Type" de la Matrice de collecte ABC : Physique,
    // Comptable, Extrapolée, Statistique, Approchée.
    public string? TypeDonnee { get; set; }
    public string? Source { get; set; }
}

public sealed class DossierCarboneInfo
{
    public int Id { get; set; }
    public int PreBilanCarboneId { get; set; }
    public string RaisonSociale { get; set; } = string.Empty;
    public DateTime OuvertLe { get; set; }

    public string ClientUtilisateurId { get; set; } = string.Empty;
    public string ClientEmail { get; set; } = string.Empty;
    public string? ClientPrenom { get; set; }

    // Vrai des que le client a defini son mot de passe (invitation utilisee).
    public bool CompteActive { get; set; }

    public StatutQuestionnaireCollecte? StatutQuestionnaire { get; set; }
    public DateTime? QuestionnaireEnvoyeLe { get; set; }
    public DateTime? QuestionnaireSoumisLe { get; set; }

    public Dictionary<string, string> Reponses { get; set; } = [];

    public string? TableurDeposeNomFichier { get; set; }
    public DateTime? TableurDeposeLe { get; set; }
    public int? TableurDeposeAvancementPourcent { get; set; }
    public bool? CollecteDeclareeTerminee { get; set; }
}

// Dossier client carbone : ouverture d'un compte ClientCarbone a partir d'un
// Pre-diagnostic, envoi du questionnaire de collecte, saisie par le client dans son
// espace, puis production de la check-list et du tableur de collecte. Toutes les actions
// Gestionnaire prennent l'Id du Pre-diagnostic (point d'entree de la fiche CRM) ; toutes
// les actions client prennent l'Id de l'utilisateur connecte (jamais un Id de dossier
// fourni par le client : un client ne voit que SON dossier).
public interface ICollecteCarboneService
{
    Task<DossierCarboneInfo?> GetDossierParPreBilanAsync(int preBilanId);

    // Cree le compte ClientCarbone (sans mot de passe) et le dossier, puis envoie l'email
    // de bienvenue (identifiant + lien pour definir le mot de passe). Refuse si un compte
    // Participant existe deja avec cet email : les deux usages restent separes.
    Task<(bool Success, string? ErrorMessage)> OuvrirCompteClientAsync(int preBilanId, string gestionnaireId, Func<string, string> construireLienActivation);

    // Meme mecanique, mais declenchee par le prospect lui-meme juste apres son depot (pas
    // de Gestionnaire a l'origine - cf. DossierCarbone.OuvertParId qui reste null).
    // L'appelant doit s'assurer que preBilanId correspond bien au Pre-diagnostic que ce
    // visiteur vient de deposer (ex. via TempData), jamais a un Id fourni librement par le
    // client dans l'URL/le formulaire.
    Task<(bool Success, string? ErrorMessage)> OuvrirCompteClientSelfServiceAsync(int preBilanId, Func<string, string> construireLienActivation);

    // Invalide l'ancienne invitation et en renvoie une nouvelle (lien expire, email perdu).
    Task<(bool Success, string? ErrorMessage)> RenvoyerInvitationAsync(int preBilanId, Func<string, string> construireLienActivation);

    // Cree (ou rouvre, s'il etait deja soumis) le questionnaire de collecte et envoie au
    // client l'email contenant le lien vers son espace.
    Task<(bool Success, string? ErrorMessage)> EnvoyerQuestionnaireAsync(int preBilanId, string lienQuestionnaire);

    Task<DossierCarboneInfo?> GetDossierClientAsync(string clientUtilisateurId);

    // Enregistre les reponses du client (brouillon ou soumission definitive).
    Task<(bool Success, string? ErrorMessage)> EnregistrerReponsesAsync(string clientUtilisateurId, Dictionary<string, string> reponses, bool soumettre);

    // Check-list des elements a collecter, deduite du Pre-diagnostic et des reponses au
    // questionnaire (si deja remplies). ElementCollecteInfo.EstCoche reflete les cases
    // cochees par le client (cf. EnregistrerElementsCochesAsync).
    Task<List<ElementCollecteInfo>> GetChecklistAsync(int dossierId);

    // Enregistre les cles (ElementCollecteInfo.Cle) des lignes que le client declare avoir
    // deja saisies dans son fichier - remplace integralement la selection precedente (pas
    // un ajout cumulatif), meme principe que EnregistrerReponsesAsync.
    Task<(bool Success, string? ErrorMessage)> EnregistrerElementsCochesAsync(string clientUtilisateurId, IReadOnlyCollection<string> elementsCoches);

    // Tableur de collecte .xlsx (colonnes de la Matrice de collecte ABC, pre-rempli).
    // lienEspaceClient (optionnel) : affiche dans la feuille "Lisez-moi" un lien de retour
    // vers l'Espace Client, pour suivre sa progression et retrouver ce fichier en ligne.
    Task<(byte[] Contenu, string NomFichier)?> GenererTableurAsync(int preBilanId, string? lienEspaceClient = null);

    // Depot par le client de son tableur de collecte rempli - remplace le depot precedent
    // (un seul conserve, jamais un historique). Recalcule l'avancement estimatif a partir
    // du contenu (cf. TableurCollecteAvancementCalculator). Refuse si le fichier n'est pas
    // un classeur Excel valide.
    Task<(bool Success, string? ErrorMessage)> DeposerTableurRempliAsync(string clientUtilisateurId, Stream contenu, string nomFichier, bool collecteDeclareeTerminee);

    // Cote Gestionnaire : le dernier fichier depose par le client, tel quel (pas
    // regenere) - null si aucun depot n'a encore ete fait.
    Task<(Stream Contenu, string NomFichier)?> TelechargerDernierTableurDeposeAsync(int preBilanId);
}

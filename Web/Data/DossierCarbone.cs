using Domain.Entities;

namespace Web.Data;

// Dossier client carbone : point d'ancrage de l'espace client ouvert par un Gestionnaire
// a partir d'un Pre-diagnostic carbone (un dossier par Pre-diagnostic, jamais plus).
// Relie le Pre-diagnostic (donnees deja saisies par le prospect), le compte ClientCarbone
// qui y accede, et le questionnaire de collecte du Bilan Carbone(R). C'est aussi sur ce
// dossier que viendront se rattacher plus tard les plans d'action de decarbonation.
public class DossierCarbone
{
    public int Id { get; set; }

    public int PreBilanCarboneId { get; set; }
    public PreBilanCarbone PreBilanCarbone { get; set; } = null!;

    // Compte TypeCompte.ClientCarbone qui accede a ce dossier depuis son espace client.
    public string ClientUtilisateurId { get; set; } = string.Empty;
    public ApplicationUser ClientUtilisateur { get; set; } = null!;

    // Recopiee du Pre-diagnostic a l'ouverture, puis modifiable independamment (le
    // Pre-diagnostic reste une trace fidele de ce que le prospect a depose).
    public string RaisonSociale { get; set; } = string.Empty;

    public DateTime OuvertLe { get; set; } = DateTime.UtcNow;
    public string? OuvertParId { get; set; }

    public QuestionnaireCollecte? Questionnaire { get; set; }

    // Dernier tableur de collecte .xlsx depose par le client depuis son Espace Client -
    // un seul conserve (le precedent est supprime du stockage a chaque nouveau depot),
    // jamais un historique complet. CheminStockage : reference opaque
    // IPreuveFichierStockageService, jamais exposee telle quelle en dehors du service.
    public string? TableurDeposeCheminStockage { get; set; }
    public string? TableurDeposeNomFichier { get; set; }
    public DateTime? TableurDeposeLe { get; set; }

    // Avancement (0-100), recalcule a chaque depot Excel ou sauvegarde du formulaire web :
    // % des elements connus dont le Statut saisi (toutes sources fusionnees) vaut
    // Recu/Valide (cf. CollecteCarboneService.CalculerAvancement). Approximatif par nature
    // (le client peut renommer un onglet, deplacer des colonnes...) - jamais presente comme
    // une mesure exacte.
    public int? TableurDeposeAvancementPourcent { get; set; }

    // Declaration du client au moment du depot ("j'ai termine la collecte") - purement
    // informatif pour le Gestionnaire, ne bloque jamais un depot ulterieur.
    public bool? CollecteDeclareeTerminee { get; set; }

    // Cles (ElementCollecteInfo.Cle) des lignes de la check-list que le client a cochees
    // comme "deja saisies dans le fichier" - purement declaratif cote client, independant
    // de l'avancement estimatif calcule depuis le fichier depose (les deux se completent :
    // l'un est ce que le client affirme, l'autre ce qu'on peut verifier dans son fichier).
    public string ElementsCochesJson { get; set; } = "[]";

    // Valeurs reellement extraites du dernier tableur depose : dictionnaire Cle ->
    // ValeurSaisieFichier (JSON), fusionne de maniere incrementale a chaque depot (cf.
    // CollecteCarboneService.FusionnerValeurs) - une cellule vide dans un nouveau depot ne
    // efface jamais une valeur deja enregistree, elle est simplement ignoree.
    public string ValeursSaisiesJson { get; set; } = "{}";

    // Lignes presentes dans le dernier tableur depose mais non reconnues (cle sans
    // correspondance dans le catalogue - ex. ligne complementaire ajoutee par le client pour
    // un second site/fournisseur) : capturees telles quelles pour traitement ulterieur par
    // le Gestionnaire, jamais integrees a la check-list structuree. Remplacees en bloc (pas
    // cumulatif) a chaque nouveau depot - reflete toujours le dernier fichier.
    public string LignesSupplementairesJson { get; set; } = "[]";

    // Comptes supplementaires (collegues du contact principal ClientUtilisateur) qui peuvent
    // eux aussi consulter et completer ce dossier - une entreprise a souvent besoin de
    // plusieurs personnes (RH, facilities, comptabilite...) pour renseigner des postes
    // differents. Acces complet pour tous les collaborateurs, pas de perimetre par poste
    // (cf. discussion produit) ; jamais le contact principal lui-meme (deja via
    // ClientUtilisateurId).
    public ICollection<DossierCarboneCollaborateur> Collaborateurs { get; set; } = new List<DossierCarboneCollaborateur>();
}

// Un collaborateur invite sur un DossierCarbone, en plus du contact principal
// (DossierCarbone.ClientUtilisateurId). Invitation possible par le contact principal
// lui-meme (self-service, cf. EspaceClient/Index) ou par un Gestionnaire depuis la fiche
// admin - InviteParId garde la trace de qui a invite, utile si le collaborateur a une
// question sur pourquoi il a acces a ce dossier.
public class DossierCarboneCollaborateur
{
    public int Id { get; set; }

    public int DossierCarboneId { get; set; }
    public DossierCarbone DossierCarbone { get; set; } = null!;

    public string UtilisateurId { get; set; } = string.Empty;
    public ApplicationUser Utilisateur { get; set; } = null!;

    public DateTime InviteLe { get; set; } = DateTime.UtcNow;
    public string InviteParId { get; set; } = string.Empty;
}

// Questionnaire de collecte (questions de filtrage du Bilan Carbone(R)) rempli par le client
// depuis son espace. Les reponses sont stockees en JSON (code de question -> valeur) :
// le catalogue des questions vit dans le code (QuestionnaireCollecteCatalogue) et peut
// evoluer sans migration ; une question retiree du catalogue laisse simplement une
// reponse orpheline, ignoree.
public class QuestionnaireCollecte
{
    public int Id { get; set; }

    public int DossierCarboneId { get; set; }
    public DossierCarbone DossierCarbone { get; set; } = null!;

    public StatutQuestionnaireCollecte Statut { get; set; } = StatutQuestionnaireCollecte.Envoye;

    public string ReponsesJson { get; set; } = "{}";

    public DateTime EnvoyeLe { get; set; } = DateTime.UtcNow;
    public DateTime? ModifieLe { get; set; }
    public DateTime? SoumisLe { get; set; }
}

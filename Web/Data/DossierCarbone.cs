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

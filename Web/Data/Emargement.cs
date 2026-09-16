namespace Web.Data;

// Emargement numerique (suivi de l'execution, exigence Qualiopi) : une ligne par carte
// attribuee a un membre dans le cadre d'une Cohorte (CarteAttribution.OrigineType ==
// Challenge), qui trace la sollicitation ET la signature du membre certifiant sa
// participation a la seance ET l'acquisition de la carte. HeuresPresence /
// HeuresTravailPersonnel restent optionnelles : utilisees pour les parcours de type
// bilan de competences (seances accompagnees + travail en autonomie a la maison),
// laissees vides pour un parcours standard - aucune branche specifique au type de
// Challenge dans le modele, uniquement des champs optionnels.
public class Emargement
{
    public int Id { get; set; }

    public int CarteAttributionId { get; set; }
    public CarteAttribution CarteAttribution { get; set; } = null!;

    // Rempli a chaque (re)envoi de la demande de signature par le Gestionnaire.
    public DateTime? EnvoyeLe { get; set; }

    // Rempli une seule fois, par le membre lui-meme - jamais reinitialise (une signature ne
    // se retire pas), cf. IEmargementService.SignerAsync.
    public DateTime? SigneLe { get; set; }

    public decimal? HeuresPresence { get; set; }
    public decimal? HeuresTravailPersonnel { get; set; }
}

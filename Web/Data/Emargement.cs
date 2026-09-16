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

    // Reprise de EtapeVisio.DateVisio au moment de l'envoi (cf. EmargementService) : la
    // date de seance emargee est celle de la visio planifiee pour l'etape, jamais une
    // valeur saisie independamment - un seul rendez-vous fait foi.
    public DateTime? DateSeance { get; set; }

    // Rempli a chaque (re)envoi de la demande de signature par le Gestionnaire.
    public DateTime? EnvoyeLe { get; set; }

    // Rempli une seule fois, par le membre lui-meme - jamais reinitialise (une signature ne
    // se retire pas), cf. IEmargementService.SignerAsync.
    public DateTime? SigneLe { get; set; }

    // Reference opaque vers le trace de signature (image PNG dessinee sur un pad tactile),
    // stockee via IPreuveFichierStockageService - meme abstraction et meme principe que
    // PreuveFichier.CheminStockage (jamais de blob binaire directement en base, jamais
    // expose par une URL publique directe). La meme reference est dupliquee sur chaque
    // ligne signee dans une meme soumission (une seule image physique, plusieurs cartes).
    public string? SignatureCheminStockage { get; set; }

    public decimal? HeuresPresence { get; set; }
    public decimal? HeuresTravailPersonnel { get; set; }
}

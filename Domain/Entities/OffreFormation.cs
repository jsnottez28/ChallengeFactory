namespace Domain.Entities;

// Une ligne de la vitrine "Catalogue de formations" (/catalogue-formations) : mecanisme
// commun qui couvre a la fois les quelques formations deja montees et les tres nombreuses
// formations encore a l'etat de catalogue marketing (cf. CLAUDE.md, "je les cree s'il y a
// des demandes"). Distinct de Challenge : un Challenge est un parcours CBL reellement
// jouable (Cohortes, Etapes, Cartes) ; une OffreFormation peut exister sans aucun Challenge
// derriere (statut SurDemande), et se rattache a un Challenge (ChallengeId) seulement une
// fois celui-ci construit et publie - c'est ce rattachement qui fait basculer le CTA de la
// carte publique de "Nous contacter" vers "Rejoindre ce Challenge" sans toucher au code.
public class OffreFormation
{
    public int Id { get; set; }

    public string Titre { get; set; } = null!;

    public CategorieOffreFormation Categorie { get; set; }

    // Colonne "Lien avec l'offre" / "Contenu cle" du catalogue source - texte court,
    // facultatif.
    public string? LienAvecOffre { get; set; }

    // Duree en texte libre ("3 h", "1 jour", "6 classes virtuelles de 1h30 sur 6 mois") :
    // trop heterogene pour un champ structure unique.
    public string Duree { get; set; } = null!;

    public string Public { get; set; } = null!;

    // "OPCO", "CPF", "Gratuit"... ou null si non pertinent/pas encore determine.
    public string? Financement { get; set; }

    // Prix affiche (exigence Qualiopi) - "Sur devis" par defaut tant que l'offre n'est pas
    // montee et chiffree precisement.
    public string Prix { get; set; } = "Sur devis";

    // Le format Challenge reste le mode de delivery privilegie chez Challenges Factory
    // (cf. CLAUDE.md) : a true par defaut sur toute nouvelle offre. Le presentiel est une
    // alternative possible, jamais le mode par defaut.
    public bool DisponibleEnChallenge { get; set; } = true;

    public bool DisponibleEnPresentiel { get; set; }

    // Cible marketing principale de l'offre (entreprise vs particulier) - reutilise
    // ModePlateforme plutot que de dupliquer un enum BtoB/BtoC.
    public ModePlateforme Cible { get; set; } = ModePlateforme.BtoB;

    public StatutOffreFormation Statut { get; set; } = StatutOffreFormation.SurDemande;

    // Renseigne uniquement quand un vrai Challenge a ete construit pour cette offre (cf.
    // commentaire de classe). Null tant que l'offre est SurDemande.
    public int? ChallengeId { get; set; }
    public Challenge? Challenge { get; set; }

    // Lien de secours pour une offre Disponible qui n'est pas un Challenge (ex. une page
    // marketing statique existante comme /bilan-de-competences). Si renseigne, prime sur
    // ChallengeId pour construire le CTA de la carte publique.
    public string? LienPersonnalise { get; set; }

    // Ordre d'affichage au sein d'une meme Categorie, modifiable depuis l'admin sans
    // redeploiement.
    public int Ordre { get; set; }

    public bool EstPublie { get; set; } = true;

    public DateTime CreeLe { get; set; } = DateTime.UtcNow;
}

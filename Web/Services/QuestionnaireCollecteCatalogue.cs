using Domain.Entities;

namespace Web.Services;

public enum TypeQuestionCollecte
{
    OuiNon,
    Nombre,
    Choix,
}

public sealed record QuestionCollecte(
    string Code,
    PosteCollecte Poste,
    TypeQuestionCollecte Type,
    string Libelle,
    string? Aide = null,
    string? Unite = null,
    string[]? Choix = null);

// Catalogue des questions de filtrage du questionnaire de collecte. Chaque question
// active (ou non) des postes/sous-postes du tableur Bilan Carbone(R) V9.0 qui ne peuvent
// PAS etre detectes par le Pre-diagnostic (approche monetaire) : fluides frigorigenes,
// fioul/GPL, flotte, fret, dechets, produits vendus... Les codes sont des cles stables :
// ils sont stockes dans QuestionnaireCollecte.ReponsesJson - ne jamais renommer un code
// publie (en ajouter un nouveau et retirer l'ancien du catalogue).
public static class QuestionnaireCollecteCatalogue
{
    public const string Oui = "Oui";
    public const string Non = "Non";
    public const string NeSaitPas = "Je ne sais pas";

    public static readonly IReadOnlyList<QuestionCollecte> Questions =
    [
        // Descriptif / perimetre
        new("NB_SITES", PosteCollecte.Descriptif, TypeQuestionCollecte.Nombre,
            "Combien de sites (bureaux, ateliers, entrepôts, magasins) compte votre entreprise ?", Unite: "sites"),
        new("SURFACE_TOTALE", PosteCollecte.Descriptif, TypeQuestionCollecte.Nombre,
            "Surface totale approximative de vos locaux", "Tous sites confondus, même approximative.", Unite: "m²"),
        new("STATUT_LOCAUX", PosteCollecte.Descriptif, TypeQuestionCollecte.Choix,
            "Vos locaux sont-ils…", Choix: ["Propriétaire", "Locataire", "Les deux"]),

        // Energie
        new("FIOUL_GPL", PosteCollecte.Energie, TypeQuestionCollecte.OuiNon,
            "Utilisez-vous du fioul, du GPL (propane/butane) ou du bois pour le chauffage ou un process ?"),
        new("RESEAU_CHALEUR", PosteCollecte.Energie, TypeQuestionCollecte.OuiNon,
            "Êtes-vous raccordé à un réseau de chaleur ou de froid urbain ?"),
        new("PRODUCTION_ENR", PosteCollecte.Energie, TypeQuestionCollecte.OuiNon,
            "Produisez-vous de l'électricité sur site (panneaux photovoltaïques…) ?"),
        new("CONTRAT_VERT", PosteCollecte.Energie, TypeQuestionCollecte.OuiNon,
            "Avez-vous un contrat d'électricité « verte » (garanties d'origine) ?"),

        // Autres emissions directes
        new("CLIM_FROID", PosteCollecte.AutresEmissionsDirectes, TypeQuestionCollecte.OuiNon,
            "Avez-vous de la climatisation, des pompes à chaleur, des chambres froides ou des vitrines réfrigérées ?",
            "Les recharges de fluides frigorigènes sont souvent un poste important et invisible en comptabilité."),
        new("PROCEDES_INDUSTRIELS", PosteCollecte.AutresEmissionsDirectes, TypeQuestionCollecte.OuiNon,
            "Avez-vous des procédés industriels émettant des gaz (soudure, chimie, CO2 technique…) ?"),
        new("AGRICULTURE", PosteCollecte.AutresEmissionsDirectes, TypeQuestionCollecte.OuiNon,
            "Avez-vous une activité agricole (élevage, cultures, épandage d'engrais) ?"),

        // Dechets
        new("DECHETS_PRESTATAIRE", PosteCollecte.DechetsDirects, TypeQuestionCollecte.OuiNon,
            "Avez-vous un ou plusieurs prestataires de collecte de déchets (hors collecte municipale) ?"),
        new("DECHETS_DANGEREUX", PosteCollecte.DechetsDirects, TypeQuestionCollecte.OuiNon,
            "Produisez-vous des déchets dangereux (huiles, solvants, DEEE, piles, déchets de soins…) ?"),

        // Immobilisations
        new("TRAVAUX_RECENTS", PosteCollecte.Immobilisations, TypeQuestionCollecte.OuiNon,
            "Avez-vous construit, agrandi ou rénové lourdement un bâtiment ces 5 dernières années ?"),
        new("NB_POSTES_INFO", PosteCollecte.Immobilisations, TypeQuestionCollecte.Nombre,
            "Nombre approximatif d'ordinateurs (fixes et portables)", Unite: "postes"),

        // Intrants
        new("ACHATS_MATIERES", PosteCollecte.IntrantsBiensEtMatieres, TypeQuestionCollecte.OuiNon,
            "Achetez-vous des matières premières ou des marchandises physiques (métaux, plastiques, papier, produits alimentaires…) ?"),
        new("RESTAURATION", PosteCollecte.IntrantsBiensEtMatieres, TypeQuestionCollecte.OuiNon,
            "Fournissez-vous des repas (restaurant d'entreprise, plateaux-repas, restauration pour vos clients) ?"),

        // Fret
        new("FLOTTE_FRET", PosteCollecte.Fret, TypeQuestionCollecte.OuiNon,
            "Possédez-vous ou louez-vous des véhicules pour transporter des marchandises (camions, utilitaires) ?"),
        new("TRANSPORTEURS", PosteCollecte.Fret, TypeQuestionCollecte.OuiNon,
            "Faites-vous appel à des transporteurs pour vos approvisionnements ou vos livraisons ?"),
        new("FRET_INTERNATIONAL", PosteCollecte.Fret, TypeQuestionCollecte.OuiNon,
            "Importez-vous ou exportez-vous hors d'Europe (fret maritime ou aérien) ?"),

        // Deplacements
        new("VEHICULES_SOCIETE", PosteCollecte.Deplacements, TypeQuestionCollecte.OuiNon,
            "Avez-vous des véhicules de société (fonction ou service) pour vos salariés ?"),
        new("VISITEURS", PosteCollecte.Deplacements, TypeQuestionCollecte.OuiNon,
            "Accueillez-vous régulièrement du public ou des clients sur site (magasin, événements, formations) ?"),

        // Aval
        new("PRODUITS_VENDUS", PosteCollecte.FinDeVie, TypeQuestionCollecte.OuiNon,
            "Vendez-vous des produits physiques (fabriqués ou revendus) ?"),
        new("PRODUITS_ENERGIE", PosteCollecte.Utilisation, TypeQuestionCollecte.OuiNon,
            "Ces produits consomment-ils de l'énergie ou des fluides lors de leur utilisation (appareils, véhicules, machines…) ?"),
    ];

    public static readonly IReadOnlyDictionary<string, QuestionCollecte> ParCode =
        Questions.ToDictionary(q => q.Code);

    public static string LibellePoste(PosteCollecte poste) => poste switch
    {
        PosteCollecte.Descriptif => "Descriptif de l'entreprise",
        PosteCollecte.Energie => "Énergie",
        PosteCollecte.AutresEmissionsDirectes => "Autres émissions directes",
        PosteCollecte.DechetsDirects => "Déchets directs",
        PosteCollecte.Immobilisations => "Immobilisations",
        PosteCollecte.IntrantsBiensEtMatieres => "Intrants - biens et matières",
        PosteCollecte.IntrantsServices => "Intrants - services",
        PosteCollecte.Fret => "Fret",
        PosteCollecte.Deplacements => "Déplacements",
        PosteCollecte.Utilisation => "Utilisation",
        PosteCollecte.FinDeVie => "Fin de vie",
        _ => poste.ToString(),
    };

    // Un mot sur l'enjeu de chaque poste + un exemple concret - reutilise a la fois dans le
    // tableur .xlsx (en tete de chaque onglet, cf. TableurCollecteGenerator) et dans
    // l'Espace Client (en tete de chaque groupe de la check-list, cf. EspaceClient/Index)
    // pour que remplir le tableur ait du sens plutot que de cocher des cases.
    public static string? NotePedagogique(PosteCollecte poste) => poste switch
    {
        PosteCollecte.Descriptif => "Ces informations cadrent le périmètre de votre Bilan Carbone® (taille, activité, année de référence) : elles ne génèrent pas d'émissions mais permettent de comparer vos résultats dans le temps et à des entreprises similaires.",
        PosteCollecte.Energie => "L'énergie de vos sites (électricité, gaz, fioul…) est souvent le poste le plus facile à mesurer précisément grâce à vos factures. Exemple : 20 000 kWh d'électricité par an représentent environ 1,2 t CO2e.",
        PosteCollecte.AutresEmissionsDirectes => "Les fuites de fluides frigorigènes (climatisation, chambres froides) sont un poste invisible en comptabilité mais souvent très impactant : quelques kg de gaz R410a peuvent représenter plusieurs tonnes de CO2e.",
        PosteCollecte.DechetsDirects => "Le traitement de vos déchets (collecte, incinération, enfouissement) émet du CO2 selon le mode de traitement choisi : le recyclage émet généralement moins que l'enfouissement.",
        PosteCollecte.Immobilisations => "Vos bâtiments, véhicules et équipements ont nécessité de l'énergie et des matières pour être fabriqués : cet impact est amorti sur leur durée de vie, comme en comptabilité.",
        PosteCollecte.IntrantsBiensEtMatieres => "Les matières premières et produits physiques que vous achetez (métaux, plastiques, papier…) ont une empreinte carbone liée à leur fabrication, calculée au poids.",
        PosteCollecte.IntrantsServices => "Vos dépenses de services (conseil, informatique, assurances…) sont estimées via un ratio €→CO2e par secteur d'activité, à partir de votre comptabilité (export FEC).",
        PosteCollecte.Fret => "Le transport de marchandises (vos achats livrés, vos produits expédiés) émet du CO2 selon le mode utilisé (route, rail, mer, air) et la distance parcourue.",
        PosteCollecte.Deplacements => "Les trajets domicile-travail et professionnels comptent parmi les postes les plus variables d'une entreprise à l'autre : un trajet en covoiturage ou à vélo pèse bien moins qu'une voiture seule.",
        PosteCollecte.Utilisation => "Si vos produits consomment de l'énergie une fois chez le client (appareil électrique, véhicule…), cette consommation sur toute leur durée de vie fait partie de votre empreinte.",
        PosteCollecte.FinDeVie => "Le devenir de vos produits et emballages une fois jetés (recyclage, incinération) complète le cycle de vie de ce que vous mettez sur le marché.",
        _ => null,
    };

    // Ne garde que les codes connus et des valeurs coherentes avec le type de question -
    // les reponses viennent d'un formulaire client, jamais de confiance aveugle.
    public static Dictionary<string, string> Nettoyer(Dictionary<string, string>? reponses)
    {
        var propres = new Dictionary<string, string>();
        if (reponses is null)
        {
            return propres;
        }

        foreach (var (code, valeurBrute) in reponses)
        {
            if (!ParCode.TryGetValue(code, out var question) || string.IsNullOrWhiteSpace(valeurBrute))
            {
                continue;
            }

            var valeur = valeurBrute.Trim();
            var acceptee = question.Type switch
            {
                TypeQuestionCollecte.OuiNon => valeur is Oui or Non or NeSaitPas,
                TypeQuestionCollecte.Choix => question.Choix?.Contains(valeur) == true,
                TypeQuestionCollecte.Nombre => decimal.TryParse(valeur.Replace(',', '.'), System.Globalization.NumberStyles.Number,
                    System.Globalization.CultureInfo.InvariantCulture, out var nombre) && nombre >= 0,
                _ => false,
            };

            if (acceptee)
            {
                propres[code] = question.Type == TypeQuestionCollecte.Nombre ? valeur.Replace(',', '.') : valeur;
            }
        }

        return propres;
    }
}

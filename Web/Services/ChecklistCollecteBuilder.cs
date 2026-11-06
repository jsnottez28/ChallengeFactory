using Application.Common.Interfaces;
using Domain.Entities;
using Web.Data;
using static Web.Services.QuestionnaireCollecteCatalogue;

namespace Web.Services;

// Construit la liste des elements a collecter pour un Bilan Carbone(R) complet, a partir :
// 1. d'un socle toujours demande (descriptif, electricite, FEC, domicile-travail...) ;
// 2. des reponses au questionnaire de filtrage (chaque "Oui" - ou "Je ne sais pas", qu'on
//    prefere verifier - active les sous-postes correspondants du tableur ABC V9.0) ;
// 3. des lignes du Pre-diagnostic : chaque valeur deja saisie par le prospect est
//    pre-remplie (Type "Comptable" pour un montant, "Approchée" pour une quantite
//    declarative) et peut declencher des donnees physiques plus fines a collecter.
// Classe pure (aucun acces base/IO) : entierement testable.
public static class ChecklistCollecteBuilder
{
    public const string TypeComptable = "Comptable";
    public const string TypeApprochee = "Approchée";
    public const string TypePhysique = "Physique";

    // Codes FacteurEmission (seed du Pre-diagnostic) classes en "biens" plutot qu'en
    // "services" dans la nomenclature ABC.
    private static readonly HashSet<string> CodesAchatsBiens = ["ACHAT_MARCHANDISES", "ACHAT_FOURNITURES", "ACHAT_PRODUITS_DIVERS"];

    private sealed class Element
    {
        public required ElementCollecteInfo Info { get; init; }
        public string? CodeFacteur { get; init; }
    }

    public static List<ElementCollecteInfo> Construire(PreBilanCarbone preBilan, IReadOnlyDictionary<string, string>? reponses)
    {
        reponses ??= new Dictionary<string, string>();
        var elements = new List<Element>();

        bool Active(string code) => reponses.TryGetValue(code, out var v) && v is Oui or NeSaitPas;
        string Motif(string code) => reponses.TryGetValue(code, out var v) ? $"Questionnaire : « {ParCode[code].Libelle} » → {v}" : "";

        void Ajouter(PosteCollecte poste, string sousPoste, string donnee, string unite, string piece, string referent,
            string declencheur, string? codeFacteur = null)
        {
            elements.Add(new Element
            {
                CodeFacteur = codeFacteur,
                Info = new ElementCollecteInfo
                {
                    Cle = $"{(int)poste}|{sousPoste}|{donnee}",
                    Poste = poste,
                    SousPoste = sousPoste,
                    Donnee = donnee,
                    Unite = unite,
                    PieceJustificative = piece,
                    Referent = referent,
                    Declencheur = declencheur,
                },
            });
        }

        const string socle = "Socle Bilan Carbone® (toujours demandé)";

        // --- Descriptif -------------------------------------------------------------
        Ajouter(PosteCollecte.Descriptif, "Informations générales", "Raison sociale, SIRET, SIREN, code NAF", "-",
            "Extrait Kbis / avis de situation INSEE", "Direction", socle);
        Ajouter(PosteCollecte.Descriptif, "Périmètre", "Année de référence du Bilan (12 mois consécutifs)", "année",
            "Dates de l'exercice comptable", "Direction", socle);
        Ajouter(PosteCollecte.Descriptif, "Métriques", "Effectif moyen", "ETP", "Registre du personnel / DSN", "RH", socle);
        Ajouter(PosteCollecte.Descriptif, "Métriques", "Chiffre d'affaires", "k€", "Compte de résultat", "Comptabilité", socle);
        Ajouter(PosteCollecte.Descriptif, "Métriques", "Résultat d'exploitation et résultat net", "k€", "Compte de résultat", "Comptabilité", socle);
        Ajouter(PosteCollecte.Descriptif, "Périmètre", "Liste des sites (adresse, surface, usage, propriétaire ou locataire)", "m²",
            "Baux / actes de propriété", "Services généraux", socle);

        // --- Energie ----------------------------------------------------------------
        Ajouter(PosteCollecte.Energie, "Électricité", "Consommation d'électricité par site sur 12 mois", "kWh",
            "Factures d'électricité (12 mois) ou export espace client fournisseur", "Services généraux", socle, "ENERGIE_ELEC");
        if (preBilan.Lignes.Any(l => l.FacteurEmission?.Code == "ENERGIE_GAZ"))
        {
            Ajouter(PosteCollecte.Energie, "Combustibles fossiles", "Consommation de gaz naturel par site sur 12 mois", "kWh PCS",
                "Factures de gaz (12 mois) - relever les kWh PCS", "Services généraux", "Pré-diagnostic : gaz renseigné", "ENERGIE_GAZ");
        }
        if (Active("FIOUL_GPL"))
        {
            Ajouter(PosteCollecte.Energie, "Combustibles fossiles", "Fioul, GPL (propane/butane) livrés sur 12 mois", "litres ou kg",
                "Bons de livraison / factures de combustibles", "Services généraux", Motif("FIOUL_GPL"));
            Ajouter(PosteCollecte.Energie, "Combustibles organiques", "Bois, granulés, plaquettes achetés", "tonnes",
                "Factures de combustibles bois", "Services généraux", Motif("FIOUL_GPL"));
        }
        if (Active("RESEAU_CHALEUR"))
        {
            Ajouter(PosteCollecte.Energie, "Réseaux de chaleur et de vapeur / de froid", "Énergie livrée par le réseau + nom du réseau", "kWh",
                "Factures du réseau de chaleur ou de froid", "Services généraux", Motif("RESEAU_CHALEUR"));
        }
        if (Active("PRODUCTION_ENR"))
        {
            Ajouter(PosteCollecte.Energie, "Électricité", "Électricité produite sur site (autoconsommée et revendue)", "kWh",
                "Relevés de production / contrat de rachat", "Services généraux", Motif("PRODUCTION_ENR"));
        }
        if (Active("CONTRAT_VERT"))
        {
            Ajouter(PosteCollecte.Energie, "Électricité", "Contrat d'électricité verte (approche market-based)", "-",
                "Contrat fournisseur + attestation de garanties d'origine", "Services généraux", Motif("CONTRAT_VERT"));
        }

        // --- Autres emissions directes ---------------------------------------------
        if (Active("CLIM_FROID"))
        {
            Ajouter(PosteCollecte.AutresEmissionsDirectes, "Fuites de fluides frigorigènes",
                "Quantités de fluides rechargées par type de gaz (R410a, R32, R404a…)", "kg",
                "Fiches d'intervention du frigoriste / registre des équipements", "Services généraux / prestataire maintenance", Motif("CLIM_FROID"));
        }
        if (Active("PROCEDES_INDUSTRIELS"))
        {
            Ajouter(PosteCollecte.AutresEmissionsDirectes, "Émissions liées aux procédés industriels",
                "Gaz émis par les procédés (CO2, N2O, CH4, SF6…)", "kg émis",
                "Bilan matière / déclarations GEREP / achats de gaz techniques", "Production / QHSE", Motif("PROCEDES_INDUSTRIELS"));
        }
        if (Active("AGRICULTURE"))
        {
            Ajouter(PosteCollecte.AutresEmissionsDirectes, "Émissions liées à l'agriculture",
                "Cheptel (nombre d'animaux par type), surfaces cultivées, azote épandu", "animaux / ha / kg N",
                "Déclaration PAC, plan de fumure", "Exploitation", Motif("AGRICULTURE"));
        }

        // --- Dechets ---------------------------------------------------------------
        Ajouter(PosteCollecte.DechetsDirects, "Déchets ordures ménagères", "Volume de déchets en mélange (nombre de bacs × fréquence)", "tonnes",
            "Contrat de collecte / estimation des volumes", "Services généraux", socle);
        if (Active("DECHETS_PRESTATAIRE"))
        {
            Ajouter(PosteCollecte.DechetsDirects, "Déchets emballages, matériaux, organiques",
                "Tonnages par type de déchet ET par mode de traitement (recyclage, incinération, enfouissement)", "tonnes",
                "Bilans annuels / bordereaux des prestataires déchets", "Services généraux", Motif("DECHETS_PRESTATAIRE"));
        }
        if (Active("DECHETS_DANGEREUX"))
        {
            Ajouter(PosteCollecte.DechetsDirects, "Déchets dangereux", "Tonnages de déchets dangereux, DEEE, piles", "tonnes",
                "Bordereaux de suivi des déchets dangereux (BSD / Trackdéchets)", "QHSE / Services généraux", Motif("DECHETS_DANGEREUX"));
        }
        Ajouter(PosteCollecte.DechetsDirects, "Eaux usées", "Volume d'eau consommée / rejetée", "m³",
            "Factures d'eau", "Services généraux", socle);

        // --- Immobilisations -------------------------------------------------------
        Ajouter(PosteCollecte.Immobilisations, "Bâtiments, équipements, véhicules",
            "Registre des immobilisations (date d'acquisition, valeur, durée d'amortissement)", "k€ HT / m² / tonnes",
            "Tableau des immobilisations (liasse fiscale)", "Comptabilité / expert-comptable", socle);
        Ajouter(PosteCollecte.Immobilisations, "Informatique", "Inventaire du parc informatique par type (portables, fixes, écrans, serveurs, smartphones)",
            "unités", "Inventaire IT / factures d'achat", "Informatique",
            reponses.TryGetValue("NB_POSTES_INFO", out var nbPostes) ? $"Questionnaire : {nbPostes} postes déclarés" : socle);
        if (Active("TRAVAUX_RECENTS"))
        {
            Ajouter(PosteCollecte.Immobilisations, "Bâtiments", "Surface construite/rénovée, année et type de structure", "m²",
                "Permis de construire / DOE / factures de travaux", "Direction / Services généraux", Motif("TRAVAUX_RECENTS"));
        }

        // --- Intrants ---------------------------------------------------------------
        Ajouter(PosteCollecte.IntrantsServices, "Services en approche monétaire", "Dépenses par compte de charges (classe 6)", "k€ HT",
            "Export FEC ou grand livre de l'exercice", "Expert-comptable", socle);
        if (Active("ACHATS_MATIERES"))
        {
            Ajouter(PosteCollecte.IntrantsBiensEtMatieres, "Métaux, plastiques, verre, papiers, produits chimiques",
                "Quantités achetées par matière (en poids)", "tonnes",
                "Extraction ERP / relevés fournisseurs / fiches techniques", "Achats", Motif("ACHATS_MATIERES"));
            Ajouter(PosteCollecte.IntrantsBiensEtMatieres, "Matière destinée aux emballages", "Emballages achetés par matériau", "tonnes",
                "Factures fournisseurs d'emballages / déclaration éco-organisme", "Achats / Logistique", Motif("ACHATS_MATIERES"));
        }
        if (Active("RESTAURATION"))
        {
            Ajouter(PosteCollecte.IntrantsBiensEtMatieres, "Nourriture", "Nombre de repas servis par type (avec/sans viande)", "repas",
                "Factures du prestataire de restauration / tickets", "Services généraux", Motif("RESTAURATION"));
        }

        // --- Fret ------------------------------------------------------------------
        if (Active("FLOTTE_FRET"))
        {
            Ajouter(PosteCollecte.Fret, "Fret (véhicules possédés ou opérés)", "Carburant consommé par les véhicules de transport", "litres",
                "Relevés de cartes carburant / factures", "Logistique / flotte", Motif("FLOTTE_FRET"));
        }
        if (Active("TRANSPORTEURS") || preBilan.Lignes.Any(l => l.FacteurEmission?.Code == "ACHAT_TRANSPORT_MARCHANDISES"))
        {
            Ajouter(PosteCollecte.Fret, "Fret entrant et sortant", "Poids transporté × distance par mode (route, rail…), ou CO2e fourni par le transporteur",
                "tonne.km ou kg CO2e", "Factures transporteurs / information CO2 des prestations de transport",
                "Logistique / Achats", Active("TRANSPORTEURS") ? Motif("TRANSPORTEURS") : "Pré-diagnostic : dépenses de transport renseignées");
        }
        if (Active("FRET_INTERNATIONAL"))
        {
            Ajouter(PosteCollecte.Fret, "Fret maritime / aérien", "Tonnages et trajets (origine, destination, mode)", "tonne.km",
                "Déclarations en douane / connaissements / Incoterms", "Logistique / ADV", Motif("FRET_INTERNATIONAL"));
        }

        // --- Deplacements ----------------------------------------------------------
        Ajouter(PosteCollecte.Deplacements, "Déplacements domicile-travail", "Distance, mode de transport et jours de présence par salarié", "km",
            "Questionnaire mobilité salariés (2 minutes)", "RH / chaque salarié", socle);
        Ajouter(PosteCollecte.Deplacements, "Déplacements professionnels", "Kilomètres parcourus par mode (voiture, train, avion)", "km / passager.km",
            "Notes de frais, relevés de l'agence de voyage, indemnités kilométriques", "Comptabilité / RH", socle);
        if (Active("VEHICULES_SOCIETE"))
        {
            Ajouter(PosteCollecte.Deplacements, "Déplacements professionnels (véhicules possédés ou opérés)",
                "Carburant ou kWh consommés par les véhicules de société", "litres / kWh",
                "Relevés de cartes carburant / recharge, contrats de location longue durée", "Gestionnaire de flotte", Motif("VEHICULES_SOCIETE"));
        }
        if (Active("VISITEURS"))
        {
            Ajouter(PosteCollecte.Deplacements, "Déplacements visiteurs", "Nombre de visiteurs par an et répartition estimée des modes de transport",
                "visiteurs / km", "Comptage des entrées / enquête visiteurs", "Accueil / Commercial", Motif("VISITEURS"));
        }

        // --- Aval : utilisation et fin de vie -------------------------------------
        if (Active("PRODUITS_VENDUS"))
        {
            Ajouter(PosteCollecte.FinDeVie, "Traitement des déchets et des emballages en fin de vie",
                "Quantités de produits et d'emballages vendus, par matériau", "tonnes",
                "Volumes de ventes + nomenclatures produits / déclarations éco-organismes", "Commercial / Bureau d'études", Motif("PRODUITS_VENDUS"));
        }
        if (Active("PRODUITS_ENERGIE"))
        {
            Ajouter(PosteCollecte.Utilisation, "Utilisation en responsabilité",
                "Consommation d'énergie ou de fluides d'un produit sur sa durée de vie × quantités vendues", "kWh / litres",
                "Fiches techniques produits + volumes de ventes", "Bureau d'études / Commercial", Motif("PRODUITS_ENERGIE"));
        }

        // --- Pre-remplissage depuis le Pre-diagnostic ------------------------------
        var dateDepot = preBilan.DeposeLe.ToLocalTime().ToString("dd/MM/yyyy");
        var source = $"Pré-diagnostic du {dateDepot} (déclaratif, à confirmer)";

        var descriptifEtp = elements.First(e => e.Info.Donnee == "Effectif moyen").Info;
        if (preBilan.EffectifEtp is > 0)
        {
            descriptifEtp.Valeur = preBilan.EffectifEtp;
            descriptifEtp.TypeDonnee = TypeApprochee;
            descriptifEtp.Source = source;
        }

        var descriptifCa = elements.First(e => e.Info.Donnee == "Chiffre d'affaires").Info;
        if (preBilan.ChiffreAffairesKEuros is > 0)
        {
            descriptifCa.Valeur = preBilan.ChiffreAffairesKEuros;
            descriptifCa.TypeDonnee = TypeComptable;
            descriptifCa.Source = source;
        }

        foreach (var ligne in preBilan.Lignes.Where(l => l.FacteurEmission is not null).OrderBy(l => l.FacteurEmission.Poste).ThenBy(l => l.FacteurEmission.Ordre))
        {
            var facteur = ligne.FacteurEmission;
            var monetaire = facteur.TypeDonnee == TypeDonneeActivite.Monetaire;
            // Le tableur ABC attend les montants en k€ HT ; le Pre-diagnostic les saisit en €.
            var valeur = monetaire ? Math.Round(ligne.ValeurSaisie / 1000m, 3) : ligne.ValeurSaisie;
            var unite = monetaire ? "k€ HT" : facteur.Unite;
            var typeDonnee = monetaire ? TypeComptable : TypeApprochee;

            var existant = elements.FirstOrDefault(e => e.CodeFacteur == facteur.Code && e.Info.Valeur is null);
            if (existant is not null)
            {
                existant.Info.Valeur = valeur;
                existant.Info.Unite = unite;
                existant.Info.TypeDonnee = typeDonnee;
                existant.Info.Source = source;
                existant.Info.Declencheur += " + pré-diagnostic (valeur à confirmer)";
                continue;
            }

            var (poste, sousPoste) = Classer(facteur);
            elements.Add(new Element
            {
                CodeFacteur = facteur.Code,
                Info = new ElementCollecteInfo
                {
                    Cle = $"{(int)poste}|{sousPoste}|{facteur.Nom}",
                    Poste = poste,
                    SousPoste = sousPoste,
                    Donnee = facteur.Nom,
                    Unite = unite,
                    PieceJustificative = monetaire ? "Export FEC / grand livre (comptes concernés)" : "Relevés / factures",
                    Referent = monetaire ? "Comptabilité / expert-comptable" : "À préciser",
                    Declencheur = "Pré-diagnostic : donnée renseignée",
                    Valeur = valeur,
                    TypeDonnee = typeDonnee,
                    Source = source,
                },
            });
        }

        return elements
            .Select(e => e.Info)
            .OrderBy(i => i.Poste)
            .ToList();
    }

    // Classement d'un facteur du Pre-diagnostic dans la nomenclature ABC (onglet + sous-poste
    // du tableur V9.0).
    private static (PosteCollecte Poste, string SousPoste) Classer(FacteurEmission facteur) => facteur.Poste switch
    {
        PosteEmission.AchatsBiensEtServices when CodesAchatsBiens.Contains(facteur.Code)
            => (PosteCollecte.IntrantsBiensEtMatieres, "Biens et matières en approche monétaire"),
        PosteEmission.AchatsBiensEtServices => (PosteCollecte.IntrantsServices, "Services en approche monétaire"),
        PosteEmission.Immobilisations when facteur.Code == "IMMO_BATIMENTS" => (PosteCollecte.Immobilisations, "Bâtiments"),
        PosteEmission.Immobilisations when facteur.Code == "IMMO_INFORMATIQUE" => (PosteCollecte.Immobilisations, "Informatique"),
        PosteEmission.Immobilisations => (PosteCollecte.Immobilisations, "Équipements (via les ratios monétaires)"),
        PosteEmission.Energie => (PosteCollecte.Energie, "Électricité / combustibles"),
        PosteEmission.Deplacements when facteur.Code.StartsWith("DEPLACEMENT_DOMTRAV") => (PosteCollecte.Deplacements, "Déplacements domicile-travail"),
        PosteEmission.Deplacements => (PosteCollecte.Deplacements, "Déplacements professionnels"),
        _ => (PosteCollecte.IntrantsServices, "Services en approche monétaire"),
    };
}

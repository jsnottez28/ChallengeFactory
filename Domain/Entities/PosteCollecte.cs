namespace Domain.Entities;

// Postes de la collecte de donnees d'un Bilan Carbone(R) complet : reprend EXACTEMENT les
// 10 onglets de saisie du tableur Bilan Carbone(R) V9.0 de l'ABC (+ la fiche Descriptif),
// dans l'ordre du tableur. Distinct de PosteEmission, qui est le regroupement simplifie du
// Pre-diagnostic (ex. "AchatsBiensEtServices" couvre a la fois les onglets "Intrants -
// biens et matieres" et "Intrants - services") : la collecte doit, elle, pouvoir etre
// recopiee onglet par onglet dans le tableur officiel.
public enum PosteCollecte
{
    Descriptif,
    Energie,
    AutresEmissionsDirectes,
    DechetsDirects,
    Immobilisations,
    IntrantsBiensEtMatieres,
    IntrantsServices,
    Fret,
    Deplacements,
    Utilisation,
    FinDeVie,
}

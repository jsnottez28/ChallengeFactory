namespace Domain.Entities;

// Nomenclature des "postes" d'emission du Bilan Carbone(R) (methode ABC) : seuls les
// postes couverts par le Pre-diagnostic carbone sont presents aujourd'hui, mais les valeurs
// reprennent le nom exact des onglets de calcul officiels pour pouvoir etendre plus tard
// vers un vrai Bilan Carbone multi-postes sans renommer quoi que ce soit (ex. Fret,
// DechetsDirects, Utilisation, FinDeVie, AutresEmissionsDirectes).
public enum PosteEmission
{
    AchatsBiensEtServices,
    Immobilisations,
    Energie,
    Deplacements,
}

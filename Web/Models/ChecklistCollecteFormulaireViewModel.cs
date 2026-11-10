using Application.Common.Interfaces;

namespace Web.Models;

// Modele de la vue partagee _ChecklistCollecteFormulaire (Espace Client + fiche admin) :
// la check-list du catalogue, les lignes ajoutees librement, et les 3 URLs de
// soumission (une ligne du catalogue, une ligne libre, suppression d'une ligne libre) -
// differentes selon l'appelant (Razor Page cote client, MVC cote admin).
public sealed class ChecklistCollecteFormulaireViewModel
{
    public required List<ElementCollecteInfo> Checklist { get; init; }
    public required List<LigneLibreCollecte> LignesLibres { get; init; }
    public required string FormActionLigne { get; init; }
    public required string FormActionLigneLibre { get; init; }
    public required string FormActionSupprimerLigneLibre { get; init; }
}

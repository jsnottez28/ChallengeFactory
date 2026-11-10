namespace Web.Models;

// Une ligne du formulaire web "simple" de saisie de la check-list de collecte carbone -
// cf. ICollecteCarboneService.EnregistrerValeursFormulaireAsync. Partage entre l'Espace
// Client (EspaceClient/Index) et la fiche admin (PreDiagnosticsCarbone/Details), qui
// postent tous les deux vers le meme type de donnees, via la meme vue partielle
// _ChecklistCollecteFormulaire.
public sealed class ValeurSaisieLigneForm
{
    public string Cle { get; set; } = string.Empty;
    public string? LibellePersonnalise { get; set; }
    public string? Valeur { get; set; }
    public string? Unite { get; set; }
    public string? Type { get; set; }
    public string? Source { get; set; }
    public string? Hypotheses { get; set; }
    public string? Statut { get; set; }
    public string? Commentaire { get; set; }
}

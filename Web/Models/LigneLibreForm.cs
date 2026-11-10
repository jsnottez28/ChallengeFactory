using Domain.Entities;

namespace Web.Models;

// Ajout/edition d'une ligne "libre" (donnee hors catalogue) depuis le formulaire web -
// cf. ICollecteCarboneService.EnregistrerLigneLibreAsync. Id vide = creation.
public sealed class LigneLibreForm
{
    public string? Id { get; set; }
    public PosteCollecte Poste { get; set; }
    public string Donnee { get; set; } = string.Empty;
    public string? Valeur { get; set; }
    public string? Unite { get; set; }
    public string? Type { get; set; }
    public string? Source { get; set; }
    public string? Hypotheses { get; set; }
    public string? Statut { get; set; }
    public string? Commentaire { get; set; }
}

namespace Domain.Entities;

// Entite referentielle (feuille "badges" de data.xlsx). Rattache une ou plusieurs Cartes
// de Competences a un badge/programme.
public class Badge
{
    public int Id { get; set; }

    public string BadgeCode { get; set; } = null!;
    public string BadgeNom { get; set; } = null!;
    public string? BadgeImage { get; set; }
    public string? Programme { get; set; }
}

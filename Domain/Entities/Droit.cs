namespace Domain.Entities;

public class Droit
{
    public int Id { get; set; }
    public string Code { get; set; } = null!;
    public string Libelle { get; set; } = null!;
    public string? Description { get; set; }

    public int RessourceId { get; set; }
    public Ressource Ressource { get; set; } = null!;

    public int TypeActionId { get; set; }
    public TypeAction TypeAction { get; set; } = null!;

    // Nullable : un droit peut exister sans etre rattache a un groupe
    public int? GroupeDroitId { get; set; }
    public GroupeDroit? GroupeDroit { get; set; }
}

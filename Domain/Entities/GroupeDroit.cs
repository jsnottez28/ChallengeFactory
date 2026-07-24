namespace Domain.Entities;

public class GroupeDroit
{
    public int Id { get; set; }
    public string Code { get; set; } = null!;
    public string Libelle { get; set; } = null!;
    public string? Description { get; set; }

    public ICollection<Droit> Droits { get; set; } = new List<Droit>();
}

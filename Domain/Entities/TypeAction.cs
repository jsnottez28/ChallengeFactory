namespace Domain.Entities;

// Nomme "TypeAction" et non "Action" pour eviter toute collision
// avec les Actions ASP.NET Core MVC (IActionResult, [ActionName], etc.)
public class TypeAction
{
    public int Id { get; set; }
    public string Code { get; set; } = null!;
    public string Libelle { get; set; } = null!;
    public string? Description { get; set; }

    public ICollection<Droit> Droits { get; set; } = new List<Droit>();
}

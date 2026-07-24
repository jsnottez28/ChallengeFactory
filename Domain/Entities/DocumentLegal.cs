namespace Domain.Entities;

public class DocumentLegal
{
    public int Id { get; set; }
    public TypeDocumentLegal Type { get; set; }
    public string Version { get; set; } = null!;

    // Redige en Markdown par l'administrateur, converti en HTML a l'affichage.
    public string Contenu { get; set; } = null!;

    public DateTime DateEffective { get; set; }
    public bool EstPublie { get; set; }
    public DateTime CreeLe { get; set; } = DateTime.UtcNow;
}

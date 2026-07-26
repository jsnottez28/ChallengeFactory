using Domain.Entities;

namespace Web.Data;

public class PreuveFichier
{
    public int Id { get; set; }

    public int PreuveId { get; set; }
    public Preuve Preuve { get; set; } = null!;

    public TypeFichierPreuve TypeFichier { get; set; }
    public string NomFichier { get; set; } = string.Empty;

    // Reference opaque vers le stockage (cf. IPreuveFichierStockageService) - jamais un
    // chemin disque en dur manipule directement ici.
    public string CheminStockage { get; set; } = string.Empty;

    public long TailleOctets { get; set; }
    public DateTime DateUpload { get; set; } = DateTime.UtcNow;
}

namespace Web.Data;

// Journal d'audit : une ligne par validation manuelle d'etape (trace qui a fait avancer
// la Cohorte et quand - jamais de progression automatique, voir ICohorteService).
public class CohorteEtapeValidation
{
    public int Id { get; set; }

    public int CohorteId { get; set; }
    public Cohorte Cohorte { get; set; } = null!;

    // L'etape qui vient d'etre validee (celle que l'on quitte, pas celle que l'on rejoint).
    public int NumeroEtape { get; set; }

    public string ValideParId { get; set; } = string.Empty;
    public ApplicationUser ValidePar { get; set; } = null!;

    public DateTime ValideLe { get; set; } = DateTime.UtcNow;
}

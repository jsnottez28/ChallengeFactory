namespace Web.Data;

public class CohorteMembre
{
    public int Id { get; set; }

    public int CohorteId { get; set; }
    public Cohorte Cohorte { get; set; } = null!;

    public string UtilisateurId { get; set; } = string.Empty;
    public ApplicationUser Utilisateur { get; set; } = null!;

    public DateTime DateAjout { get; set; } = DateTime.UtcNow;
    public MethodeAjoutMembre MethodeAjout { get; set; }
}

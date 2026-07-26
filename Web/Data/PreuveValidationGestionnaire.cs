using Domain.Entities;

namespace Web.Data;

// Validation directe (section 4.1) : le Gestionnaire/Coach/Chef de Projet (au sens
// "detient le droit PREUVE.VALIDER", pas un role nomme en dur - cf. RBAC existant) peut
// trancher une Preuve a tout moment avant la cloture de l'etape, sans attendre le vote
// des pairs.
public class PreuveValidationGestionnaire
{
    public int Id { get; set; }

    public int PreuveId { get; set; }
    public Preuve Preuve { get; set; } = null!;

    public string ValideurId { get; set; } = string.Empty;
    public ApplicationUser Valideur { get; set; } = null!;

    public DecisionValidationGestionnaire Decision { get; set; }

    // Optionnel si Valide, obligatoire si Refuse (regle appliquee cote serveur).
    public string? Commentaire { get; set; }

    public DateTime DateValidation { get; set; } = DateTime.UtcNow;
}

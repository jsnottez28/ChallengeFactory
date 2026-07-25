using Domain.Entities;

namespace Web.Data;

// Vote independant d'un pair de la cohorte. Le controle "valideur != auteur de la
// preuve" est applique cote serveur dans IPreuveService, jamais suppose garanti par le
// modele seul. Un meme (PreuveId, ValideurId) ne peut apparaitre qu'une fois (index
// unique) - modifier son avis met a jour la ligne existante, ne la duplique jamais.
public class PreuveValidationPair
{
    public int Id { get; set; }

    public int PreuveId { get; set; }
    public Preuve Preuve { get; set; } = null!;

    public string ValideurId { get; set; } = string.Empty;
    public ApplicationUser Valideur { get; set; } = null!;

    public DecisionValidationPair Decision { get; set; }

    // Optionnel si Valide, obligatoire si ARevoir (regle appliquee cote serveur).
    public string? Commentaire { get; set; }

    public DateTime DateValidation { get; set; } = DateTime.UtcNow;
}

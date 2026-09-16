using Domain.Entities;

namespace Web.Data;

// Canal de reclamation trace (Qualiopi indicateur 32 : traitement des reclamations) -
// distinct du formulaire de contact general (Web/Views/Home/Contact.cshtml), qui se
// contente de relayer un email sans aucune tracabilite. Toute reclamation deposee ici est
// persistee avec un statut audite ; jamais de suppression (contrairement au formulaire de
// contact, purement transactionnel), pour rester consultable a l'audit.
public class Reclamation
{
    public int Id { get; set; }

    public string Nom { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;

    // Rempli automatiquement si la personne etait connectee au moment du depot - jamais
    // exige (le canal reste ouvert aux prospects/visiteurs non inscrits).
    public string? UtilisateurId { get; set; }
    public ApplicationUser? Utilisateur { get; set; }

    public DateTime DeposeLe { get; set; } = DateTime.UtcNow;

    public StatutReclamation Statut { get; set; } = StatutReclamation.Recue;

    public string? Reponse { get; set; }
    public string? TraiteParId { get; set; }
    public ApplicationUser? TraitePar { get; set; }
    public DateTime? TraiteLe { get; set; }
}

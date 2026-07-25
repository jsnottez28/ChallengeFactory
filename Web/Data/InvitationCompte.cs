namespace Web.Data;

// Compte cree sans mot de passe utilisable (import BtoB, voir ICohorteService) : un token
// a usage unique permet a l'utilisateur de definir son propre mot de passe via une page
// publique, sans jamais transmettre de mot de passe en clair par email.
public class InvitationCompte
{
    public int Id { get; set; }

    public string UtilisateurId { get; set; } = string.Empty;
    public ApplicationUser Utilisateur { get; set; } = null!;

    public string Token { get; set; } = Guid.NewGuid().ToString("N");

    public DateTime CreeLe { get; set; } = DateTime.UtcNow;
    public DateTime ExpireLe { get; set; } = DateTime.UtcNow.AddDays(7);
    public DateTime? UtiliseLe { get; set; }

    // Renvoyer une invitation invalide l'ancien token plutot que de le supprimer, pour
    // conserver un historique complet des tentatives d'activation.
    public bool EstActif { get; set; } = true;
}

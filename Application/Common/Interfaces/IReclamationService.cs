using Domain.Entities;

namespace Application.Common.Interfaces;

public sealed class ReclamationInput
{
    public string Nom { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;

    // Rempli par l'appelant si l'auteur est connecte - jamais exige.
    public string? UtilisateurId { get; set; }
}

public sealed class ReclamationInfo
{
    public int Id { get; set; }
    public string Nom { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime DeposeLe { get; set; }
    public StatutReclamation Statut { get; set; }
    public string? Reponse { get; set; }
    public string? TraiteParNomComplet { get; set; }
    public DateTime? TraiteLe { get; set; }
}

// Canal de reclamation trace (Qualiopi indicateur 32), distinct du formulaire de contact
// general - cf. Web/Data/Reclamation.cs pour le detail du raisonnement.
public interface IReclamationService
{
    // Accessible sans compte (prospect/visiteur) - accuse de reception envoye
    // systematiquement a l'auteur.
    Task DeposerAsync(ReclamationInput input);

    Task<List<ReclamationInfo>> GetAllAsync();

    Task<ReclamationInfo?> GetByIdAsync(int id);

    // Passe le statut a EnCours sans reponse definitive - utile pour tracer une prise en
    // charge avant d'avoir la reponse finale.
    Task<(bool Success, string? ErrorMessage)> PrendreEnChargeAsync(int id, string gestionnaireId);

    // Enregistre la reponse, passe le statut a Traitee, et envoie la reponse par email a
    // l'auteur de la reclamation.
    Task<(bool Success, string? ErrorMessage)> RepondreAsync(int id, string gestionnaireId, string reponse);
}

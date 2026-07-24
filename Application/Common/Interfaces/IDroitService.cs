using Domain.Entities;

namespace Application.Common.Interfaces;

public sealed class DroitInput
{
    /// <summary>Si vide, genere automatiquement "RESSOURCE.ACTION" a partir des codes choisis.</summary>
    public string? Code { get; set; }

    public string Libelle { get; set; } = string.Empty;
    public string? Description { get; set; }

    public int RessourceId { get; set; }
    public int TypeActionId { get; set; }
    public int? GroupeDroitId { get; set; }
}

public interface IDroitService
{
    /// <summary>Charge Ressource/TypeAction/GroupeDroit pour l'affichage. Filtres optionnels.</summary>
    Task<List<Droit>> GetAllAsync(int? ressourceId, int? groupeDroitId);

    Task<Droit?> GetByIdAsync(int id);

    Task<(bool Success, string? ErrorMessage, Droit? Droit)> CreateAsync(DroitInput input);

    Task<(bool Success, string? ErrorMessage)> UpdateAsync(int id, DroitInput input);

    Task<(bool Success, string? ErrorMessage)> DeleteAsync(int id);
}

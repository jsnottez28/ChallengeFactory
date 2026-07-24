using Domain.Entities;

namespace Application.Common.Interfaces;

public sealed class GroupeDroitInput
{
    public string Code { get; set; } = string.Empty;
    public string Libelle { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public interface IGroupeDroitService
{
    /// <summary>Charge aussi les droits associes (GroupeDroit.Droits) pour permettre d'en afficher le nombre.</summary>
    Task<List<GroupeDroit>> GetAllAsync();

    Task<GroupeDroit?> GetByIdAsync(int id);

    Task<(bool Success, string? ErrorMessage, GroupeDroit? GroupeDroit)> CreateAsync(GroupeDroitInput input);

    Task<(bool Success, string? ErrorMessage)> UpdateAsync(int id, GroupeDroitInput input);

    Task<(bool Success, string? ErrorMessage)> DeleteAsync(int id);
}

using Domain.Entities;

namespace Application.Common.Interfaces;

public sealed class RessourceInput
{
    public string Code { get; set; } = string.Empty;
    public string Libelle { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public interface IRessourceService
{
    Task<List<Ressource>> GetAllAsync();

    Task<Ressource?> GetByIdAsync(int id);

    Task<(bool Success, string? ErrorMessage, Ressource? Ressource)> CreateAsync(RessourceInput input);

    Task<(bool Success, string? ErrorMessage)> UpdateAsync(int id, RessourceInput input);

    Task<(bool Success, string? ErrorMessage)> DeleteAsync(int id);
}

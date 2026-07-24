using Domain.Entities;

namespace Application.Common.Interfaces;

public sealed class TypeActionInput
{
    public string Code { get; set; } = string.Empty;
    public string Libelle { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public interface ITypeActionService
{
    Task<List<TypeAction>> GetAllAsync();

    Task<TypeAction?> GetByIdAsync(int id);

    Task<(bool Success, string? ErrorMessage, TypeAction? TypeAction)> CreateAsync(TypeActionInput input);

    Task<(bool Success, string? ErrorMessage)> UpdateAsync(int id, TypeActionInput input);

    Task<(bool Success, string? ErrorMessage)> DeleteAsync(int id);
}

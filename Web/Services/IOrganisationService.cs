using Web.Data.Entities;

namespace Web.Services;

public sealed class OrganisationInput
{
    public string CodeAdherent { get; set; } = string.Empty;
    public string RaisonSociale { get; set; } = string.Empty;
    public string? Adresse1 { get; set; }
    public string? Adresse2 { get; set; }
    public string? CodePostal { get; set; }
    public string? Ville { get; set; }
    public string? TelephoneStandard { get; set; }
    public bool EstActif { get; set; } = true;
}

public interface IOrganisationService
{
    Task<List<Organisation>> GetAllAsync();

    Task<Organisation?> GetByIdAsync(int id);

    Task<Organisation> CreateAsync(OrganisationInput input);

    Task<bool> UpdateAsync(int id, OrganisationInput input);

    Task<bool> SetActiveAsync(int id, bool estActif);
}

using Domain.Entities;

namespace Application.Common.Interfaces;

public sealed class DroitAvecStatut
{
    public Droit Droit { get; set; } = null!;
    public bool EstCocheDirectement { get; set; }
    public bool EstIncluViaGroupe { get; set; }
    public string? NomGroupeInclus { get; set; }
}

public sealed class GroupeDroitAvecStatut
{
    public GroupeDroit GroupeDroit { get; set; } = null!;
    public bool EstCoche { get; set; }
}

public sealed class RolePermissions
{
    public string RoleId { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
    public List<GroupeDroitAvecStatut> Groupes { get; set; } = [];
    public List<DroitAvecStatut> Droits { get; set; } = [];
}

public interface IPermissionService
{
    Task<RolePermissions?> GetRolePermissionsAsync(string roleId);

    /// <summary>Remplace integralement les affectations directes du role (droits individuels + groupes).</summary>
    Task<(bool Success, string? ErrorMessage)> UpdateRolePermissionsAsync(string roleId, List<int> droitIds, List<int> groupeDroitIds);

    /// <summary>
    /// Codes des droits effectifs pour un ensemble de roles (union des droits directs et de ceux
    /// inclus via un groupe attribue). Utilise pour l'autorisation runtime (claims transformation).
    /// </summary>
    Task<List<string>> GetEffectiveDroitCodesAsync(IReadOnlyCollection<string> roleIds);
}

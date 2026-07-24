using Domain.Entities;

namespace Web.Data;

// Cle composite (RoleId, DroitId) configuree dans ApplicationDbContext.OnModelCreating
public class RoleDroit
{
    public string RoleId { get; set; } = null!;
    public ApplicationRole Role { get; set; } = null!;

    public int DroitId { get; set; }
    public Droit Droit { get; set; } = null!;
}

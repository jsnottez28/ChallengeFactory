using Domain.Entities;

namespace Web.Data;

// Cle composite (RoleId, GroupeDroitId) configuree dans ApplicationDbContext.OnModelCreating
public class RoleGroupeDroit
{
    public string RoleId { get; set; } = null!;
    public ApplicationRole Role { get; set; } = null!;

    public int GroupeDroitId { get; set; }
    public GroupeDroit GroupeDroit { get; set; } = null!;
}

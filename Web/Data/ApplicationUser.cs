using Microsoft.AspNetCore.Identity;
using Web.Data.Entities;

namespace Web.Data;

public enum StatutUtilisateur
{
    Actif,
    Modere,
    Inactif
}

public class ApplicationUser : IdentityUser
{
    public int? CiviliteId { get; set; }
    public Civilite? Civilite { get; set; }

    public string? Prenom { get; set; }
    public string? Nom { get; set; }
    public StatutUtilisateur Statut { get; set; } = StatutUtilisateur.Actif;

    public string? Telephone { get; set; }
    public DateTime CreeLe { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Compte de secours qui contourne toute verification de droit/role (voir
    /// DroitsClaimsTransformation et DroitAuthorizationHandler). Ne jamais exposer ce
    /// champ dans un formulaire d'administration : a positionner uniquement en base par
    /// un DBA/administrateur technique, pour eviter toute elevation de privilege.
    /// </summary>
    public bool EstSuperAdministrateur { get; set; }

    public ICollection<Rattachement> Rattachements { get; set; } = new List<Rattachement>();
}

public class Civilite
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string LibelleCourt { get; set; } = string.Empty;
    public string LibelleLong { get; set; } = string.Empty;
    public string LibelleArticle { get; set; } = string.Empty;
    public int Ordre { get; set; }
    public bool EstActif { get; set; } = true;
}

public class Rattachement
{
    public int Id { get; set; }
    public string ApplicationUserId { get; set; } = string.Empty;
    public ApplicationUser ApplicationUser { get; set; } = null!;
    public int OrganisationId { get; set; }
    public Organisation Organisation { get; set; } = null!;
    public int? FonctionId { get; set; }
    public Fonction? Fonction { get; set; }
    public bool EstActif { get; set; } = true;
    public DateTime CreeLe { get; set; } = DateTime.UtcNow;
}
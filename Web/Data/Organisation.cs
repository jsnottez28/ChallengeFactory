namespace Web.Data.Entities;

public class Organisation
{
    public int Id { get; set; }

    public string CodeAdherent { get; set; } = string.Empty; // clé Eudonet
    public string RaisonSociale { get; set; } = string.Empty;

    public string? Adresse1 { get; set; }
    public string? Adresse2 { get; set; }
    public string? CodePostal { get; set; }
    public string? Ville { get; set; }
    public string? TelephoneStandard { get; set; }

    public bool EstActif { get; set; } = true;
    public DateTime CreeLe { get; set; } = DateTime.UtcNow;

    public ICollection<Rattachement> Rattachements { get; set; } = new List<Rattachement>();
}

public class Fonction
{
    public int Id { get; set; }

    public string Libelle { get; set; } = string.Empty; // ex: "Directeur Général", "Président"

    public bool EstActif { get; set; } = true;

    // Pas de lien vers Role pour l'instant — sera ajouté plus tard
    // (ex: string? RoleId + FK vers AspNetRoles) une fois le besoin précisé
}

public class Scope
{
    public int Id { get; set; }

    public string ApplicationUserId { get; set; } = string.Empty;
    public ApplicationUser ApplicationUser { get; set; } = null!;

    public int? OrganisationId { get; set; }   // rempli = voit les données de cette organisation
    public Organisation? Organisation { get; set; }

    //public int? CongresId { get; set; }         // rempli = voit les données de ce congrès
    //public Congres? Congres { get; set; }

    public bool EstActif { get; set; } = true;
    public DateTime CreeLe { get; set; } = DateTime.UtcNow;
}
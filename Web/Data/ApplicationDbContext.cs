using Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Web.Data;
using Web.Data.Entities;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser, ApplicationRole, string>(options)
{
    public DbSet<Civilite> Civilites { get; set; }
    public DbSet<Organisation> Organisations { get; set; }
    public DbSet<Rattachement> Rattachements { get; set; }
    public DbSet<Fonction> Fonctions { get; set; }
    public DbSet<Scope> Scopes { get; set; }
    public DbSet<Ressource> Ressources { get; set; }
    public DbSet<TypeAction> TypesAction { get; set; }
    public DbSet<GroupeDroit> GroupesDroit { get; set; }
    public DbSet<Droit> Droits { get; set; }
    public DbSet<RoleDroit> RoleDroits { get; set; }
    public DbSet<RoleGroupeDroit> RoleGroupesDroit { get; set; }
    public DbSet<DocumentLegal> DocumentsLegaux { get; set; }
    public DbSet<AcceptationDocumentLegal> AcceptationsDocumentsLegaux { get; set; }
    public DbSet<Badge> Badges { get; set; }
    public DbSet<CarteCompetence> CartesCompetences { get; set; }
    public DbSet<CarteAttribution> CarteAttributions { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Civilite>().HasData(
            new Civilite
            {
                Id = 1,
                Code = "M",
                LibelleCourt = "M.",
                LibelleLong = "Monsieur",
                LibelleArticle = "à Monsieur",
                Ordre = 1,
                EstActif = true
            },
            new Civilite
            {
                Id = 2,
                Code = "MME",
                LibelleCourt = "Mme",
                LibelleLong = "Madame",
                LibelleArticle = "à Madame",
                Ordre = 2,
                EstActif = true
            }
        );

        builder.Entity<Rattachement>()
            .HasOne(r => r.ApplicationUser)
            .WithMany(u => u.Rattachements)
            .HasForeignKey(r => r.ApplicationUserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Rattachement>()
            .HasOne(r => r.Organisation)
            .WithMany(o => o.Rattachements)
            .HasForeignKey(r => r.OrganisationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Scope>()
            .HasOne(s => s.ApplicationUser)
            .WithMany() // ou .WithMany(u => u.Scopes) si tu ajoutes la navigation inverse
            .HasForeignKey(s => s.ApplicationUserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Scope>()
            .HasOne(s => s.Organisation)
            .WithMany()
            .HasForeignKey(s => s.OrganisationId)
            .OnDelete(DeleteBehavior.Restrict); // évite le cascade delete en chaîne

        ConfigureDroitsEtPermissions(builder);
        ConfigureDocumentsLegaux(builder);
        ConfigureCartesCompetences(builder);
    }

    private static void ConfigureDroitsEtPermissions(ModelBuilder builder)
    {
        builder.Entity<Droit>()
            .HasIndex(d => new { d.RessourceId, d.TypeActionId })
            .IsUnique();

        builder.Entity<Droit>()
            .HasOne(d => d.Ressource)
            .WithMany(r => r.Droits)
            .HasForeignKey(d => d.RessourceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Droit>()
            .HasOne(d => d.TypeAction)
            .WithMany(t => t.Droits)
            .HasForeignKey(d => d.TypeActionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Droit>()
            .HasOne(d => d.GroupeDroit)
            .WithMany(g => g.Droits)
            .HasForeignKey(d => d.GroupeDroitId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<RoleDroit>()
            .HasKey(rd => new { rd.RoleId, rd.DroitId });

        builder.Entity<RoleDroit>()
            .HasOne(rd => rd.Role)
            .WithMany()
            .HasForeignKey(rd => rd.RoleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<RoleDroit>()
            .HasOne(rd => rd.Droit)
            .WithMany()
            .HasForeignKey(rd => rd.DroitId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<RoleGroupeDroit>()
            .HasKey(rg => new { rg.RoleId, rg.GroupeDroitId });

        builder.Entity<RoleGroupeDroit>()
            .HasOne(rg => rg.Role)
            .WithMany()
            .HasForeignKey(rg => rg.RoleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<RoleGroupeDroit>()
            .HasOne(rg => rg.GroupeDroit)
            .WithMany()
            .HasForeignKey(rg => rg.GroupeDroitId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Ressource>().HasData(
            new Ressource { Id = 1, Code = "CONGRES", Libelle = "Congrès" },
            new Ressource { Id = 2, Code = "COMMANDE", Libelle = "Commande" },
            new Ressource { Id = 3, Code = "UTILISATEUR", Libelle = "Utilisateur" },
            new Ressource { Id = 4, Code = "ORGANISATION", Libelle = "Organisation" }
        );

        builder.Entity<TypeAction>().HasData(
            new TypeAction { Id = 1, Code = "CONSULTER", Libelle = "Consulter" },
            new TypeAction { Id = 2, Code = "CREER", Libelle = "Créer" },
            new TypeAction { Id = 3, Code = "MODIFIER", Libelle = "Modifier" },
            new TypeAction { Id = 4, Code = "SUPPRIMER", Libelle = "Supprimer" },
            new TypeAction { Id = 5, Code = "VALIDER", Libelle = "Valider" }
        );

        // Les droits ORGANISATION.* ne sont pas seedes ici via HasData : contrairement a
        // Ressource/TypeAction (seedees dans la meme migration que la creation de leur
        // table, donc garanties vides), la table Droits recoit aussi des ecritures live
        // depuis l'ecran /Administration/Droits. Un HasData a Id fixe entrerait en
        // collision avec l'auto-incrementation IDENTITY des qu'un droit est cree
        // manuellement avant l'application du seed. Voir la migration
        // SeedDroitsOrganisation, qui insere ces droits via SQL idempotent (par Code).
    }

    private static void ConfigureDocumentsLegaux(ModelBuilder builder)
    {
        builder.Entity<DocumentLegal>()
            .HasIndex(d => new { d.Type, d.Version })
            .IsUnique();

        builder.Entity<AcceptationDocumentLegal>()
            .HasOne(a => a.ApplicationUser)
            .WithMany()
            .HasForeignKey(a => a.ApplicationUserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<AcceptationDocumentLegal>()
            .HasOne(a => a.DocumentLegal)
            .WithMany()
            .HasForeignKey(a => a.DocumentLegalId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    private static void ConfigureCartesCompetences(ModelBuilder builder)
    {
        builder.Entity<Badge>()
            .HasIndex(b => b.BadgeCode)
            .IsUnique();

        builder.Entity<CarteCompetence>()
            .HasIndex(c => c.Code)
            .IsUnique();

        builder.Entity<CarteCompetence>()
            .HasOne(c => c.Badge)
            .WithMany()
            .HasForeignKey(c => c.BadgeId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<CarteAttribution>()
            .HasIndex(a => new { a.CarteCompetenceId, a.UtilisateurId })
            .IsUnique();

        builder.Entity<CarteAttribution>()
            .HasOne(a => a.CarteCompetence)
            .WithMany()
            .HasForeignKey(a => a.CarteCompetenceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<CarteAttribution>()
            .HasOne(a => a.Utilisateur)
            .WithMany()
            .HasForeignKey(a => a.UtilisateurId)
            .OnDelete(DeleteBehavior.Cascade);

        // Restrict (et non Cascade) : evite le conflit "multiple cascade paths" sur SQL
        // Server puisque CarteAttribution a deja un chemin de cascade vers ApplicationUser
        // via UtilisateurId. Supprimer le compte de l'attributeur ne doit de toute facon
        // pas effacer l'historique d'attribution.
        builder.Entity<CarteAttribution>()
            .HasOne(a => a.AttribuePar)
            .WithMany()
            .HasForeignKey(a => a.AttribueParId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

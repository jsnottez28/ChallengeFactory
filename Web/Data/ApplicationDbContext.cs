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
    public DbSet<Challenge> Challenges { get; set; }
    public DbSet<ChallengeEtape> ChallengeEtapes { get; set; }
    public DbSet<ChallengeEtapeCarte> ChallengeEtapeCartes { get; set; }
    public DbSet<Cohorte> Cohortes { get; set; }
    public DbSet<CohorteMembre> CohorteMembres { get; set; }
    public DbSet<CohorteEtapeValidation> CohorteEtapeValidations { get; set; }
    public DbSet<InvitationCompte> InvitationsComptes { get; set; }
    public DbSet<Preuve> Preuves { get; set; }
    public DbSet<PreuveFichier> PreuveFichiers { get; set; }
    public DbSet<PreuveValidationPair> PreuveValidationsPairs { get; set; }
    public DbSet<PreuveValidationGestionnaire> PreuveValidationsGestionnaire { get; set; }
    public DbSet<ForumMessage> ForumMessages { get; set; }
    public DbSet<ForumMessageUtile> ForumMessagesUtiles { get; set; }
    public DbSet<PointsEvenement> PointsEvenements { get; set; }
    public DbSet<BadgeSocialAttribution> BadgeSocialAttributions { get; set; }
    public DbSet<NotificationInApp> NotificationsInApp { get; set; }

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
        ConfigureChallenges(builder);
        ConfigurePreuvesPointsEtForum(builder);
        ConfigureNotifications(builder);
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

        // Pas d'unicite globale sur (CarteCompetenceId, UtilisateurId) seuls : une meme
        // carte peut desormais etre attribuee plusieurs fois au meme utilisateur si elle
        // provient d'origines differentes (Libre, ou plusieurs etapes/cohortes distinctes
        // du moteur de Challenges). L'unicite est scopee par origine : au plus une ligne
        // Libre par (carte, utilisateur), et au plus une ligne par (carte, utilisateur,
        // cohorte, etape) pour une origine Challenge - SQL Server traite les NULL comme
        // egaux dans un index unique, donc (Libre, null, null) reste bien unique par
        // (carte, utilisateur) comme avant cette extension.
        builder.Entity<CarteAttribution>()
            .HasIndex(a => new { a.CarteCompetenceId, a.UtilisateurId, a.OrigineType, a.CohorteId, a.ChallengeEtapeId })
            .IsUnique();

        builder.Entity<CarteAttribution>()
            .HasOne(a => a.Cohorte)
            .WithMany()
            .HasForeignKey(a => a.CohorteId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<CarteAttribution>()
            .HasOne(a => a.ChallengeEtape)
            .WithMany()
            .HasForeignKey(a => a.ChallengeEtapeId)
            .OnDelete(DeleteBehavior.Restrict);

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

    private static void ConfigureChallenges(ModelBuilder builder)
    {
        builder.Entity<Challenge>()
            .HasIndex(c => c.Code)
            .IsUnique();

        builder.Entity<ChallengeEtape>()
            .HasIndex(e => new { e.ChallengeId, e.NumeroEtape })
            .IsUnique();

        builder.Entity<ChallengeEtape>()
            .HasOne(e => e.Challenge)
            .WithMany(c => c.Etapes)
            .HasForeignKey(e => e.ChallengeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<ChallengeEtapeCarte>()
            .HasIndex(ec => new { ec.ChallengeEtapeId, ec.CarteCompetenceId })
            .IsUnique();

        builder.Entity<ChallengeEtapeCarte>()
            .HasOne(ec => ec.ChallengeEtape)
            .WithMany(e => e.Cartes)
            .HasForeignKey(ec => ec.ChallengeEtapeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<ChallengeEtapeCarte>()
            .HasOne(ec => ec.CarteCompetence)
            .WithMany()
            .HasForeignKey(ec => ec.CarteCompetenceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Cohorte>()
            .HasOne(c => c.Challenge)
            .WithMany()
            .HasForeignKey(c => c.ChallengeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Cohorte>()
            .HasOne(c => c.Organisation)
            .WithMany()
            .HasForeignKey(c => c.OrganisationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<CohorteMembre>()
            .HasIndex(m => new { m.CohorteId, m.UtilisateurId })
            .IsUnique();

        builder.Entity<CohorteMembre>()
            .HasOne(m => m.Cohorte)
            .WithMany(c => c.Membres)
            .HasForeignKey(m => m.CohorteId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<CohorteMembre>()
            .HasOne(m => m.Utilisateur)
            .WithMany()
            .HasForeignKey(m => m.UtilisateurId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<CohorteEtapeValidation>()
            .HasIndex(v => new { v.CohorteId, v.NumeroEtape })
            .IsUnique();

        builder.Entity<CohorteEtapeValidation>()
            .HasOne(v => v.Cohorte)
            .WithMany()
            .HasForeignKey(v => v.CohorteId)
            .OnDelete(DeleteBehavior.Cascade);

        // Restrict : l'historique de validation d'une Cohorte doit survivre a la
        // suppression du compte du Gestionnaire qui a valide (meme principe que
        // CarteAttribution.AttribuePar).
        builder.Entity<CohorteEtapeValidation>()
            .HasOne(v => v.ValidePar)
            .WithMany()
            .HasForeignKey(v => v.ValideParId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<InvitationCompte>()
            .HasIndex(i => i.Token)
            .IsUnique();

        builder.Entity<InvitationCompte>()
            .HasOne(i => i.Utilisateur)
            .WithMany()
            .HasForeignKey(i => i.UtilisateurId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    private static void ConfigurePreuvesPointsEtForum(ModelBuilder builder)
    {
        // Une seule Preuve par (Utilisateur, ChallengeEtape) : modifiee en place, jamais
        // recreee (cf. IPreuveService).
        builder.Entity<Preuve>()
            .HasIndex(p => new { p.UtilisateurId, p.ChallengeEtapeId })
            .IsUnique();

        builder.Entity<Preuve>()
            .HasOne(p => p.Utilisateur)
            .WithMany()
            .HasForeignKey(p => p.UtilisateurId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Preuve>()
            .HasOne(p => p.Cohorte)
            .WithMany()
            .HasForeignKey(p => p.CohorteId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Preuve>()
            .HasOne(p => p.ChallengeEtape)
            .WithMany()
            .HasForeignKey(p => p.ChallengeEtapeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<PreuveFichier>()
            .HasOne(f => f.Preuve)
            .WithMany(p => p.Fichiers)
            .HasForeignKey(f => f.PreuveId)
            .OnDelete(DeleteBehavior.Cascade);

        // Un meme pair ne peut voter qu'une fois par Preuve (modifier son avis met a jour
        // la ligne existante, cf. IPreuveService).
        builder.Entity<PreuveValidationPair>()
            .HasIndex(v => new { v.PreuveId, v.ValideurId })
            .IsUnique();

        builder.Entity<PreuveValidationPair>()
            .HasOne(v => v.Preuve)
            .WithMany(p => p.ValidationsPairs)
            .HasForeignKey(v => v.PreuveId)
            .OnDelete(DeleteBehavior.Cascade);

        // Restrict : le fil de retours recu par un auteur doit survivre a la suppression
        // du compte du pair qui a valide (meme principe que CohorteEtapeValidation.ValidePar).
        builder.Entity<PreuveValidationPair>()
            .HasOne(v => v.Valideur)
            .WithMany()
            .HasForeignKey(v => v.ValideurId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<PreuveValidationGestionnaire>()
            .HasOne(v => v.Preuve)
            .WithMany(p => p.ValidationsGestionnaire)
            .HasForeignKey(v => v.PreuveId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<PreuveValidationGestionnaire>()
            .HasOne(v => v.Valideur)
            .WithMany()
            .HasForeignKey(v => v.ValideurId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ForumMessage>()
            .HasOne(m => m.Cohorte)
            .WithMany()
            .HasForeignKey(m => m.CohorteId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ForumMessage>()
            .HasOne(m => m.ChallengeEtape)
            .WithMany()
            .HasForeignKey(m => m.ChallengeEtapeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ForumMessage>()
            .HasOne(m => m.Auteur)
            .WithMany()
            .HasForeignKey(m => m.AuteurId)
            .OnDelete(DeleteBehavior.Restrict);

        // Restrict (pas Cascade) : SQL Server refuse un cascade sur une FK
        // auto-referencee ("may cause cycles or multiple cascade paths"). La suppression
        // en cascade des reponses en fil lors d'une moderation est donc geree cote
        // application (cf. ForumService.SupprimerMessageAsync), pas par la base.
        builder.Entity<ForumMessage>()
            .HasOne(m => m.MessageParent)
            .WithMany(m => m.Reponses)
            .HasForeignKey(m => m.MessageParentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ForumMessageUtile>()
            .HasIndex(u => new { u.MessageId, u.MarqueParId })
            .IsUnique();

        builder.Entity<ForumMessageUtile>()
            .HasOne(u => u.Message)
            .WithMany(m => m.MarquagesUtile)
            .HasForeignKey(u => u.MessageId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<ForumMessageUtile>()
            .HasOne(u => u.MarquePar)
            .WithMany()
            .HasForeignKey(u => u.MarqueParId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<PointsEvenement>()
            .HasOne(e => e.Utilisateur)
            .WithMany()
            .HasForeignKey(e => e.UtilisateurId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<PointsEvenement>()
            .HasOne(e => e.Cohorte)
            .WithMany()
            .HasForeignKey(e => e.CohorteId)
            .OnDelete(DeleteBehavior.Restrict);

        // Idempotence : au plus un badge d'un type donne par (Utilisateur, Cohorte,
        // ChallengeEtape) - protege le calcul a la cloture d'etape contre un double appel.
        builder.Entity<BadgeSocialAttribution>()
            .HasIndex(b => new { b.UtilisateurId, b.CohorteId, b.ChallengeEtapeId, b.TypeBadge })
            .IsUnique();

        builder.Entity<BadgeSocialAttribution>()
            .HasOne(b => b.Utilisateur)
            .WithMany()
            .HasForeignKey(b => b.UtilisateurId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<BadgeSocialAttribution>()
            .HasOne(b => b.Cohorte)
            .WithMany()
            .HasForeignKey(b => b.CohorteId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<BadgeSocialAttribution>()
            .HasOne(b => b.ChallengeEtape)
            .WithMany()
            .HasForeignKey(b => b.ChallengeEtapeId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    private static void ConfigureNotifications(ModelBuilder builder)
    {
        builder.Entity<NotificationInApp>()
            .HasIndex(n => new { n.UtilisateurId, n.Lu, n.DateCreation });

        builder.Entity<NotificationInApp>()
            .HasOne(n => n.Utilisateur)
            .WithMany()
            .HasForeignKey(n => n.UtilisateurId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

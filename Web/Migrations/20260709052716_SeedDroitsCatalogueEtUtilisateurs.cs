using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Web.Migrations
{
    /// <inheritdoc />
    public partial class SeedDroitsCatalogueEtUtilisateurs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Meme principe que SeedDroitsOrganisation / SeedDroitsAdministrationRbac :
            // Ressources et Droits recoivent des ecritures live depuis les ecrans
            // /Administration/*, donc insertion idempotente par Code (pas de HasData).
            migrationBuilder.Sql("""
                IF NOT EXISTS (SELECT 1 FROM [Ressources] WHERE [Code] = N'RESSOURCE')
                INSERT INTO [Ressources] ([Code], [Libelle], [Description])
                VALUES (N'RESSOURCE', N'Ressource', NULL);

                IF NOT EXISTS (SELECT 1 FROM [Ressources] WHERE [Code] = N'TYPEACTION')
                INSERT INTO [Ressources] ([Code], [Libelle], [Description])
                VALUES (N'TYPEACTION', N'Type d''action', NULL);

                IF NOT EXISTS (SELECT 1 FROM [Ressources] WHERE [Code] = N'GROUPEDROIT')
                INSERT INTO [Ressources] ([Code], [Libelle], [Description])
                VALUES (N'GROUPEDROIT', N'Groupe de droits', NULL);
                """);

            migrationBuilder.Sql("""
                IF NOT EXISTS (SELECT 1 FROM [Droits] WHERE [Code] = N'RESSOURCE.CONSULTER')
                INSERT INTO [Droits] ([Code], [Libelle], [Description], [RessourceId], [TypeActionId], [GroupeDroitId])
                VALUES (N'RESSOURCE.CONSULTER', N'Consulter les ressources', NULL,
                    (SELECT [Id] FROM [Ressources] WHERE [Code] = N'RESSOURCE'),
                    (SELECT [Id] FROM [TypesAction] WHERE [Code] = N'CONSULTER'), NULL);

                IF NOT EXISTS (SELECT 1 FROM [Droits] WHERE [Code] = N'RESSOURCE.CREER')
                INSERT INTO [Droits] ([Code], [Libelle], [Description], [RessourceId], [TypeActionId], [GroupeDroitId])
                VALUES (N'RESSOURCE.CREER', N'Creer une ressource', NULL,
                    (SELECT [Id] FROM [Ressources] WHERE [Code] = N'RESSOURCE'),
                    (SELECT [Id] FROM [TypesAction] WHERE [Code] = N'CREER'), NULL);

                IF NOT EXISTS (SELECT 1 FROM [Droits] WHERE [Code] = N'RESSOURCE.MODIFIER')
                INSERT INTO [Droits] ([Code], [Libelle], [Description], [RessourceId], [TypeActionId], [GroupeDroitId])
                VALUES (N'RESSOURCE.MODIFIER', N'Modifier une ressource', NULL,
                    (SELECT [Id] FROM [Ressources] WHERE [Code] = N'RESSOURCE'),
                    (SELECT [Id] FROM [TypesAction] WHERE [Code] = N'MODIFIER'), NULL);

                IF NOT EXISTS (SELECT 1 FROM [Droits] WHERE [Code] = N'RESSOURCE.SUPPRIMER')
                INSERT INTO [Droits] ([Code], [Libelle], [Description], [RessourceId], [TypeActionId], [GroupeDroitId])
                VALUES (N'RESSOURCE.SUPPRIMER', N'Supprimer une ressource', NULL,
                    (SELECT [Id] FROM [Ressources] WHERE [Code] = N'RESSOURCE'),
                    (SELECT [Id] FROM [TypesAction] WHERE [Code] = N'SUPPRIMER'), NULL);

                IF NOT EXISTS (SELECT 1 FROM [Droits] WHERE [Code] = N'TYPEACTION.CONSULTER')
                INSERT INTO [Droits] ([Code], [Libelle], [Description], [RessourceId], [TypeActionId], [GroupeDroitId])
                VALUES (N'TYPEACTION.CONSULTER', N'Consulter les types d''action', NULL,
                    (SELECT [Id] FROM [Ressources] WHERE [Code] = N'TYPEACTION'),
                    (SELECT [Id] FROM [TypesAction] WHERE [Code] = N'CONSULTER'), NULL);

                IF NOT EXISTS (SELECT 1 FROM [Droits] WHERE [Code] = N'TYPEACTION.CREER')
                INSERT INTO [Droits] ([Code], [Libelle], [Description], [RessourceId], [TypeActionId], [GroupeDroitId])
                VALUES (N'TYPEACTION.CREER', N'Creer un type d''action', NULL,
                    (SELECT [Id] FROM [Ressources] WHERE [Code] = N'TYPEACTION'),
                    (SELECT [Id] FROM [TypesAction] WHERE [Code] = N'CREER'), NULL);

                IF NOT EXISTS (SELECT 1 FROM [Droits] WHERE [Code] = N'TYPEACTION.MODIFIER')
                INSERT INTO [Droits] ([Code], [Libelle], [Description], [RessourceId], [TypeActionId], [GroupeDroitId])
                VALUES (N'TYPEACTION.MODIFIER', N'Modifier un type d''action', NULL,
                    (SELECT [Id] FROM [Ressources] WHERE [Code] = N'TYPEACTION'),
                    (SELECT [Id] FROM [TypesAction] WHERE [Code] = N'MODIFIER'), NULL);

                IF NOT EXISTS (SELECT 1 FROM [Droits] WHERE [Code] = N'TYPEACTION.SUPPRIMER')
                INSERT INTO [Droits] ([Code], [Libelle], [Description], [RessourceId], [TypeActionId], [GroupeDroitId])
                VALUES (N'TYPEACTION.SUPPRIMER', N'Supprimer un type d''action', NULL,
                    (SELECT [Id] FROM [Ressources] WHERE [Code] = N'TYPEACTION'),
                    (SELECT [Id] FROM [TypesAction] WHERE [Code] = N'SUPPRIMER'), NULL);

                IF NOT EXISTS (SELECT 1 FROM [Droits] WHERE [Code] = N'GROUPEDROIT.CONSULTER')
                INSERT INTO [Droits] ([Code], [Libelle], [Description], [RessourceId], [TypeActionId], [GroupeDroitId])
                VALUES (N'GROUPEDROIT.CONSULTER', N'Consulter les groupes de droits', NULL,
                    (SELECT [Id] FROM [Ressources] WHERE [Code] = N'GROUPEDROIT'),
                    (SELECT [Id] FROM [TypesAction] WHERE [Code] = N'CONSULTER'), NULL);

                IF NOT EXISTS (SELECT 1 FROM [Droits] WHERE [Code] = N'GROUPEDROIT.CREER')
                INSERT INTO [Droits] ([Code], [Libelle], [Description], [RessourceId], [TypeActionId], [GroupeDroitId])
                VALUES (N'GROUPEDROIT.CREER', N'Creer un groupe de droits', NULL,
                    (SELECT [Id] FROM [Ressources] WHERE [Code] = N'GROUPEDROIT'),
                    (SELECT [Id] FROM [TypesAction] WHERE [Code] = N'CREER'), NULL);

                IF NOT EXISTS (SELECT 1 FROM [Droits] WHERE [Code] = N'GROUPEDROIT.MODIFIER')
                INSERT INTO [Droits] ([Code], [Libelle], [Description], [RessourceId], [TypeActionId], [GroupeDroitId])
                VALUES (N'GROUPEDROIT.MODIFIER', N'Modifier un groupe de droits', NULL,
                    (SELECT [Id] FROM [Ressources] WHERE [Code] = N'GROUPEDROIT'),
                    (SELECT [Id] FROM [TypesAction] WHERE [Code] = N'MODIFIER'), NULL);

                IF NOT EXISTS (SELECT 1 FROM [Droits] WHERE [Code] = N'GROUPEDROIT.SUPPRIMER')
                INSERT INTO [Droits] ([Code], [Libelle], [Description], [RessourceId], [TypeActionId], [GroupeDroitId])
                VALUES (N'GROUPEDROIT.SUPPRIMER', N'Supprimer un groupe de droits', NULL,
                    (SELECT [Id] FROM [Ressources] WHERE [Code] = N'GROUPEDROIT'),
                    (SELECT [Id] FROM [TypesAction] WHERE [Code] = N'SUPPRIMER'), NULL);

                IF NOT EXISTS (SELECT 1 FROM [Droits] WHERE [Code] = N'UTILISATEUR.CONSULTER')
                INSERT INTO [Droits] ([Code], [Libelle], [Description], [RessourceId], [TypeActionId], [GroupeDroitId])
                VALUES (N'UTILISATEUR.CONSULTER', N'Consulter les roles d''un utilisateur', NULL,
                    (SELECT [Id] FROM [Ressources] WHERE [Code] = N'UTILISATEUR'),
                    (SELECT [Id] FROM [TypesAction] WHERE [Code] = N'CONSULTER'), NULL);

                IF NOT EXISTS (SELECT 1 FROM [Droits] WHERE [Code] = N'UTILISATEUR.MODIFIER')
                INSERT INTO [Droits] ([Code], [Libelle], [Description], [RessourceId], [TypeActionId], [GroupeDroitId])
                VALUES (N'UTILISATEUR.MODIFIER', N'Modifier les roles d''un utilisateur', NULL,
                    (SELECT [Id] FROM [Ressources] WHERE [Code] = N'UTILISATEUR'),
                    (SELECT [Id] FROM [TypesAction] WHERE [Code] = N'MODIFIER'), NULL);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DELETE FROM [Droits]
                WHERE [Code] IN (
                    N'RESSOURCE.CONSULTER', N'RESSOURCE.CREER', N'RESSOURCE.MODIFIER', N'RESSOURCE.SUPPRIMER',
                    N'TYPEACTION.CONSULTER', N'TYPEACTION.CREER', N'TYPEACTION.MODIFIER', N'TYPEACTION.SUPPRIMER',
                    N'GROUPEDROIT.CONSULTER', N'GROUPEDROIT.CREER', N'GROUPEDROIT.MODIFIER', N'GROUPEDROIT.SUPPRIMER',
                    N'UTILISATEUR.CONSULTER', N'UTILISATEUR.MODIFIER');
                """);

            migrationBuilder.Sql("""
                DELETE FROM [Ressources]
                WHERE [Code] IN (N'RESSOURCE', N'TYPEACTION', N'GROUPEDROIT');
                """);
        }
    }
}

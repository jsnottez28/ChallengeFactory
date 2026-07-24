using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Web.Migrations
{
    /// <inheritdoc />
    public partial class SeedDroitsAdministrationRbac : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Comme pour SeedDroitsOrganisation : Ressources et Droits recoivent des
            // ecritures live depuis /Administration/Ressources et /Administration/Droits,
            // donc insertion idempotente par Code (pas de HasData a Id fixe).
            migrationBuilder.Sql("""
                IF NOT EXISTS (SELECT 1 FROM [Ressources] WHERE [Code] = N'ROLE')
                INSERT INTO [Ressources] ([Code], [Libelle], [Description])
                VALUES (N'ROLE', N'Role', NULL);

                IF NOT EXISTS (SELECT 1 FROM [Ressources] WHERE [Code] = N'DROIT')
                INSERT INTO [Ressources] ([Code], [Libelle], [Description])
                VALUES (N'DROIT', N'Droit', NULL);

                IF NOT EXISTS (SELECT 1 FROM [Ressources] WHERE [Code] = N'PERMISSION')
                INSERT INTO [Ressources] ([Code], [Libelle], [Description])
                VALUES (N'PERMISSION', N'Permission', NULL);
                """);

            migrationBuilder.Sql("""
                IF NOT EXISTS (SELECT 1 FROM [Droits] WHERE [Code] = N'ROLE.CONSULTER')
                INSERT INTO [Droits] ([Code], [Libelle], [Description], [RessourceId], [TypeActionId], [GroupeDroitId])
                VALUES (N'ROLE.CONSULTER', N'Consulter les roles', NULL,
                    (SELECT [Id] FROM [Ressources] WHERE [Code] = N'ROLE'),
                    (SELECT [Id] FROM [TypesAction] WHERE [Code] = N'CONSULTER'), NULL);

                IF NOT EXISTS (SELECT 1 FROM [Droits] WHERE [Code] = N'ROLE.CREER')
                INSERT INTO [Droits] ([Code], [Libelle], [Description], [RessourceId], [TypeActionId], [GroupeDroitId])
                VALUES (N'ROLE.CREER', N'Creer un role', NULL,
                    (SELECT [Id] FROM [Ressources] WHERE [Code] = N'ROLE'),
                    (SELECT [Id] FROM [TypesAction] WHERE [Code] = N'CREER'), NULL);

                IF NOT EXISTS (SELECT 1 FROM [Droits] WHERE [Code] = N'ROLE.MODIFIER')
                INSERT INTO [Droits] ([Code], [Libelle], [Description], [RessourceId], [TypeActionId], [GroupeDroitId])
                VALUES (N'ROLE.MODIFIER', N'Modifier un role', NULL,
                    (SELECT [Id] FROM [Ressources] WHERE [Code] = N'ROLE'),
                    (SELECT [Id] FROM [TypesAction] WHERE [Code] = N'MODIFIER'), NULL);

                IF NOT EXISTS (SELECT 1 FROM [Droits] WHERE [Code] = N'ROLE.SUPPRIMER')
                INSERT INTO [Droits] ([Code], [Libelle], [Description], [RessourceId], [TypeActionId], [GroupeDroitId])
                VALUES (N'ROLE.SUPPRIMER', N'Supprimer un role', NULL,
                    (SELECT [Id] FROM [Ressources] WHERE [Code] = N'ROLE'),
                    (SELECT [Id] FROM [TypesAction] WHERE [Code] = N'SUPPRIMER'), NULL);

                IF NOT EXISTS (SELECT 1 FROM [Droits] WHERE [Code] = N'DROIT.CONSULTER')
                INSERT INTO [Droits] ([Code], [Libelle], [Description], [RessourceId], [TypeActionId], [GroupeDroitId])
                VALUES (N'DROIT.CONSULTER', N'Consulter les droits', NULL,
                    (SELECT [Id] FROM [Ressources] WHERE [Code] = N'DROIT'),
                    (SELECT [Id] FROM [TypesAction] WHERE [Code] = N'CONSULTER'), NULL);

                IF NOT EXISTS (SELECT 1 FROM [Droits] WHERE [Code] = N'DROIT.CREER')
                INSERT INTO [Droits] ([Code], [Libelle], [Description], [RessourceId], [TypeActionId], [GroupeDroitId])
                VALUES (N'DROIT.CREER', N'Creer un droit', NULL,
                    (SELECT [Id] FROM [Ressources] WHERE [Code] = N'DROIT'),
                    (SELECT [Id] FROM [TypesAction] WHERE [Code] = N'CREER'), NULL);

                IF NOT EXISTS (SELECT 1 FROM [Droits] WHERE [Code] = N'DROIT.MODIFIER')
                INSERT INTO [Droits] ([Code], [Libelle], [Description], [RessourceId], [TypeActionId], [GroupeDroitId])
                VALUES (N'DROIT.MODIFIER', N'Modifier un droit', NULL,
                    (SELECT [Id] FROM [Ressources] WHERE [Code] = N'DROIT'),
                    (SELECT [Id] FROM [TypesAction] WHERE [Code] = N'MODIFIER'), NULL);

                IF NOT EXISTS (SELECT 1 FROM [Droits] WHERE [Code] = N'DROIT.SUPPRIMER')
                INSERT INTO [Droits] ([Code], [Libelle], [Description], [RessourceId], [TypeActionId], [GroupeDroitId])
                VALUES (N'DROIT.SUPPRIMER', N'Supprimer un droit', NULL,
                    (SELECT [Id] FROM [Ressources] WHERE [Code] = N'DROIT'),
                    (SELECT [Id] FROM [TypesAction] WHERE [Code] = N'SUPPRIMER'), NULL);

                IF NOT EXISTS (SELECT 1 FROM [Droits] WHERE [Code] = N'PERMISSION.CONSULTER')
                INSERT INTO [Droits] ([Code], [Libelle], [Description], [RessourceId], [TypeActionId], [GroupeDroitId])
                VALUES (N'PERMISSION.CONSULTER', N'Consulter les permissions', NULL,
                    (SELECT [Id] FROM [Ressources] WHERE [Code] = N'PERMISSION'),
                    (SELECT [Id] FROM [TypesAction] WHERE [Code] = N'CONSULTER'), NULL);

                IF NOT EXISTS (SELECT 1 FROM [Droits] WHERE [Code] = N'PERMISSION.MODIFIER')
                INSERT INTO [Droits] ([Code], [Libelle], [Description], [RessourceId], [TypeActionId], [GroupeDroitId])
                VALUES (N'PERMISSION.MODIFIER', N'Modifier les permissions', NULL,
                    (SELECT [Id] FROM [Ressources] WHERE [Code] = N'PERMISSION'),
                    (SELECT [Id] FROM [TypesAction] WHERE [Code] = N'MODIFIER'), NULL);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DELETE FROM [Droits]
                WHERE [Code] IN (
                    N'ROLE.CONSULTER', N'ROLE.CREER', N'ROLE.MODIFIER', N'ROLE.SUPPRIMER',
                    N'DROIT.CONSULTER', N'DROIT.CREER', N'DROIT.MODIFIER', N'DROIT.SUPPRIMER',
                    N'PERMISSION.CONSULTER', N'PERMISSION.MODIFIER');
                """);

            migrationBuilder.Sql("""
                DELETE FROM [Ressources]
                WHERE [Code] IN (N'ROLE', N'DROIT', N'PERMISSION');
                """);
        }
    }
}

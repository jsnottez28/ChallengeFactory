using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Web.Migrations
{
    /// <inheritdoc />
    public partial class SeedDroitsPreBilanCarbone : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Meme principe que SeedDroitsReclamations : insertion idempotente par Code
            // (pas de HasData). CONSULTER pour la liste/le detail CRM, MODIFIER pour le
            // changement de statut commercial - les Pre-diagnostics eux-memes sont
            // toujours deposes par le public, jamais crees/modifies/supprimes par un
            // Gestionnaire.
            migrationBuilder.Sql("""
                IF NOT EXISTS (SELECT 1 FROM [Ressources] WHERE [Code] = N'PREBILAN')
                INSERT INTO [Ressources] ([Code], [Libelle], [Description])
                VALUES (N'PREBILAN', N'Pré-diagnostic carbone', NULL);
                """);

            migrationBuilder.Sql("""
                IF NOT EXISTS (SELECT 1 FROM [Droits] WHERE [Code] = N'PREBILAN.CONSULTER')
                INSERT INTO [Droits] ([Code], [Libelle], [Description], [RessourceId], [TypeActionId], [GroupeDroitId])
                VALUES (N'PREBILAN.CONSULTER', N'Consulter les pré-diagnostics carbone', NULL,
                    (SELECT [Id] FROM [Ressources] WHERE [Code] = N'PREBILAN'),
                    (SELECT [Id] FROM [TypesAction] WHERE [Code] = N'CONSULTER'), NULL);

                IF NOT EXISTS (SELECT 1 FROM [Droits] WHERE [Code] = N'PREBILAN.MODIFIER')
                INSERT INTO [Droits] ([Code], [Libelle], [Description], [RessourceId], [TypeActionId], [GroupeDroitId])
                VALUES (N'PREBILAN.MODIFIER', N'Changer le statut commercial d''un pré-diagnostic', NULL,
                    (SELECT [Id] FROM [Ressources] WHERE [Code] = N'PREBILAN'),
                    (SELECT [Id] FROM [TypesAction] WHERE [Code] = N'MODIFIER'), NULL);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DELETE FROM [Droits]
                WHERE [Code] IN (N'PREBILAN.CONSULTER', N'PREBILAN.MODIFIER');
                """);

            migrationBuilder.Sql("""
                DELETE FROM [Ressources]
                WHERE [Code] = N'PREBILAN';
                """);
        }
    }
}

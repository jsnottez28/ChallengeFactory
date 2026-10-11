using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Web.Migrations
{
    /// <inheritdoc />
    public partial class SeedDroitsQualiopi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Meme principe que SeedDroitsPreBilanCarbone : insertion idempotente par Code
            // (pas de HasData). Un seul droit CONSULTER : le tableau de bord est en lecture
            // seule, il n'ecrit jamais sur les donnees qu'il agrege.
            migrationBuilder.Sql("""
                IF NOT EXISTS (SELECT 1 FROM [Ressources] WHERE [Code] = N'QUALIOPI')
                INSERT INTO [Ressources] ([Code], [Libelle], [Description])
                VALUES (N'QUALIOPI', N'Tableau de bord Qualiopi', NULL);
                """);

            migrationBuilder.Sql("""
                IF NOT EXISTS (SELECT 1 FROM [Droits] WHERE [Code] = N'QUALIOPI.CONSULTER')
                INSERT INTO [Droits] ([Code], [Libelle], [Description], [RessourceId], [TypeActionId], [GroupeDroitId])
                VALUES (N'QUALIOPI.CONSULTER', N'Consulter le tableau de bord Qualiopi', NULL,
                    (SELECT [Id] FROM [Ressources] WHERE [Code] = N'QUALIOPI'),
                    (SELECT [Id] FROM [TypesAction] WHERE [Code] = N'CONSULTER'), NULL);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DELETE FROM [Droits]
                WHERE [Code] = N'QUALIOPI.CONSULTER';
                """);

            migrationBuilder.Sql("""
                DELETE FROM [Ressources]
                WHERE [Code] = N'QUALIOPI';
                """);
        }
    }
}

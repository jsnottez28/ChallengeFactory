using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Web.Migrations
{
    /// <inheritdoc />
    public partial class SeedDroitsScope : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Meme principe que les migrations SeedDroits* precedentes : Ressources et
            // Droits recoivent des ecritures live depuis les ecrans /Administration/*,
            // donc insertion idempotente par Code (pas de HasData a Id fixe).
            migrationBuilder.Sql("""
                IF NOT EXISTS (SELECT 1 FROM [Ressources] WHERE [Code] = N'SCOPE')
                INSERT INTO [Ressources] ([Code], [Libelle], [Description])
                VALUES (N'SCOPE', N'Périmètre d''accès', NULL);
                """);

            migrationBuilder.Sql("""
                IF NOT EXISTS (SELECT 1 FROM [Droits] WHERE [Code] = N'SCOPE.CONSULTER')
                INSERT INTO [Droits] ([Code], [Libelle], [Description], [RessourceId], [TypeActionId], [GroupeDroitId])
                VALUES (N'SCOPE.CONSULTER', N'Consulter les périmètres d''accès', NULL,
                    (SELECT [Id] FROM [Ressources] WHERE [Code] = N'SCOPE'),
                    (SELECT [Id] FROM [TypesAction] WHERE [Code] = N'CONSULTER'), NULL);

                IF NOT EXISTS (SELECT 1 FROM [Droits] WHERE [Code] = N'SCOPE.MODIFIER')
                INSERT INTO [Droits] ([Code], [Libelle], [Description], [RessourceId], [TypeActionId], [GroupeDroitId])
                VALUES (N'SCOPE.MODIFIER', N'Modifier les périmètres d''accès', NULL,
                    (SELECT [Id] FROM [Ressources] WHERE [Code] = N'SCOPE'),
                    (SELECT [Id] FROM [TypesAction] WHERE [Code] = N'MODIFIER'), NULL);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DELETE FROM [Droits]
                WHERE [Code] IN (N'SCOPE.CONSULTER', N'SCOPE.MODIFIER');
                """);

            migrationBuilder.Sql("""
                DELETE FROM [Ressources]
                WHERE [Code] = N'SCOPE';
                """);
        }
    }
}

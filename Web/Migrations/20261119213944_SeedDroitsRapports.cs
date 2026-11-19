using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Web.Migrations
{
    /// <inheritdoc />
    public partial class SeedDroitsRapports : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Meme principe que SeedDroitsArticles : insertion idempotente par Code
            // (pas de HasData), car Ressources/Droits recoivent aussi des ecritures live
            // depuis /Administration/*.
            migrationBuilder.Sql("""
                IF NOT EXISTS (SELECT 1 FROM [Ressources] WHERE [Code] = N'RAPPORT')
                INSERT INTO [Ressources] ([Code], [Libelle], [Description])
                VALUES (N'RAPPORT', N'Rapport (étude de cas Bilan Carbone/ACV)', NULL);
                """);

            migrationBuilder.Sql("""
                IF NOT EXISTS (SELECT 1 FROM [Droits] WHERE [Code] = N'RAPPORT.CONSULTER')
                INSERT INTO [Droits] ([Code], [Libelle], [Description], [RessourceId], [TypeActionId], [GroupeDroitId])
                VALUES (N'RAPPORT.CONSULTER', N'Consulter les rapports (back-office)', NULL,
                    (SELECT [Id] FROM [Ressources] WHERE [Code] = N'RAPPORT'),
                    (SELECT [Id] FROM [TypesAction] WHERE [Code] = N'CONSULTER'), NULL);

                IF NOT EXISTS (SELECT 1 FROM [Droits] WHERE [Code] = N'RAPPORT.CREER')
                INSERT INTO [Droits] ([Code], [Libelle], [Description], [RessourceId], [TypeActionId], [GroupeDroitId])
                VALUES (N'RAPPORT.CREER', N'Créer un rapport', NULL,
                    (SELECT [Id] FROM [Ressources] WHERE [Code] = N'RAPPORT'),
                    (SELECT [Id] FROM [TypesAction] WHERE [Code] = N'CREER'), NULL);

                IF NOT EXISTS (SELECT 1 FROM [Droits] WHERE [Code] = N'RAPPORT.MODIFIER')
                INSERT INTO [Droits] ([Code], [Libelle], [Description], [RessourceId], [TypeActionId], [GroupeDroitId])
                VALUES (N'RAPPORT.MODIFIER', N'Modifier un rapport (inclut changer son statut)', NULL,
                    (SELECT [Id] FROM [Ressources] WHERE [Code] = N'RAPPORT'),
                    (SELECT [Id] FROM [TypesAction] WHERE [Code] = N'MODIFIER'), NULL);

                IF NOT EXISTS (SELECT 1 FROM [Droits] WHERE [Code] = N'RAPPORT.SUPPRIMER')
                INSERT INTO [Droits] ([Code], [Libelle], [Description], [RessourceId], [TypeActionId], [GroupeDroitId])
                VALUES (N'RAPPORT.SUPPRIMER', N'Supprimer un rapport', NULL,
                    (SELECT [Id] FROM [Ressources] WHERE [Code] = N'RAPPORT'),
                    (SELECT [Id] FROM [TypesAction] WHERE [Code] = N'SUPPRIMER'), NULL);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DELETE FROM [Droits]
                WHERE [Code] IN (N'RAPPORT.CONSULTER', N'RAPPORT.CREER', N'RAPPORT.MODIFIER', N'RAPPORT.SUPPRIMER');
                """);

            migrationBuilder.Sql("""
                DELETE FROM [Ressources]
                WHERE [Code] IN (N'RAPPORT');
                """);
        }
    }
}

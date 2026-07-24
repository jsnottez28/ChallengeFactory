using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Web.Migrations
{
    /// <inheritdoc />
    public partial class SeedDroitsCartesCompetences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Meme principe que SeedDroitsCatalogueEtUtilisateurs : Ressources et Droits
            // recoivent des ecritures live depuis les ecrans /Administration/*, donc
            // insertion idempotente par Code (pas de HasData).
            migrationBuilder.Sql("""
                IF NOT EXISTS (SELECT 1 FROM [Ressources] WHERE [Code] = N'CARTE')
                INSERT INTO [Ressources] ([Code], [Libelle], [Description])
                VALUES (N'CARTE', N'Carte de Compétences', NULL);
                """);

            migrationBuilder.Sql("""
                IF NOT EXISTS (SELECT 1 FROM [Droits] WHERE [Code] = N'CARTE.CONSULTER')
                INSERT INTO [Droits] ([Code], [Libelle], [Description], [RessourceId], [TypeActionId], [GroupeDroitId])
                VALUES (N'CARTE.CONSULTER', N'Consulter les cartes de compétences', NULL,
                    (SELECT [Id] FROM [Ressources] WHERE [Code] = N'CARTE'),
                    (SELECT [Id] FROM [TypesAction] WHERE [Code] = N'CONSULTER'), NULL);

                IF NOT EXISTS (SELECT 1 FROM [Droits] WHERE [Code] = N'CARTE.CREER')
                INSERT INTO [Droits] ([Code], [Libelle], [Description], [RessourceId], [TypeActionId], [GroupeDroitId])
                VALUES (N'CARTE.CREER', N'Créer une carte de compétences', NULL,
                    (SELECT [Id] FROM [Ressources] WHERE [Code] = N'CARTE'),
                    (SELECT [Id] FROM [TypesAction] WHERE [Code] = N'CREER'), NULL);

                IF NOT EXISTS (SELECT 1 FROM [Droits] WHERE [Code] = N'CARTE.MODIFIER')
                INSERT INTO [Droits] ([Code], [Libelle], [Description], [RessourceId], [TypeActionId], [GroupeDroitId])
                VALUES (N'CARTE.MODIFIER', N'Modifier une carte de compétences (inclut l''attribution/désattribution)', NULL,
                    (SELECT [Id] FROM [Ressources] WHERE [Code] = N'CARTE'),
                    (SELECT [Id] FROM [TypesAction] WHERE [Code] = N'MODIFIER'), NULL);

                IF NOT EXISTS (SELECT 1 FROM [Droits] WHERE [Code] = N'CARTE.SUPPRIMER')
                INSERT INTO [Droits] ([Code], [Libelle], [Description], [RessourceId], [TypeActionId], [GroupeDroitId])
                VALUES (N'CARTE.SUPPRIMER', N'Supprimer une carte de compétences', NULL,
                    (SELECT [Id] FROM [Ressources] WHERE [Code] = N'CARTE'),
                    (SELECT [Id] FROM [TypesAction] WHERE [Code] = N'SUPPRIMER'), NULL);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DELETE FROM [Droits]
                WHERE [Code] IN (N'CARTE.CONSULTER', N'CARTE.CREER', N'CARTE.MODIFIER', N'CARTE.SUPPRIMER');
                """);

            migrationBuilder.Sql("""
                DELETE FROM [Ressources]
                WHERE [Code] IN (N'CARTE');
                """);
        }
    }
}

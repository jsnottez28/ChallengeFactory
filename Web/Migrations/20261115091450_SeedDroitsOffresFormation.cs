using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Web.Migrations
{
    /// <inheritdoc />
    public partial class SeedDroitsOffresFormation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Meme principe que SeedDroitsArticles : insertion idempotente par Code (pas de
            // HasData), car Ressources/Droits recoivent aussi des ecritures live depuis
            // /Administration/*.
            migrationBuilder.Sql("""
                IF NOT EXISTS (SELECT 1 FROM [Ressources] WHERE [Code] = N'OFFREFORMATION')
                INSERT INTO [Ressources] ([Code], [Libelle], [Description])
                VALUES (N'OFFREFORMATION', N'Offre de formation (vitrine catalogue)', NULL);
                """);

            migrationBuilder.Sql("""
                IF NOT EXISTS (SELECT 1 FROM [Droits] WHERE [Code] = N'OFFREFORMATION.CONSULTER')
                INSERT INTO [Droits] ([Code], [Libelle], [Description], [RessourceId], [TypeActionId], [GroupeDroitId])
                VALUES (N'OFFREFORMATION.CONSULTER', N'Consulter le catalogue des offres de formation (back-office)', NULL,
                    (SELECT [Id] FROM [Ressources] WHERE [Code] = N'OFFREFORMATION'),
                    (SELECT [Id] FROM [TypesAction] WHERE [Code] = N'CONSULTER'), NULL);

                IF NOT EXISTS (SELECT 1 FROM [Droits] WHERE [Code] = N'OFFREFORMATION.CREER')
                INSERT INTO [Droits] ([Code], [Libelle], [Description], [RessourceId], [TypeActionId], [GroupeDroitId])
                VALUES (N'OFFREFORMATION.CREER', N'Créer une offre de formation', NULL,
                    (SELECT [Id] FROM [Ressources] WHERE [Code] = N'OFFREFORMATION'),
                    (SELECT [Id] FROM [TypesAction] WHERE [Code] = N'CREER'), NULL);

                IF NOT EXISTS (SELECT 1 FROM [Droits] WHERE [Code] = N'OFFREFORMATION.MODIFIER')
                INSERT INTO [Droits] ([Code], [Libelle], [Description], [RessourceId], [TypeActionId], [GroupeDroitId])
                VALUES (N'OFFREFORMATION.MODIFIER', N'Modifier une offre de formation', NULL,
                    (SELECT [Id] FROM [Ressources] WHERE [Code] = N'OFFREFORMATION'),
                    (SELECT [Id] FROM [TypesAction] WHERE [Code] = N'MODIFIER'), NULL);

                IF NOT EXISTS (SELECT 1 FROM [Droits] WHERE [Code] = N'OFFREFORMATION.SUPPRIMER')
                INSERT INTO [Droits] ([Code], [Libelle], [Description], [RessourceId], [TypeActionId], [GroupeDroitId])
                VALUES (N'OFFREFORMATION.SUPPRIMER', N'Supprimer une offre de formation', NULL,
                    (SELECT [Id] FROM [Ressources] WHERE [Code] = N'OFFREFORMATION'),
                    (SELECT [Id] FROM [TypesAction] WHERE [Code] = N'SUPPRIMER'), NULL);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DELETE FROM [Droits]
                WHERE [Code] IN (N'OFFREFORMATION.CONSULTER', N'OFFREFORMATION.CREER', N'OFFREFORMATION.MODIFIER', N'OFFREFORMATION.SUPPRIMER');
                """);

            migrationBuilder.Sql("""
                DELETE FROM [Ressources]
                WHERE [Code] = N'OFFREFORMATION';
                """);
        }
    }
}

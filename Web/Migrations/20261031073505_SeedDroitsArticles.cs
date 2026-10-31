using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Web.Migrations
{
    /// <inheritdoc />
    public partial class SeedDroitsArticles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Meme principe que SeedDroitsCartesCompetences : insertion idempotente par Code
            // (pas de HasData), car Ressources/Droits recoivent aussi des ecritures live
            // depuis /Administration/*.
            migrationBuilder.Sql("""
                IF NOT EXISTS (SELECT 1 FROM [Ressources] WHERE [Code] = N'ARTICLE')
                INSERT INTO [Ressources] ([Code], [Libelle], [Description])
                VALUES (N'ARTICLE', N'Article de blog', NULL);
                """);

            migrationBuilder.Sql("""
                IF NOT EXISTS (SELECT 1 FROM [Droits] WHERE [Code] = N'ARTICLE.CONSULTER')
                INSERT INTO [Droits] ([Code], [Libelle], [Description], [RessourceId], [TypeActionId], [GroupeDroitId])
                VALUES (N'ARTICLE.CONSULTER', N'Consulter les articles de blog (back-office)', NULL,
                    (SELECT [Id] FROM [Ressources] WHERE [Code] = N'ARTICLE'),
                    (SELECT [Id] FROM [TypesAction] WHERE [Code] = N'CONSULTER'), NULL);

                IF NOT EXISTS (SELECT 1 FROM [Droits] WHERE [Code] = N'ARTICLE.CREER')
                INSERT INTO [Droits] ([Code], [Libelle], [Description], [RessourceId], [TypeActionId], [GroupeDroitId])
                VALUES (N'ARTICLE.CREER', N'Créer un article de blog', NULL,
                    (SELECT [Id] FROM [Ressources] WHERE [Code] = N'ARTICLE'),
                    (SELECT [Id] FROM [TypesAction] WHERE [Code] = N'CREER'), NULL);

                IF NOT EXISTS (SELECT 1 FROM [Droits] WHERE [Code] = N'ARTICLE.MODIFIER')
                INSERT INTO [Droits] ([Code], [Libelle], [Description], [RessourceId], [TypeActionId], [GroupeDroitId])
                VALUES (N'ARTICLE.MODIFIER', N'Modifier un article de blog (inclut changer son statut)', NULL,
                    (SELECT [Id] FROM [Ressources] WHERE [Code] = N'ARTICLE'),
                    (SELECT [Id] FROM [TypesAction] WHERE [Code] = N'MODIFIER'), NULL);

                IF NOT EXISTS (SELECT 1 FROM [Droits] WHERE [Code] = N'ARTICLE.SUPPRIMER')
                INSERT INTO [Droits] ([Code], [Libelle], [Description], [RessourceId], [TypeActionId], [GroupeDroitId])
                VALUES (N'ARTICLE.SUPPRIMER', N'Supprimer un article de blog', NULL,
                    (SELECT [Id] FROM [Ressources] WHERE [Code] = N'ARTICLE'),
                    (SELECT [Id] FROM [TypesAction] WHERE [Code] = N'SUPPRIMER'), NULL);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DELETE FROM [Droits]
                WHERE [Code] IN (N'ARTICLE.CONSULTER', N'ARTICLE.CREER', N'ARTICLE.MODIFIER', N'ARTICLE.SUPPRIMER');
                """);

            migrationBuilder.Sql("""
                DELETE FROM [Ressources]
                WHERE [Code] IN (N'ARTICLE');
                """);
        }
    }
}

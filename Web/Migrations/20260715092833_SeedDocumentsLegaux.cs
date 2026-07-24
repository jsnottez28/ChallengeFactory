using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Web.Migrations
{
    /// <inheritdoc />
    public partial class SeedDocumentsLegaux : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Meme principe que les migrations Seed* precedentes : Ressources et Droits
            // recoivent des ecritures live depuis les ecrans /Administration/*, donc
            // insertion idempotente par Code (pas de HasData a Id fixe).
            migrationBuilder.Sql("""
                IF NOT EXISTS (SELECT 1 FROM [Ressources] WHERE [Code] = N'DOCUMENTLEGAL')
                INSERT INTO [Ressources] ([Code], [Libelle], [Description])
                VALUES (N'DOCUMENTLEGAL', N'Document légal', N'CGU et politique de protection des données.');
                """);

            migrationBuilder.Sql("""
                IF NOT EXISTS (SELECT 1 FROM [Droits] WHERE [Code] = N'DOCUMENTLEGAL.CONSULTER')
                INSERT INTO [Droits] ([Code], [Libelle], [Description], [RessourceId], [TypeActionId], [GroupeDroitId])
                VALUES (N'DOCUMENTLEGAL.CONSULTER', N'Consulter les documents légaux', NULL,
                    (SELECT [Id] FROM [Ressources] WHERE [Code] = N'DOCUMENTLEGAL'),
                    (SELECT [Id] FROM [TypesAction] WHERE [Code] = N'CONSULTER'), NULL);

                IF NOT EXISTS (SELECT 1 FROM [Droits] WHERE [Code] = N'DOCUMENTLEGAL.CREER')
                INSERT INTO [Droits] ([Code], [Libelle], [Description], [RessourceId], [TypeActionId], [GroupeDroitId])
                VALUES (N'DOCUMENTLEGAL.CREER', N'Publier une nouvelle version d''un document légal', NULL,
                    (SELECT [Id] FROM [Ressources] WHERE [Code] = N'DOCUMENTLEGAL'),
                    (SELECT [Id] FROM [TypesAction] WHERE [Code] = N'CREER'), NULL);
                """);

            // Version placeholder pour que l'inscription et les pages publiques /cgu et
            // /confidentialite ne soient jamais vides : a remplacer via
            // /Administration/DocumentsLegaux avant mise en production.
            migrationBuilder.Sql("""
                IF NOT EXISTS (SELECT 1 FROM [DocumentsLegaux] WHERE [Type] = 0 AND [Version] = N'0.1-brouillon')
                INSERT INTO [DocumentsLegaux] ([Type], [Version], [Contenu], [DateEffective], [EstPublie], [CreeLe])
                VALUES (0, N'0.1-brouillon',
                    N'# Conditions générales d''utilisation' + CHAR(13) + CHAR(10) + CHAR(13) + CHAR(10) +
                    N'*Contenu à rédiger par un administrateur depuis /Administration/DocumentsLegaux avant mise en production.*',
                    GETUTCDATE(), 1, GETUTCDATE());

                IF NOT EXISTS (SELECT 1 FROM [DocumentsLegaux] WHERE [Type] = 1 AND [Version] = N'0.1-brouillon')
                INSERT INTO [DocumentsLegaux] ([Type], [Version], [Contenu], [DateEffective], [EstPublie], [CreeLe])
                VALUES (1, N'0.1-brouillon',
                    N'# Politique de protection des données' + CHAR(13) + CHAR(10) + CHAR(13) + CHAR(10) +
                    N'*Contenu à rédiger par un administrateur depuis /Administration/DocumentsLegaux avant mise en production.*',
                    GETUTCDATE(), 1, GETUTCDATE());
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DELETE FROM [DocumentsLegaux] WHERE [Version] = N'0.1-brouillon';
                """);

            migrationBuilder.Sql("""
                DELETE FROM [Droits]
                WHERE [Code] IN (N'DOCUMENTLEGAL.CONSULTER', N'DOCUMENTLEGAL.CREER');
                """);

            migrationBuilder.Sql("""
                DELETE FROM [Ressources] WHERE [Code] = N'DOCUMENTLEGAL';
                """);
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Web.Migrations
{
    /// <inheritdoc />
    public partial class SeedDroitsTestsPsychotechniques : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Meme principe que SeedDroitsReclamations : insertion idempotente par Code (pas
            // de HasData), car Ressources/Droits recoivent aussi des ecritures live depuis
            // /Administration/*. Seul CONSULTER est pertinent ici (catalogue statique de
            // tests psychotechniques - aujourd'hui DISC uniquement - et resultats deja lies
            // au stagiaire qui les a passes ; rien a creer/modifier/supprimer depuis
            // l'admin).
            migrationBuilder.Sql("""
                IF NOT EXISTS (SELECT 1 FROM [Ressources] WHERE [Code] = N'TEST')
                INSERT INTO [Ressources] ([Code], [Libelle], [Description])
                VALUES (N'TEST', N'Test psychotechnique', NULL);
                """);

            migrationBuilder.Sql("""
                IF NOT EXISTS (SELECT 1 FROM [Droits] WHERE [Code] = N'TEST.CONSULTER')
                INSERT INTO [Droits] ([Code], [Libelle], [Description], [RessourceId], [TypeActionId], [GroupeDroitId])
                VALUES (N'TEST.CONSULTER', N'Consulter les tests psychotechniques et les résultats des stagiaires', NULL,
                    (SELECT [Id] FROM [Ressources] WHERE [Code] = N'TEST'),
                    (SELECT [Id] FROM [TypesAction] WHERE [Code] = N'CONSULTER'), NULL);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DELETE FROM [Droits]
                WHERE [Code] = N'TEST.CONSULTER';
                """);

            migrationBuilder.Sql("""
                DELETE FROM [Ressources]
                WHERE [Code] = N'TEST';
                """);
        }
    }
}

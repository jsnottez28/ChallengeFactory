using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Web.Migrations
{
    /// <inheritdoc />
    public partial class SeedDroitsReclamations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Meme principe que SeedDroitsChallengesEtCohortes : insertion idempotente par
            // Code (pas de HasData), car Ressources/Droits recoivent aussi des ecritures
            // live depuis /Administration/*. Seuls CONSULTER et VALIDER sont pertinents ici
            // (les reclamations sont deposees par le public, jamais creees/modifiees/
            // supprimees par un Gestionnaire) - VALIDER couvre "prendre en charge" et
            // "repondre", memes semantique que COHORTE.VALIDER pour l'avancement d'un
            // workflow.
            migrationBuilder.Sql("""
                IF NOT EXISTS (SELECT 1 FROM [Ressources] WHERE [Code] = N'RECLAMATION')
                INSERT INTO [Ressources] ([Code], [Libelle], [Description])
                VALUES (N'RECLAMATION', N'Réclamation', NULL);
                """);

            migrationBuilder.Sql("""
                IF NOT EXISTS (SELECT 1 FROM [Droits] WHERE [Code] = N'RECLAMATION.CONSULTER')
                INSERT INTO [Droits] ([Code], [Libelle], [Description], [RessourceId], [TypeActionId], [GroupeDroitId])
                VALUES (N'RECLAMATION.CONSULTER', N'Consulter les réclamations', NULL,
                    (SELECT [Id] FROM [Ressources] WHERE [Code] = N'RECLAMATION'),
                    (SELECT [Id] FROM [TypesAction] WHERE [Code] = N'CONSULTER'), NULL);

                IF NOT EXISTS (SELECT 1 FROM [Droits] WHERE [Code] = N'RECLAMATION.VALIDER')
                INSERT INTO [Droits] ([Code], [Libelle], [Description], [RessourceId], [TypeActionId], [GroupeDroitId])
                VALUES (N'RECLAMATION.VALIDER', N'Prendre en charge et répondre à une réclamation', NULL,
                    (SELECT [Id] FROM [Ressources] WHERE [Code] = N'RECLAMATION'),
                    (SELECT [Id] FROM [TypesAction] WHERE [Code] = N'VALIDER'), NULL);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DELETE FROM [Droits]
                WHERE [Code] IN (N'RECLAMATION.CONSULTER', N'RECLAMATION.VALIDER');
                """);

            migrationBuilder.Sql("""
                DELETE FROM [Ressources]
                WHERE [Code] = N'RECLAMATION';
                """);
        }
    }
}

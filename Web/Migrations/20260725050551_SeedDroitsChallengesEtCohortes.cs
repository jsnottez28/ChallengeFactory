using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Web.Migrations
{
    /// <inheritdoc />
    public partial class SeedDroitsChallengesEtCohortes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Meme principe que SeedDroitsCartesCompetences : insertion idempotente par
            // Code (pas de HasData), car Ressources/Droits recoivent aussi des ecritures
            // live depuis /Administration/*.
            migrationBuilder.Sql("""
                IF NOT EXISTS (SELECT 1 FROM [Ressources] WHERE [Code] = N'CHALLENGE')
                INSERT INTO [Ressources] ([Code], [Libelle], [Description])
                VALUES (N'CHALLENGE', N'Challenge', NULL);

                IF NOT EXISTS (SELECT 1 FROM [Ressources] WHERE [Code] = N'COHORTE')
                INSERT INTO [Ressources] ([Code], [Libelle], [Description])
                VALUES (N'COHORTE', N'Cohorte', NULL);
                """);

            migrationBuilder.Sql("""
                IF NOT EXISTS (SELECT 1 FROM [Droits] WHERE [Code] = N'CHALLENGE.CONSULTER')
                INSERT INTO [Droits] ([Code], [Libelle], [Description], [RessourceId], [TypeActionId], [GroupeDroitId])
                VALUES (N'CHALLENGE.CONSULTER', N'Consulter les Challenges', NULL,
                    (SELECT [Id] FROM [Ressources] WHERE [Code] = N'CHALLENGE'),
                    (SELECT [Id] FROM [TypesAction] WHERE [Code] = N'CONSULTER'), NULL);

                IF NOT EXISTS (SELECT 1 FROM [Droits] WHERE [Code] = N'CHALLENGE.CREER')
                INSERT INTO [Droits] ([Code], [Libelle], [Description], [RessourceId], [TypeActionId], [GroupeDroitId])
                VALUES (N'CHALLENGE.CREER', N'Créer un Challenge', NULL,
                    (SELECT [Id] FROM [Ressources] WHERE [Code] = N'CHALLENGE'),
                    (SELECT [Id] FROM [TypesAction] WHERE [Code] = N'CREER'), NULL);

                IF NOT EXISTS (SELECT 1 FROM [Droits] WHERE [Code] = N'CHALLENGE.MODIFIER')
                INSERT INTO [Droits] ([Code], [Libelle], [Description], [RessourceId], [TypeActionId], [GroupeDroitId])
                VALUES (N'CHALLENGE.MODIFIER', N'Modifier un Challenge (architecture, étapes, cartes rattachées)', NULL,
                    (SELECT [Id] FROM [Ressources] WHERE [Code] = N'CHALLENGE'),
                    (SELECT [Id] FROM [TypesAction] WHERE [Code] = N'MODIFIER'), NULL);

                IF NOT EXISTS (SELECT 1 FROM [Droits] WHERE [Code] = N'CHALLENGE.SUPPRIMER')
                INSERT INTO [Droits] ([Code], [Libelle], [Description], [RessourceId], [TypeActionId], [GroupeDroitId])
                VALUES (N'CHALLENGE.SUPPRIMER', N'Supprimer un Challenge', NULL,
                    (SELECT [Id] FROM [Ressources] WHERE [Code] = N'CHALLENGE'),
                    (SELECT [Id] FROM [TypesAction] WHERE [Code] = N'SUPPRIMER'), NULL);

                IF NOT EXISTS (SELECT 1 FROM [Droits] WHERE [Code] = N'CHALLENGE.VALIDER')
                INSERT INTO [Droits] ([Code], [Libelle], [Description], [RessourceId], [TypeActionId], [GroupeDroitId])
                VALUES (N'CHALLENGE.VALIDER', N'Publier un Challenge (Brouillon → Publié)', NULL,
                    (SELECT [Id] FROM [Ressources] WHERE [Code] = N'CHALLENGE'),
                    (SELECT [Id] FROM [TypesAction] WHERE [Code] = N'VALIDER'), NULL);
                """);

            migrationBuilder.Sql("""
                IF NOT EXISTS (SELECT 1 FROM [Droits] WHERE [Code] = N'COHORTE.CONSULTER')
                INSERT INTO [Droits] ([Code], [Libelle], [Description], [RessourceId], [TypeActionId], [GroupeDroitId])
                VALUES (N'COHORTE.CONSULTER', N'Consulter les Cohortes', NULL,
                    (SELECT [Id] FROM [Ressources] WHERE [Code] = N'COHORTE'),
                    (SELECT [Id] FROM [TypesAction] WHERE [Code] = N'CONSULTER'), NULL);

                IF NOT EXISTS (SELECT 1 FROM [Droits] WHERE [Code] = N'COHORTE.CREER')
                INSERT INTO [Droits] ([Code], [Libelle], [Description], [RessourceId], [TypeActionId], [GroupeDroitId])
                VALUES (N'COHORTE.CREER', N'Créer une Cohorte', NULL,
                    (SELECT [Id] FROM [Ressources] WHERE [Code] = N'COHORTE'),
                    (SELECT [Id] FROM [TypesAction] WHERE [Code] = N'CREER'), NULL);

                IF NOT EXISTS (SELECT 1 FROM [Droits] WHERE [Code] = N'COHORTE.MODIFIER')
                INSERT INTO [Droits] ([Code], [Libelle], [Description], [RessourceId], [TypeActionId], [GroupeDroitId])
                VALUES (N'COHORTE.MODIFIER', N'Modifier une Cohorte (membres, abonnement BtoC)', NULL,
                    (SELECT [Id] FROM [Ressources] WHERE [Code] = N'COHORTE'),
                    (SELECT [Id] FROM [TypesAction] WHERE [Code] = N'MODIFIER'), NULL);

                IF NOT EXISTS (SELECT 1 FROM [Droits] WHERE [Code] = N'COHORTE.SUPPRIMER')
                INSERT INTO [Droits] ([Code], [Libelle], [Description], [RessourceId], [TypeActionId], [GroupeDroitId])
                VALUES (N'COHORTE.SUPPRIMER', N'Supprimer une Cohorte', NULL,
                    (SELECT [Id] FROM [Ressources] WHERE [Code] = N'COHORTE'),
                    (SELECT [Id] FROM [TypesAction] WHERE [Code] = N'SUPPRIMER'), NULL);

                IF NOT EXISTS (SELECT 1 FROM [Droits] WHERE [Code] = N'COHORTE.VALIDER')
                INSERT INTO [Droits] ([Code], [Libelle], [Description], [RessourceId], [TypeActionId], [GroupeDroitId])
                VALUES (N'COHORTE.VALIDER', N'Lancer une Cohorte et valider ses étapes', NULL,
                    (SELECT [Id] FROM [Ressources] WHERE [Code] = N'COHORTE'),
                    (SELECT [Id] FROM [TypesAction] WHERE [Code] = N'VALIDER'), NULL);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DELETE FROM [Droits]
                WHERE [Code] IN (
                    N'CHALLENGE.CONSULTER', N'CHALLENGE.CREER', N'CHALLENGE.MODIFIER', N'CHALLENGE.SUPPRIMER', N'CHALLENGE.VALIDER',
                    N'COHORTE.CONSULTER', N'COHORTE.CREER', N'COHORTE.MODIFIER', N'COHORTE.SUPPRIMER', N'COHORTE.VALIDER');
                """);

            migrationBuilder.Sql("""
                DELETE FROM [Ressources]
                WHERE [Code] IN (N'CHALLENGE', N'COHORTE');
                """);
        }
    }
}

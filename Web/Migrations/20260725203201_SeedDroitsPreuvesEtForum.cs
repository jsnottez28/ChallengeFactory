using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Web.Migrations
{
    /// <inheritdoc />
    public partial class SeedDroitsPreuvesEtForum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Meme principe que SeedDroitsChallengesEtCohortes : insertion idempotente par
            // Code (pas de HasData), car Ressources/Droits recoivent aussi des ecritures
            // live depuis /Administration/*.
            migrationBuilder.Sql("""
                IF NOT EXISTS (SELECT 1 FROM [Ressources] WHERE [Code] = N'PREUVE')
                INSERT INTO [Ressources] ([Code], [Libelle], [Description])
                VALUES (N'PREUVE', N'Preuve', NULL);

                IF NOT EXISTS (SELECT 1 FROM [Ressources] WHERE [Code] = N'FORUM')
                INSERT INTO [Ressources] ([Code], [Libelle], [Description])
                VALUES (N'FORUM', N'Forum', NULL);
                """);

            migrationBuilder.Sql("""
                IF NOT EXISTS (SELECT 1 FROM [Droits] WHERE [Code] = N'PREUVE.CONSULTER')
                INSERT INTO [Droits] ([Code], [Libelle], [Description], [RessourceId], [TypeActionId], [GroupeDroitId])
                VALUES (N'PREUVE.CONSULTER', N'Consulter les Preuves et les contributions d''une Cohorte', NULL,
                    (SELECT [Id] FROM [Ressources] WHERE [Code] = N'PREUVE'),
                    (SELECT [Id] FROM [TypesAction] WHERE [Code] = N'CONSULTER'), NULL);

                IF NOT EXISTS (SELECT 1 FROM [Droits] WHERE [Code] = N'PREUVE.CREER')
                INSERT INTO [Droits] ([Code], [Libelle], [Description], [RessourceId], [TypeActionId], [GroupeDroitId])
                VALUES (N'PREUVE.CREER', N'Créer une Preuve', NULL,
                    (SELECT [Id] FROM [Ressources] WHERE [Code] = N'PREUVE'),
                    (SELECT [Id] FROM [TypesAction] WHERE [Code] = N'CREER'), NULL);

                IF NOT EXISTS (SELECT 1 FROM [Droits] WHERE [Code] = N'PREUVE.MODIFIER')
                INSERT INTO [Droits] ([Code], [Libelle], [Description], [RessourceId], [TypeActionId], [GroupeDroitId])
                VALUES (N'PREUVE.MODIFIER', N'Modifier une Preuve', NULL,
                    (SELECT [Id] FROM [Ressources] WHERE [Code] = N'PREUVE'),
                    (SELECT [Id] FROM [TypesAction] WHERE [Code] = N'MODIFIER'), NULL);

                IF NOT EXISTS (SELECT 1 FROM [Droits] WHERE [Code] = N'PREUVE.SUPPRIMER')
                INSERT INTO [Droits] ([Code], [Libelle], [Description], [RessourceId], [TypeActionId], [GroupeDroitId])
                VALUES (N'PREUVE.SUPPRIMER', N'Supprimer une Preuve', NULL,
                    (SELECT [Id] FROM [Ressources] WHERE [Code] = N'PREUVE'),
                    (SELECT [Id] FROM [TypesAction] WHERE [Code] = N'SUPPRIMER'), NULL);

                IF NOT EXISTS (SELECT 1 FROM [Droits] WHERE [Code] = N'PREUVE.VALIDER')
                INSERT INTO [Droits] ([Code], [Libelle], [Description], [RessourceId], [TypeActionId], [GroupeDroitId])
                VALUES (N'PREUVE.VALIDER', N'Valider ou refuser directement une Preuve (hors clôture d''étape)', NULL,
                    (SELECT [Id] FROM [Ressources] WHERE [Code] = N'PREUVE'),
                    (SELECT [Id] FROM [TypesAction] WHERE [Code] = N'VALIDER'), NULL);
                """);

            migrationBuilder.Sql("""
                IF NOT EXISTS (SELECT 1 FROM [Droits] WHERE [Code] = N'FORUM.CONSULTER')
                INSERT INTO [Droits] ([Code], [Libelle], [Description], [RessourceId], [TypeActionId], [GroupeDroitId])
                VALUES (N'FORUM.CONSULTER', N'Consulter le Forum d''une Cohorte', NULL,
                    (SELECT [Id] FROM [Ressources] WHERE [Code] = N'FORUM'),
                    (SELECT [Id] FROM [TypesAction] WHERE [Code] = N'CONSULTER'), NULL);

                IF NOT EXISTS (SELECT 1 FROM [Droits] WHERE [Code] = N'FORUM.CREER')
                INSERT INTO [Droits] ([Code], [Libelle], [Description], [RessourceId], [TypeActionId], [GroupeDroitId])
                VALUES (N'FORUM.CREER', N'Poster un message de Forum', NULL,
                    (SELECT [Id] FROM [Ressources] WHERE [Code] = N'FORUM'),
                    (SELECT [Id] FROM [TypesAction] WHERE [Code] = N'CREER'), NULL);

                IF NOT EXISTS (SELECT 1 FROM [Droits] WHERE [Code] = N'FORUM.MODIFIER')
                INSERT INTO [Droits] ([Code], [Libelle], [Description], [RessourceId], [TypeActionId], [GroupeDroitId])
                VALUES (N'FORUM.MODIFIER', N'Modifier un message de Forum', NULL,
                    (SELECT [Id] FROM [Ressources] WHERE [Code] = N'FORUM'),
                    (SELECT [Id] FROM [TypesAction] WHERE [Code] = N'MODIFIER'), NULL);

                IF NOT EXISTS (SELECT 1 FROM [Droits] WHERE [Code] = N'FORUM.SUPPRIMER')
                INSERT INTO [Droits] ([Code], [Libelle], [Description], [RessourceId], [TypeActionId], [GroupeDroitId])
                VALUES (N'FORUM.SUPPRIMER', N'Modérer (supprimer) un message de Forum', NULL,
                    (SELECT [Id] FROM [Ressources] WHERE [Code] = N'FORUM'),
                    (SELECT [Id] FROM [TypesAction] WHERE [Code] = N'SUPPRIMER'), NULL);

                IF NOT EXISTS (SELECT 1 FROM [Droits] WHERE [Code] = N'FORUM.VALIDER')
                INSERT INTO [Droits] ([Code], [Libelle], [Description], [RessourceId], [TypeActionId], [GroupeDroitId])
                VALUES (N'FORUM.VALIDER', N'Valider un message de Forum', NULL,
                    (SELECT [Id] FROM [Ressources] WHERE [Code] = N'FORUM'),
                    (SELECT [Id] FROM [TypesAction] WHERE [Code] = N'VALIDER'), NULL);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DELETE FROM [Droits]
                WHERE [Code] IN (
                    N'PREUVE.CONSULTER', N'PREUVE.CREER', N'PREUVE.MODIFIER', N'PREUVE.SUPPRIMER', N'PREUVE.VALIDER',
                    N'FORUM.CONSULTER', N'FORUM.CREER', N'FORUM.MODIFIER', N'FORUM.SUPPRIMER', N'FORUM.VALIDER');
                """);

            migrationBuilder.Sql("""
                DELETE FROM [Ressources]
                WHERE [Code] IN (N'PREUVE', N'FORUM');
                """);
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Web.Migrations
{
    /// <inheritdoc />
    public partial class SeedDroitsOrganisation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Insertion idempotente par Code (et non par Id fixe) : la table Droits recoit
            // aussi des ecritures live depuis l'ecran /Administration/Droits, un Id force
            // via HasData/InsertData entrerait en collision avec l'auto-incrementation
            // IDENTITY des qu'un droit a deja ete cree manuellement avant ce seed.
            migrationBuilder.Sql("""
                IF NOT EXISTS (SELECT 1 FROM [Droits] WHERE [Code] = N'ORGANISATION.CONSULTER')
                INSERT INTO [Droits] ([Code], [Libelle], [Description], [RessourceId], [TypeActionId], [GroupeDroitId])
                VALUES (N'ORGANISATION.CONSULTER', N'Consulter les organisations', NULL, 4, 1, NULL);

                IF NOT EXISTS (SELECT 1 FROM [Droits] WHERE [Code] = N'ORGANISATION.CREER')
                INSERT INTO [Droits] ([Code], [Libelle], [Description], [RessourceId], [TypeActionId], [GroupeDroitId])
                VALUES (N'ORGANISATION.CREER', N'Creer une organisation', NULL, 4, 2, NULL);

                IF NOT EXISTS (SELECT 1 FROM [Droits] WHERE [Code] = N'ORGANISATION.MODIFIER')
                INSERT INTO [Droits] ([Code], [Libelle], [Description], [RessourceId], [TypeActionId], [GroupeDroitId])
                VALUES (N'ORGANISATION.MODIFIER', N'Modifier une organisation', NULL, 4, 3, NULL);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DELETE FROM [Droits]
                WHERE [Code] IN (N'ORGANISATION.CONSULTER', N'ORGANISATION.CREER', N'ORGANISATION.MODIFIER');
                """);
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Web.Migrations
{
    /// <inheritdoc />
    public partial class AddFonctionAndOrganisationStatut : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EstPrincipal",
                table: "Rattachements");

            migrationBuilder.DropColumn(
                name: "Fonction",
                table: "Rattachements");

            migrationBuilder.AddColumn<int>(
                name: "FonctionId",
                table: "Rattachements",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Fonctions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Libelle = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Ordre = table.Column<int>(type: "int", nullable: false),
                    EstActif = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Fonctions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Rattachements_FonctionId",
                table: "Rattachements",
                column: "FonctionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Rattachements_Fonctions_FonctionId",
                table: "Rattachements",
                column: "FonctionId",
                principalTable: "Fonctions",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Rattachements_Fonctions_FonctionId",
                table: "Rattachements");

            migrationBuilder.DropTable(
                name: "Fonctions");

            migrationBuilder.DropIndex(
                name: "IX_Rattachements_FonctionId",
                table: "Rattachements");

            migrationBuilder.DropColumn(
                name: "FonctionId",
                table: "Rattachements");

            migrationBuilder.AddColumn<bool>(
                name: "EstPrincipal",
                table: "Rattachements",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Fonction",
                table: "Rattachements",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}

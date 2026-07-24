using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Web.Migrations
{
    /// <inheritdoc />
    public partial class AddCiviliteTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CodeAdherent",
                table: "AspNetUsers");

            migrationBuilder.RenameColumn(
                name: "Structure",
                table: "AspNetUsers",
                newName: "Telephone");

            migrationBuilder.AddColumn<int>(
                name: "CiviliteId",
                table: "AspNetUsers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Statut",
                table: "AspNetUsers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Civilites",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LibelleCourt = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LibelleLong = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LibelleArticle = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Ordre = table.Column<int>(type: "int", nullable: false),
                    EstActif = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Civilites", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Civilites",
                columns: new[] { "Id", "Code", "EstActif", "LibelleArticle", "LibelleCourt", "LibelleLong", "Ordre" },
                values: new object[,]
                {
                    { 1, "M", true, "à Monsieur", "M.", "Monsieur", 1 },
                    { 2, "MME", true, "à Madame", "Mme", "Madame", 2 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Civilites");

            migrationBuilder.DropColumn(
                name: "CiviliteId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Statut",
                table: "AspNetUsers");

            migrationBuilder.RenameColumn(
                name: "Telephone",
                table: "AspNetUsers",
                newName: "Structure");

            migrationBuilder.AddColumn<string>(
                name: "CodeAdherent",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Web.Migrations
{
    /// <inheritdoc />
    public partial class AddChampsVitrineChallenge : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "DisponibleEnPresentiel",
                table: "Challenges",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Duree",
                table: "Challenges",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Financement",
                table: "Challenges",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Prix",
                table: "Challenges",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PublicVise",
                table: "Challenges",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SousFamille",
                table: "Challenges",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DisponibleEnPresentiel",
                table: "Challenges");

            migrationBuilder.DropColumn(
                name: "Duree",
                table: "Challenges");

            migrationBuilder.DropColumn(
                name: "Financement",
                table: "Challenges");

            migrationBuilder.DropColumn(
                name: "Prix",
                table: "Challenges");

            migrationBuilder.DropColumn(
                name: "PublicVise",
                table: "Challenges");

            migrationBuilder.DropColumn(
                name: "SousFamille",
                table: "Challenges");
        }
    }
}

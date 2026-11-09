using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Web.Migrations
{
    /// <inheritdoc />
    public partial class AddValeursSaisiesDossierCarbone : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LignesSupplementairesJson",
                table: "DossiersCarbone",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ValeursSaisiesJson",
                table: "DossiersCarbone",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LignesSupplementairesJson",
                table: "DossiersCarbone");

            migrationBuilder.DropColumn(
                name: "ValeursSaisiesJson",
                table: "DossiersCarbone");
        }
    }
}

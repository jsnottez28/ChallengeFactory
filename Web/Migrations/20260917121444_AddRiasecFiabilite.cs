using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Web.Migrations
{
    /// <inheritdoc />
    public partial class AddRiasecFiabilite : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "NombrePairesCoherentes",
                table: "RiasecResultats",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "NombrePairesControle",
                table: "RiasecResultats",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NombrePairesCoherentes",
                table: "RiasecResultats");

            migrationBuilder.DropColumn(
                name: "NombrePairesControle",
                table: "RiasecResultats");
        }
    }
}

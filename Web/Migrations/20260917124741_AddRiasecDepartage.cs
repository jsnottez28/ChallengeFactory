using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Web.Migrations
{
    /// <inheritdoc />
    public partial class AddRiasecDepartage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DepartageDimensionA",
                table: "RiasecResultats",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DepartageDimensionB",
                table: "RiasecResultats",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DepartageGagnant",
                table: "RiasecResultats",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DepartageDimensionA",
                table: "RiasecResultats");

            migrationBuilder.DropColumn(
                name: "DepartageDimensionB",
                table: "RiasecResultats");

            migrationBuilder.DropColumn(
                name: "DepartageGagnant",
                table: "RiasecResultats");
        }
    }
}

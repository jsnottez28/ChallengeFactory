using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Web.Migrations
{
    /// <inheritdoc />
    public partial class AddDimensionsSatisfaction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "NoteAccompagnement",
                table: "SatisfactionReponses",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "NoteAdequationAttentes",
                table: "SatisfactionReponses",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "NoteContenus",
                table: "SatisfactionReponses",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NoteAccompagnement",
                table: "SatisfactionReponses");

            migrationBuilder.DropColumn(
                name: "NoteAdequationAttentes",
                table: "SatisfactionReponses");

            migrationBuilder.DropColumn(
                name: "NoteContenus",
                table: "SatisfactionReponses");
        }
    }
}

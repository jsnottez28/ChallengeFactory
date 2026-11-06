using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Web.Migrations
{
    /// <inheritdoc />
    public partial class AddTableurDeposeDossierCarbone : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "CollecteDeclareeTerminee",
                table: "DossiersCarbone",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TableurDeposeAvancementPourcent",
                table: "DossiersCarbone",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TableurDeposeCheminStockage",
                table: "DossiersCarbone",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TableurDeposeLe",
                table: "DossiersCarbone",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TableurDeposeNomFichier",
                table: "DossiersCarbone",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CollecteDeclareeTerminee",
                table: "DossiersCarbone");

            migrationBuilder.DropColumn(
                name: "TableurDeposeAvancementPourcent",
                table: "DossiersCarbone");

            migrationBuilder.DropColumn(
                name: "TableurDeposeCheminStockage",
                table: "DossiersCarbone");

            migrationBuilder.DropColumn(
                name: "TableurDeposeLe",
                table: "DossiersCarbone");

            migrationBuilder.DropColumn(
                name: "TableurDeposeNomFichier",
                table: "DossiersCarbone");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Web.Migrations
{
    /// <inheritdoc />
    public partial class AddRiasecResultat : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RiasecResultats",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UtilisateurId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ScoreR = table.Column<int>(type: "int", nullable: false),
                    ScoreI = table.Column<int>(type: "int", nullable: false),
                    ScoreA = table.Column<int>(type: "int", nullable: false),
                    ScoreS = table.Column<int>(type: "int", nullable: false),
                    ScoreE = table.Column<int>(type: "int", nullable: false),
                    ScoreC = table.Column<int>(type: "int", nullable: false),
                    CodeHolland = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CompleteLe = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RiasecResultats", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RiasecResultats_AspNetUsers_UtilisateurId",
                        column: x => x.UtilisateurId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RiasecResultats_UtilisateurId",
                table: "RiasecResultats",
                column: "UtilisateurId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RiasecResultats");
        }
    }
}

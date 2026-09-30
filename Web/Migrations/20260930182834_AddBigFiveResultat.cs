using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Web.Migrations
{
    /// <inheritdoc />
    public partial class AddBigFiveResultat : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BigFiveResultats",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UtilisateurId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ScoreNevrosisme = table.Column<int>(type: "int", nullable: false),
                    ScoreExtraversion = table.Column<int>(type: "int", nullable: false),
                    ScoreOuverture = table.Column<int>(type: "int", nullable: false),
                    ScoreAgreabilite = table.Column<int>(type: "int", nullable: false),
                    ScoreConsciencieusite = table.Column<int>(type: "int", nullable: false),
                    CompleteLe = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BigFiveResultats", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BigFiveResultats_AspNetUsers_UtilisateurId",
                        column: x => x.UtilisateurId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BigFiveResultatFacettes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BigFiveResultatId = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Score = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BigFiveResultatFacettes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BigFiveResultatFacettes_BigFiveResultats_BigFiveResultatId",
                        column: x => x.BigFiveResultatId,
                        principalTable: "BigFiveResultats",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BigFiveResultatFacettes_BigFiveResultatId",
                table: "BigFiveResultatFacettes",
                column: "BigFiveResultatId");

            migrationBuilder.CreateIndex(
                name: "IX_BigFiveResultats_UtilisateurId",
                table: "BigFiveResultats",
                column: "UtilisateurId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BigFiveResultatFacettes");

            migrationBuilder.DropTable(
                name: "BigFiveResultats");
        }
    }
}

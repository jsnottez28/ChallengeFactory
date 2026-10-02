using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Web.Migrations
{
    /// <inheritdoc />
    public partial class AddScheinResultat : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ScheinResultats",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UtilisateurId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ScoreTechnique = table.Column<int>(type: "int", nullable: false),
                    ScoreManageriale = table.Column<int>(type: "int", nullable: false),
                    ScoreAutonomie = table.Column<int>(type: "int", nullable: false),
                    ScoreSecurite = table.Column<int>(type: "int", nullable: false),
                    ScoreCreativite = table.Column<int>(type: "int", nullable: false),
                    ScoreCause = table.Column<int>(type: "int", nullable: false),
                    ScoreDefiPur = table.Column<int>(type: "int", nullable: false),
                    ScoreQualiteDeVie = table.Column<int>(type: "int", nullable: false),
                    ScoreInternationale = table.Column<int>(type: "int", nullable: false),
                    CompleteLe = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheinResultats", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScheinResultats_AspNetUsers_UtilisateurId",
                        column: x => x.UtilisateurId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ScheinResultats_UtilisateurId",
                table: "ScheinResultats",
                column: "UtilisateurId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ScheinResultats");
        }
    }
}

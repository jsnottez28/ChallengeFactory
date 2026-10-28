using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Web.Migrations
{
    /// <inheritdoc />
    public partial class AddKilmannResultat : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "KilmannResultats",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UtilisateurId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ScoreRivaliser = table.Column<int>(type: "int", nullable: false),
                    ScoreCollaborer = table.Column<int>(type: "int", nullable: false),
                    ScoreCompromis = table.Column<int>(type: "int", nullable: false),
                    ScoreEviter = table.Column<int>(type: "int", nullable: false),
                    ScoreCeder = table.Column<int>(type: "int", nullable: false),
                    ProfilDominant = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CompleteLe = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KilmannResultats", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KilmannResultats_AspNetUsers_UtilisateurId",
                        column: x => x.UtilisateurId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_KilmannResultats_UtilisateurId",
                table: "KilmannResultats",
                column: "UtilisateurId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "KilmannResultats");
        }
    }
}

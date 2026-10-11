using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Web.Migrations
{
    /// <inheritdoc />
    public partial class AddCohorteMembreCarteSupplementaire : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CohorteMembreCartesSupplementaires",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CohorteMembreId = table.Column<int>(type: "int", nullable: false),
                    ChallengeEtapeId = table.Column<int>(type: "int", nullable: false),
                    CarteCompetenceId = table.Column<int>(type: "int", nullable: false),
                    AjouteeParId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    AjouteeLe = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CohorteMembreCartesSupplementaires", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CohorteMembreCartesSupplementaires_AspNetUsers_AjouteeParId",
                        column: x => x.AjouteeParId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CohorteMembreCartesSupplementaires_CartesCompetences_CarteCompetenceId",
                        column: x => x.CarteCompetenceId,
                        principalTable: "CartesCompetences",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CohorteMembreCartesSupplementaires_ChallengeEtapes_ChallengeEtapeId",
                        column: x => x.ChallengeEtapeId,
                        principalTable: "ChallengeEtapes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CohorteMembreCartesSupplementaires_CohorteMembres_CohorteMembreId",
                        column: x => x.CohorteMembreId,
                        principalTable: "CohorteMembres",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CohorteMembreCartesSupplementaires_AjouteeParId",
                table: "CohorteMembreCartesSupplementaires",
                column: "AjouteeParId");

            migrationBuilder.CreateIndex(
                name: "IX_CohorteMembreCartesSupplementaires_CarteCompetenceId",
                table: "CohorteMembreCartesSupplementaires",
                column: "CarteCompetenceId");

            migrationBuilder.CreateIndex(
                name: "IX_CohorteMembreCartesSupplementaires_ChallengeEtapeId",
                table: "CohorteMembreCartesSupplementaires",
                column: "ChallengeEtapeId");

            migrationBuilder.CreateIndex(
                name: "IX_CohorteMembreCartesSupplementaires_CohorteMembreId_ChallengeEtapeId_CarteCompetenceId",
                table: "CohorteMembreCartesSupplementaires",
                columns: new[] { "CohorteMembreId", "ChallengeEtapeId", "CarteCompetenceId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CohorteMembreCartesSupplementaires");
        }
    }
}

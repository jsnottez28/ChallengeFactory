using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Web.Migrations
{
    /// <inheritdoc />
    public partial class AddCohorteMembreEtapePersonnalisation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CohorteMembreEtapePersonnalisations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CohorteMembreId = table.Column<int>(type: "int", nullable: false),
                    ChallengeEtapeId = table.Column<int>(type: "int", nullable: false),
                    DefiIndividuelPersonnalise = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ModifieParId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ModifieLe = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CohorteMembreEtapePersonnalisations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CohorteMembreEtapePersonnalisations_AspNetUsers_ModifieParId",
                        column: x => x.ModifieParId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CohorteMembreEtapePersonnalisations_ChallengeEtapes_ChallengeEtapeId",
                        column: x => x.ChallengeEtapeId,
                        principalTable: "ChallengeEtapes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CohorteMembreEtapePersonnalisations_CohorteMembres_CohorteMembreId",
                        column: x => x.CohorteMembreId,
                        principalTable: "CohorteMembres",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CohorteMembreEtapePersonnalisations_ChallengeEtapeId",
                table: "CohorteMembreEtapePersonnalisations",
                column: "ChallengeEtapeId");

            migrationBuilder.CreateIndex(
                name: "IX_CohorteMembreEtapePersonnalisations_CohorteMembreId_ChallengeEtapeId",
                table: "CohorteMembreEtapePersonnalisations",
                columns: new[] { "CohorteMembreId", "ChallengeEtapeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CohorteMembreEtapePersonnalisations_ModifieParId",
                table: "CohorteMembreEtapePersonnalisations",
                column: "ModifieParId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CohorteMembreEtapePersonnalisations");
        }
    }
}

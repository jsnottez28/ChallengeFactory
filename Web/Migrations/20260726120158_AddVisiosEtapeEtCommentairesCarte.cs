using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Web.Migrations
{
    /// <inheritdoc />
    public partial class AddVisiosEtapeEtCommentairesCarte : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CommentairesCarte",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UtilisateurId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CarteCompetenceId = table.Column<int>(type: "int", nullable: false),
                    Contenu = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateCreation = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateModification = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommentairesCarte", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CommentairesCarte_AspNetUsers_UtilisateurId",
                        column: x => x.UtilisateurId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CommentairesCarte_CartesCompetences_CarteCompetenceId",
                        column: x => x.CarteCompetenceId,
                        principalTable: "CartesCompetences",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VisiosEtape",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CohorteId = table.Column<int>(type: "int", nullable: false),
                    ChallengeEtapeId = table.Column<int>(type: "int", nullable: false),
                    DateHeure = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LienConnexion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Descriptif = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PlanifieParId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    DatePlanification = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VisiosEtape", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VisiosEtape_AspNetUsers_PlanifieParId",
                        column: x => x.PlanifieParId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VisiosEtape_ChallengeEtapes_ChallengeEtapeId",
                        column: x => x.ChallengeEtapeId,
                        principalTable: "ChallengeEtapes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VisiosEtape_Cohortes_CohorteId",
                        column: x => x.CohorteId,
                        principalTable: "Cohortes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CommentairesCarte_CarteCompetenceId",
                table: "CommentairesCarte",
                column: "CarteCompetenceId");

            migrationBuilder.CreateIndex(
                name: "IX_CommentairesCarte_UtilisateurId_CarteCompetenceId",
                table: "CommentairesCarte",
                columns: new[] { "UtilisateurId", "CarteCompetenceId" });

            migrationBuilder.CreateIndex(
                name: "IX_VisiosEtape_ChallengeEtapeId",
                table: "VisiosEtape",
                column: "ChallengeEtapeId");

            migrationBuilder.CreateIndex(
                name: "IX_VisiosEtape_CohorteId_ChallengeEtapeId",
                table: "VisiosEtape",
                columns: new[] { "CohorteId", "ChallengeEtapeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VisiosEtape_PlanifieParId",
                table: "VisiosEtape",
                column: "PlanifieParId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CommentairesCarte");

            migrationBuilder.DropTable(
                name: "VisiosEtape");
        }
    }
}

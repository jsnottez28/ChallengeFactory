using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Web.Migrations
{
    /// <inheritdoc />
    public partial class AddTestPositionnement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TestsPositionnement",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CohorteId = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    EnvoyeLe = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EnvoyeParId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestsPositionnement", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TestsPositionnement_AspNetUsers_EnvoyeParId",
                        column: x => x.EnvoyeParId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TestsPositionnement_Cohortes_CohorteId",
                        column: x => x.CohorteId,
                        principalTable: "Cohortes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TestsPositionnementReponses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TestPositionnementId = table.Column<int>(type: "int", nullable: false),
                    UtilisateurId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CarteCompetenceId = table.Column<int>(type: "int", nullable: false),
                    NiveauAutoEvalue = table.Column<int>(type: "int", nullable: false),
                    RepondueLe = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestsPositionnementReponses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TestsPositionnementReponses_AspNetUsers_UtilisateurId",
                        column: x => x.UtilisateurId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TestsPositionnementReponses_CartesCompetences_CarteCompetenceId",
                        column: x => x.CarteCompetenceId,
                        principalTable: "CartesCompetences",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TestsPositionnementReponses_TestsPositionnement_TestPositionnementId",
                        column: x => x.TestPositionnementId,
                        principalTable: "TestsPositionnement",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TestsPositionnement_CohorteId_Type",
                table: "TestsPositionnement",
                columns: new[] { "CohorteId", "Type" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TestsPositionnement_EnvoyeParId",
                table: "TestsPositionnement",
                column: "EnvoyeParId");

            migrationBuilder.CreateIndex(
                name: "IX_TestsPositionnementReponses_CarteCompetenceId",
                table: "TestsPositionnementReponses",
                column: "CarteCompetenceId");

            migrationBuilder.CreateIndex(
                name: "IX_TestsPositionnementReponses_TestPositionnementId_UtilisateurId_CarteCompetenceId",
                table: "TestsPositionnementReponses",
                columns: new[] { "TestPositionnementId", "UtilisateurId", "CarteCompetenceId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TestsPositionnementReponses_UtilisateurId",
                table: "TestsPositionnementReponses",
                column: "UtilisateurId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TestsPositionnementReponses");

            migrationBuilder.DropTable(
                name: "TestsPositionnement");
        }
    }
}

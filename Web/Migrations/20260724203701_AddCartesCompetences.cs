using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Web.Migrations
{
    /// <inheritdoc />
    public partial class AddCartesCompetences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Badges",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BadgeCode = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    BadgeNom = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BadgeImage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Programme = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Badges", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CartesCompetences",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    BadgeId = table.Column<int>(type: "int", nullable: true),
                    Niveau = table.Column<int>(type: "int", nullable: false),
                    TitreTheorie = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Objectif1 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Objectif2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Objectif3 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Objectif4 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Citation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AuteurCitation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ImageCarteA = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TitreDefi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ContextePro = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ContextePerso = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TonDefi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Etape1 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Etape2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Etape3 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Etape4 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Etape5 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Tip1 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Tip2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Tip3 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Tip4 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Tip5 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CitationHumour = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LienVideo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreeLe = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifieLe = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CartesCompetences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CartesCompetences_Badges_BadgeId",
                        column: x => x.BadgeId,
                        principalTable: "Badges",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "CarteAttributions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CarteCompetenceId = table.Column<int>(type: "int", nullable: false),
                    UtilisateurId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    AttribueParId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    AttribueLe = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Contexte = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EstActif = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CarteAttributions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CarteAttributions_AspNetUsers_AttribueParId",
                        column: x => x.AttribueParId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CarteAttributions_AspNetUsers_UtilisateurId",
                        column: x => x.UtilisateurId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CarteAttributions_CartesCompetences_CarteCompetenceId",
                        column: x => x.CarteCompetenceId,
                        principalTable: "CartesCompetences",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_CiviliteId",
                table: "AspNetUsers",
                column: "CiviliteId");

            migrationBuilder.CreateIndex(
                name: "IX_Badges_BadgeCode",
                table: "Badges",
                column: "BadgeCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CarteAttributions_AttribueParId",
                table: "CarteAttributions",
                column: "AttribueParId");

            migrationBuilder.CreateIndex(
                name: "IX_CarteAttributions_CarteCompetenceId_UtilisateurId",
                table: "CarteAttributions",
                columns: new[] { "CarteCompetenceId", "UtilisateurId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CarteAttributions_UtilisateurId",
                table: "CarteAttributions",
                column: "UtilisateurId");

            migrationBuilder.CreateIndex(
                name: "IX_CartesCompetences_BadgeId",
                table: "CartesCompetences",
                column: "BadgeId");

            migrationBuilder.CreateIndex(
                name: "IX_CartesCompetences_Code",
                table: "CartesCompetences",
                column: "Code",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Civilites_CiviliteId",
                table: "AspNetUsers",
                column: "CiviliteId",
                principalTable: "Civilites",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Civilites_CiviliteId",
                table: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "CarteAttributions");

            migrationBuilder.DropTable(
                name: "CartesCompetences");

            migrationBuilder.DropTable(
                name: "Badges");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_CiviliteId",
                table: "AspNetUsers");
        }
    }
}

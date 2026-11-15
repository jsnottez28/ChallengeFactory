using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Web.Migrations
{
    /// <inheritdoc />
    public partial class AddOffresFormation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OffresFormation",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Titre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Categorie = table.Column<int>(type: "int", nullable: false),
                    LienAvecOffre = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Duree = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Public = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Financement = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Prix = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DisponibleEnChallenge = table.Column<bool>(type: "bit", nullable: false),
                    DisponibleEnPresentiel = table.Column<bool>(type: "bit", nullable: false),
                    Cible = table.Column<int>(type: "int", nullable: false),
                    Statut = table.Column<int>(type: "int", nullable: false),
                    ChallengeId = table.Column<int>(type: "int", nullable: true),
                    LienPersonnalise = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Ordre = table.Column<int>(type: "int", nullable: false),
                    EstPublie = table.Column<bool>(type: "bit", nullable: false),
                    CreeLe = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OffresFormation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OffresFormation_Challenges_ChallengeId",
                        column: x => x.ChallengeId,
                        principalTable: "Challenges",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_OffresFormation_ChallengeId",
                table: "OffresFormation",
                column: "ChallengeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OffresFormation");
        }
    }
}

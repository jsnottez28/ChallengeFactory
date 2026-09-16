using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Web.Migrations
{
    /// <inheritdoc />
    public partial class AddQuestionnaireMiParcours : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "QuestionnairesMiParcoursReponses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CohorteId = table.Column<int>(type: "int", nullable: false),
                    UtilisateurId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProjetToujoursPertinent = table.Column<int>(type: "int", nullable: false),
                    NoteAccompagnement = table.Column<int>(type: "int", nullable: false),
                    DifficultesRencontrees = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AjustementsSouhaites = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RepondueLe = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestionnairesMiParcoursReponses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuestionnairesMiParcoursReponses_AspNetUsers_UtilisateurId",
                        column: x => x.UtilisateurId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_QuestionnairesMiParcoursReponses_Cohortes_CohorteId",
                        column: x => x.CohorteId,
                        principalTable: "Cohortes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_QuestionnairesMiParcoursReponses_CohorteId_UtilisateurId",
                table: "QuestionnairesMiParcoursReponses",
                columns: new[] { "CohorteId", "UtilisateurId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_QuestionnairesMiParcoursReponses_UtilisateurId",
                table: "QuestionnairesMiParcoursReponses",
                column: "UtilisateurId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "QuestionnairesMiParcoursReponses");
        }
    }
}

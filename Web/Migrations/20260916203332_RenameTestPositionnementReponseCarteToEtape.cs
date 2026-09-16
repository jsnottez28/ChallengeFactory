using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Web.Migrations
{
    /// <inheritdoc />
    public partial class RenameTestPositionnementReponseCarteToEtape : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TestsPositionnementReponses_CartesCompetences_CarteCompetenceId",
                table: "TestsPositionnementReponses");

            migrationBuilder.RenameColumn(
                name: "CarteCompetenceId",
                table: "TestsPositionnementReponses",
                newName: "ChallengeEtapeId");

            migrationBuilder.RenameIndex(
                name: "IX_TestsPositionnementReponses_TestPositionnementId_UtilisateurId_CarteCompetenceId",
                table: "TestsPositionnementReponses",
                newName: "IX_TestsPositionnementReponses_TestPositionnementId_UtilisateurId_ChallengeEtapeId");

            migrationBuilder.RenameIndex(
                name: "IX_TestsPositionnementReponses_CarteCompetenceId",
                table: "TestsPositionnementReponses",
                newName: "IX_TestsPositionnementReponses_ChallengeEtapeId");

            migrationBuilder.AddForeignKey(
                name: "FK_TestsPositionnementReponses_ChallengeEtapes_ChallengeEtapeId",
                table: "TestsPositionnementReponses",
                column: "ChallengeEtapeId",
                principalTable: "ChallengeEtapes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TestsPositionnementReponses_ChallengeEtapes_ChallengeEtapeId",
                table: "TestsPositionnementReponses");

            migrationBuilder.RenameColumn(
                name: "ChallengeEtapeId",
                table: "TestsPositionnementReponses",
                newName: "CarteCompetenceId");

            migrationBuilder.RenameIndex(
                name: "IX_TestsPositionnementReponses_TestPositionnementId_UtilisateurId_ChallengeEtapeId",
                table: "TestsPositionnementReponses",
                newName: "IX_TestsPositionnementReponses_TestPositionnementId_UtilisateurId_CarteCompetenceId");

            migrationBuilder.RenameIndex(
                name: "IX_TestsPositionnementReponses_ChallengeEtapeId",
                table: "TestsPositionnementReponses",
                newName: "IX_TestsPositionnementReponses_CarteCompetenceId");

            migrationBuilder.AddForeignKey(
                name: "FK_TestsPositionnementReponses_CartesCompetences_CarteCompetenceId",
                table: "TestsPositionnementReponses",
                column: "CarteCompetenceId",
                principalTable: "CartesCompetences",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}

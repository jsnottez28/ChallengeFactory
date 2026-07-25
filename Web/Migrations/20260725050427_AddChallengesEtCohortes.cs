using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Web.Migrations
{
    /// <inheritdoc />
    public partial class AddChallengesEtCohortes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CarteAttributions_CarteCompetenceId_UtilisateurId",
                table: "CarteAttributions");

            migrationBuilder.AddColumn<int>(
                name: "ChallengeEtapeId",
                table: "CarteAttributions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CohorteId",
                table: "CarteAttributions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OrigineType",
                table: "CarteAttributions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Challenges",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Titre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Slogan = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NombreEtapes = table.Column<int>(type: "int", nullable: false),
                    Mode = table.Column<int>(type: "int", nullable: false),
                    Statut = table.Column<int>(type: "int", nullable: false),
                    CreeLe = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Challenges", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InvitationsComptes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UtilisateurId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Token = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreeLe = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpireLe = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UtiliseLe = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EstActif = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvitationsComptes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InvitationsComptes_AspNetUsers_UtilisateurId",
                        column: x => x.UtilisateurId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChallengeEtapes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ChallengeId = table.Column<int>(type: "int", nullable: false),
                    NumeroEtape = table.Column<int>(type: "int", nullable: false),
                    TitreEtape = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ObjectifPedagogique = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CompetenceCible = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DefiIndividuel = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChallengeEtapes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChallengeEtapes_Challenges_ChallengeId",
                        column: x => x.ChallengeId,
                        principalTable: "Challenges",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Cohortes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ChallengeId = table.Column<int>(type: "int", nullable: false),
                    Nom = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateLancement = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EtapeCourante = table.Column<int>(type: "int", nullable: false),
                    Statut = table.Column<int>(type: "int", nullable: false),
                    OrganisationId = table.Column<int>(type: "int", nullable: true),
                    CreeLe = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cohortes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Cohortes_Challenges_ChallengeId",
                        column: x => x.ChallengeId,
                        principalTable: "Challenges",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Cohortes_Organisations_OrganisationId",
                        column: x => x.OrganisationId,
                        principalTable: "Organisations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ChallengeEtapeCartes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ChallengeEtapeId = table.Column<int>(type: "int", nullable: false),
                    CarteCompetenceId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChallengeEtapeCartes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChallengeEtapeCartes_CartesCompetences_CarteCompetenceId",
                        column: x => x.CarteCompetenceId,
                        principalTable: "CartesCompetences",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ChallengeEtapeCartes_ChallengeEtapes_ChallengeEtapeId",
                        column: x => x.ChallengeEtapeId,
                        principalTable: "ChallengeEtapes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CohorteEtapeValidations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CohorteId = table.Column<int>(type: "int", nullable: false),
                    NumeroEtape = table.Column<int>(type: "int", nullable: false),
                    ValideParId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ValideLe = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CohorteEtapeValidations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CohorteEtapeValidations_AspNetUsers_ValideParId",
                        column: x => x.ValideParId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CohorteEtapeValidations_Cohortes_CohorteId",
                        column: x => x.CohorteId,
                        principalTable: "Cohortes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CohorteMembres",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CohorteId = table.Column<int>(type: "int", nullable: false),
                    UtilisateurId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    DateAjout = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MethodeAjout = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CohorteMembres", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CohorteMembres_AspNetUsers_UtilisateurId",
                        column: x => x.UtilisateurId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CohorteMembres_Cohortes_CohorteId",
                        column: x => x.CohorteId,
                        principalTable: "Cohortes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CarteAttributions_CarteCompetenceId_UtilisateurId_OrigineType_CohorteId_ChallengeEtapeId",
                table: "CarteAttributions",
                columns: new[] { "CarteCompetenceId", "UtilisateurId", "OrigineType", "CohorteId", "ChallengeEtapeId" },
                unique: true,
                filter: "[CohorteId] IS NOT NULL AND [ChallengeEtapeId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_CarteAttributions_ChallengeEtapeId",
                table: "CarteAttributions",
                column: "ChallengeEtapeId");

            migrationBuilder.CreateIndex(
                name: "IX_CarteAttributions_CohorteId",
                table: "CarteAttributions",
                column: "CohorteId");

            migrationBuilder.CreateIndex(
                name: "IX_ChallengeEtapeCartes_CarteCompetenceId",
                table: "ChallengeEtapeCartes",
                column: "CarteCompetenceId");

            migrationBuilder.CreateIndex(
                name: "IX_ChallengeEtapeCartes_ChallengeEtapeId_CarteCompetenceId",
                table: "ChallengeEtapeCartes",
                columns: new[] { "ChallengeEtapeId", "CarteCompetenceId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChallengeEtapes_ChallengeId_NumeroEtape",
                table: "ChallengeEtapes",
                columns: new[] { "ChallengeId", "NumeroEtape" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CohorteEtapeValidations_CohorteId_NumeroEtape",
                table: "CohorteEtapeValidations",
                columns: new[] { "CohorteId", "NumeroEtape" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CohorteEtapeValidations_ValideParId",
                table: "CohorteEtapeValidations",
                column: "ValideParId");

            migrationBuilder.CreateIndex(
                name: "IX_CohorteMembres_CohorteId_UtilisateurId",
                table: "CohorteMembres",
                columns: new[] { "CohorteId", "UtilisateurId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CohorteMembres_UtilisateurId",
                table: "CohorteMembres",
                column: "UtilisateurId");

            migrationBuilder.CreateIndex(
                name: "IX_Cohortes_ChallengeId",
                table: "Cohortes",
                column: "ChallengeId");

            migrationBuilder.CreateIndex(
                name: "IX_Cohortes_OrganisationId",
                table: "Cohortes",
                column: "OrganisationId");

            migrationBuilder.CreateIndex(
                name: "IX_InvitationsComptes_Token",
                table: "InvitationsComptes",
                column: "Token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InvitationsComptes_UtilisateurId",
                table: "InvitationsComptes",
                column: "UtilisateurId");

            migrationBuilder.AddForeignKey(
                name: "FK_CarteAttributions_ChallengeEtapes_ChallengeEtapeId",
                table: "CarteAttributions",
                column: "ChallengeEtapeId",
                principalTable: "ChallengeEtapes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CarteAttributions_Cohortes_CohorteId",
                table: "CarteAttributions",
                column: "CohorteId",
                principalTable: "Cohortes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CarteAttributions_ChallengeEtapes_ChallengeEtapeId",
                table: "CarteAttributions");

            migrationBuilder.DropForeignKey(
                name: "FK_CarteAttributions_Cohortes_CohorteId",
                table: "CarteAttributions");

            migrationBuilder.DropTable(
                name: "ChallengeEtapeCartes");

            migrationBuilder.DropTable(
                name: "CohorteEtapeValidations");

            migrationBuilder.DropTable(
                name: "CohorteMembres");

            migrationBuilder.DropTable(
                name: "InvitationsComptes");

            migrationBuilder.DropTable(
                name: "ChallengeEtapes");

            migrationBuilder.DropTable(
                name: "Cohortes");

            migrationBuilder.DropTable(
                name: "Challenges");

            migrationBuilder.DropIndex(
                name: "IX_CarteAttributions_CarteCompetenceId_UtilisateurId_OrigineType_CohorteId_ChallengeEtapeId",
                table: "CarteAttributions");

            migrationBuilder.DropIndex(
                name: "IX_CarteAttributions_ChallengeEtapeId",
                table: "CarteAttributions");

            migrationBuilder.DropIndex(
                name: "IX_CarteAttributions_CohorteId",
                table: "CarteAttributions");

            migrationBuilder.DropColumn(
                name: "ChallengeEtapeId",
                table: "CarteAttributions");

            migrationBuilder.DropColumn(
                name: "CohorteId",
                table: "CarteAttributions");

            migrationBuilder.DropColumn(
                name: "OrigineType",
                table: "CarteAttributions");

            migrationBuilder.CreateIndex(
                name: "IX_CarteAttributions_CarteCompetenceId_UtilisateurId",
                table: "CarteAttributions",
                columns: new[] { "CarteCompetenceId", "UtilisateurId" },
                unique: true);
        }
    }
}

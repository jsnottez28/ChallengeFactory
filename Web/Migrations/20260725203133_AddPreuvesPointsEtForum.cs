using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Web.Migrations
{
    /// <inheritdoc />
    public partial class AddPreuvesPointsEtForum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BadgeSocialAttributions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UtilisateurId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CohorteId = table.Column<int>(type: "int", nullable: false),
                    ChallengeEtapeId = table.Column<int>(type: "int", nullable: false),
                    TypeBadge = table.Column<int>(type: "int", nullable: false),
                    DateAttribution = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BadgeSocialAttributions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BadgeSocialAttributions_AspNetUsers_UtilisateurId",
                        column: x => x.UtilisateurId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BadgeSocialAttributions_ChallengeEtapes_ChallengeEtapeId",
                        column: x => x.ChallengeEtapeId,
                        principalTable: "ChallengeEtapes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BadgeSocialAttributions_Cohortes_CohorteId",
                        column: x => x.CohorteId,
                        principalTable: "Cohortes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ForumMessages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CohorteId = table.Column<int>(type: "int", nullable: false),
                    ChallengeEtapeId = table.Column<int>(type: "int", nullable: false),
                    AuteurId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Contenu = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MessageParentId = table.Column<int>(type: "int", nullable: true),
                    DateCreation = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ForumMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ForumMessages_AspNetUsers_AuteurId",
                        column: x => x.AuteurId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ForumMessages_ChallengeEtapes_ChallengeEtapeId",
                        column: x => x.ChallengeEtapeId,
                        principalTable: "ChallengeEtapes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ForumMessages_Cohortes_CohorteId",
                        column: x => x.CohorteId,
                        principalTable: "Cohortes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ForumMessages_ForumMessages_MessageParentId",
                        column: x => x.MessageParentId,
                        principalTable: "ForumMessages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PointsEvenements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UtilisateurId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CohorteId = table.Column<int>(type: "int", nullable: true),
                    TypePoints = table.Column<int>(type: "int", nullable: false),
                    Montant = table.Column<int>(type: "int", nullable: false),
                    Motif = table.Column<int>(type: "int", nullable: false),
                    ReferenceType = table.Column<int>(type: "int", nullable: false),
                    ReferenceId = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PointsEvenements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PointsEvenements_AspNetUsers_UtilisateurId",
                        column: x => x.UtilisateurId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PointsEvenements_Cohortes_CohorteId",
                        column: x => x.CohorteId,
                        principalTable: "Cohortes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Preuves",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UtilisateurId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CohorteId = table.Column<int>(type: "int", nullable: false),
                    ChallengeEtapeId = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateDepot = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Statut = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Preuves", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Preuves_AspNetUsers_UtilisateurId",
                        column: x => x.UtilisateurId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Preuves_ChallengeEtapes_ChallengeEtapeId",
                        column: x => x.ChallengeEtapeId,
                        principalTable: "ChallengeEtapes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Preuves_Cohortes_CohorteId",
                        column: x => x.CohorteId,
                        principalTable: "Cohortes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ForumMessagesUtiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MessageId = table.Column<int>(type: "int", nullable: false),
                    MarqueParId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ForumMessagesUtiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ForumMessagesUtiles_AspNetUsers_MarqueParId",
                        column: x => x.MarqueParId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ForumMessagesUtiles_ForumMessages_MessageId",
                        column: x => x.MessageId,
                        principalTable: "ForumMessages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PreuveFichiers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PreuveId = table.Column<int>(type: "int", nullable: false),
                    TypeFichier = table.Column<int>(type: "int", nullable: false),
                    NomFichier = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CheminStockage = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TailleOctets = table.Column<long>(type: "bigint", nullable: false),
                    DateUpload = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PreuveFichiers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PreuveFichiers_Preuves_PreuveId",
                        column: x => x.PreuveId,
                        principalTable: "Preuves",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PreuveValidationsGestionnaire",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PreuveId = table.Column<int>(type: "int", nullable: false),
                    ValideurId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Decision = table.Column<int>(type: "int", nullable: false),
                    Commentaire = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateValidation = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PreuveValidationsGestionnaire", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PreuveValidationsGestionnaire_AspNetUsers_ValideurId",
                        column: x => x.ValideurId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PreuveValidationsGestionnaire_Preuves_PreuveId",
                        column: x => x.PreuveId,
                        principalTable: "Preuves",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PreuveValidationsPairs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PreuveId = table.Column<int>(type: "int", nullable: false),
                    ValideurId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Decision = table.Column<int>(type: "int", nullable: false),
                    Commentaire = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateValidation = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PreuveValidationsPairs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PreuveValidationsPairs_AspNetUsers_ValideurId",
                        column: x => x.ValideurId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PreuveValidationsPairs_Preuves_PreuveId",
                        column: x => x.PreuveId,
                        principalTable: "Preuves",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BadgeSocialAttributions_ChallengeEtapeId",
                table: "BadgeSocialAttributions",
                column: "ChallengeEtapeId");

            migrationBuilder.CreateIndex(
                name: "IX_BadgeSocialAttributions_CohorteId",
                table: "BadgeSocialAttributions",
                column: "CohorteId");

            migrationBuilder.CreateIndex(
                name: "IX_BadgeSocialAttributions_UtilisateurId_CohorteId_ChallengeEtapeId_TypeBadge",
                table: "BadgeSocialAttributions",
                columns: new[] { "UtilisateurId", "CohorteId", "ChallengeEtapeId", "TypeBadge" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ForumMessages_AuteurId",
                table: "ForumMessages",
                column: "AuteurId");

            migrationBuilder.CreateIndex(
                name: "IX_ForumMessages_ChallengeEtapeId",
                table: "ForumMessages",
                column: "ChallengeEtapeId");

            migrationBuilder.CreateIndex(
                name: "IX_ForumMessages_CohorteId",
                table: "ForumMessages",
                column: "CohorteId");

            migrationBuilder.CreateIndex(
                name: "IX_ForumMessages_MessageParentId",
                table: "ForumMessages",
                column: "MessageParentId");

            migrationBuilder.CreateIndex(
                name: "IX_ForumMessagesUtiles_MarqueParId",
                table: "ForumMessagesUtiles",
                column: "MarqueParId");

            migrationBuilder.CreateIndex(
                name: "IX_ForumMessagesUtiles_MessageId_MarqueParId",
                table: "ForumMessagesUtiles",
                columns: new[] { "MessageId", "MarqueParId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PointsEvenements_CohorteId",
                table: "PointsEvenements",
                column: "CohorteId");

            migrationBuilder.CreateIndex(
                name: "IX_PointsEvenements_UtilisateurId",
                table: "PointsEvenements",
                column: "UtilisateurId");

            migrationBuilder.CreateIndex(
                name: "IX_PreuveFichiers_PreuveId",
                table: "PreuveFichiers",
                column: "PreuveId");

            migrationBuilder.CreateIndex(
                name: "IX_Preuves_ChallengeEtapeId",
                table: "Preuves",
                column: "ChallengeEtapeId");

            migrationBuilder.CreateIndex(
                name: "IX_Preuves_CohorteId",
                table: "Preuves",
                column: "CohorteId");

            migrationBuilder.CreateIndex(
                name: "IX_Preuves_UtilisateurId_ChallengeEtapeId",
                table: "Preuves",
                columns: new[] { "UtilisateurId", "ChallengeEtapeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PreuveValidationsGestionnaire_PreuveId",
                table: "PreuveValidationsGestionnaire",
                column: "PreuveId");

            migrationBuilder.CreateIndex(
                name: "IX_PreuveValidationsGestionnaire_ValideurId",
                table: "PreuveValidationsGestionnaire",
                column: "ValideurId");

            migrationBuilder.CreateIndex(
                name: "IX_PreuveValidationsPairs_PreuveId_ValideurId",
                table: "PreuveValidationsPairs",
                columns: new[] { "PreuveId", "ValideurId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PreuveValidationsPairs_ValideurId",
                table: "PreuveValidationsPairs",
                column: "ValideurId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BadgeSocialAttributions");

            migrationBuilder.DropTable(
                name: "ForumMessagesUtiles");

            migrationBuilder.DropTable(
                name: "PointsEvenements");

            migrationBuilder.DropTable(
                name: "PreuveFichiers");

            migrationBuilder.DropTable(
                name: "PreuveValidationsGestionnaire");

            migrationBuilder.DropTable(
                name: "PreuveValidationsPairs");

            migrationBuilder.DropTable(
                name: "ForumMessages");

            migrationBuilder.DropTable(
                name: "Preuves");
        }
    }
}

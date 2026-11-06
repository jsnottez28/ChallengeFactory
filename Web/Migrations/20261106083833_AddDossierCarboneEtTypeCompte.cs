using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Web.Migrations
{
    /// <inheritdoc />
    public partial class AddDossierCarboneEtTypeCompte : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TypeCompte",
                table: "AspNetUsers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "DossiersCarbone",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PreBilanCarboneId = table.Column<int>(type: "int", nullable: false),
                    ClientUtilisateurId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RaisonSociale = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    OuvertLe = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OuvertParId = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DossiersCarbone", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DossiersCarbone_AspNetUsers_ClientUtilisateurId",
                        column: x => x.ClientUtilisateurId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DossiersCarbone_PreBilansCarbone_PreBilanCarboneId",
                        column: x => x.PreBilanCarboneId,
                        principalTable: "PreBilansCarbone",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "QuestionnairesCollecte",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DossierCarboneId = table.Column<int>(type: "int", nullable: false),
                    Statut = table.Column<int>(type: "int", nullable: false),
                    ReponsesJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EnvoyeLe = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifieLe = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SoumisLe = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestionnairesCollecte", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuestionnairesCollecte_DossiersCarbone_DossierCarboneId",
                        column: x => x.DossierCarboneId,
                        principalTable: "DossiersCarbone",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DossiersCarbone_ClientUtilisateurId",
                table: "DossiersCarbone",
                column: "ClientUtilisateurId");

            migrationBuilder.CreateIndex(
                name: "IX_DossiersCarbone_PreBilanCarboneId",
                table: "DossiersCarbone",
                column: "PreBilanCarboneId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_QuestionnairesCollecte_DossierCarboneId",
                table: "QuestionnairesCollecte",
                column: "DossierCarboneId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "QuestionnairesCollecte");

            migrationBuilder.DropTable(
                name: "DossiersCarbone");

            migrationBuilder.DropColumn(
                name: "TypeCompte",
                table: "AspNetUsers");
        }
    }
}

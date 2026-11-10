using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Web.Migrations
{
    /// <inheritdoc />
    public partial class AddDossierCarboneCollaborateurs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DossierCarboneCollaborateurs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DossierCarboneId = table.Column<int>(type: "int", nullable: false),
                    UtilisateurId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    InviteLe = table.Column<DateTime>(type: "datetime2", nullable: false),
                    InviteParId = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DossierCarboneCollaborateurs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DossierCarboneCollaborateurs_AspNetUsers_UtilisateurId",
                        column: x => x.UtilisateurId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DossierCarboneCollaborateurs_DossiersCarbone_DossierCarboneId",
                        column: x => x.DossierCarboneId,
                        principalTable: "DossiersCarbone",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DossierCarboneCollaborateurs_DossierCarboneId_UtilisateurId",
                table: "DossierCarboneCollaborateurs",
                columns: new[] { "DossierCarboneId", "UtilisateurId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DossierCarboneCollaborateurs_UtilisateurId",
                table: "DossierCarboneCollaborateurs",
                column: "UtilisateurId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DossierCarboneCollaborateurs");
        }
    }
}

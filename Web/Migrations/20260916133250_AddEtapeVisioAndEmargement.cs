using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Web.Migrations
{
    /// <inheritdoc />
    public partial class AddEtapeVisioAndEmargement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Emargements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CarteAttributionId = table.Column<int>(type: "int", nullable: false),
                    EnvoyeLe = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SigneLe = table.Column<DateTime>(type: "datetime2", nullable: true),
                    HeuresPresence = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    HeuresTravailPersonnel = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Emargements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Emargements_CarteAttributions_CarteAttributionId",
                        column: x => x.CarteAttributionId,
                        principalTable: "CarteAttributions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EtapesVisio",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CohorteId = table.Column<int>(type: "int", nullable: false),
                    NumeroEtape = table.Column<int>(type: "int", nullable: false),
                    DateVisio = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LienVisio = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PlanifieParId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    PlanifieLe = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DernierEnvoiLe = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EtapesVisio", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EtapesVisio_AspNetUsers_PlanifieParId",
                        column: x => x.PlanifieParId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EtapesVisio_Cohortes_CohorteId",
                        column: x => x.CohorteId,
                        principalTable: "Cohortes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Emargements_CarteAttributionId",
                table: "Emargements",
                column: "CarteAttributionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EtapesVisio_CohorteId_NumeroEtape",
                table: "EtapesVisio",
                columns: new[] { "CohorteId", "NumeroEtape" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EtapesVisio_PlanifieParId",
                table: "EtapesVisio",
                column: "PlanifieParId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Emargements");

            migrationBuilder.DropTable(
                name: "EtapesVisio");
        }
    }
}

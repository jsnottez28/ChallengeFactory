using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Web.Migrations
{
    /// <inheritdoc />
    public partial class AddPreBilanCarbone : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FacteursEmission",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Poste = table.Column<int>(type: "int", nullable: false),
                    TypeDonnee = table.Column<int>(type: "int", nullable: false),
                    Nom = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Unite = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ValeurKgCO2eParUnite = table.Column<decimal>(type: "decimal(14,8)", precision: 14, scale: 8, nullable: false),
                    Source = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Ordre = table.Column<int>(type: "int", nullable: false),
                    Actif = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FacteursEmission", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PreBilansCarbone",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Nom = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Prenom = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Societe = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Telephone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecteurActivite = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EffectifEtp = table.Column<int>(type: "int", nullable: true),
                    ChiffreAffairesKEuros = table.Column<decimal>(type: "decimal(14,2)", precision: 14, scale: 2, nullable: true),
                    TotalEmissionsKgCO2e = table.Column<decimal>(type: "decimal(14,2)", precision: 14, scale: 2, nullable: false),
                    DeposeLe = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StatutCrm = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PreBilansCarbone", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PreBilanLignes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PreBilanCarboneId = table.Column<int>(type: "int", nullable: false),
                    FacteurEmissionId = table.Column<int>(type: "int", nullable: false),
                    ValeurSaisie = table.Column<decimal>(type: "decimal(14,2)", precision: 14, scale: 2, nullable: false),
                    EmissionsKgCO2e = table.Column<decimal>(type: "decimal(14,2)", precision: 14, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PreBilanLignes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PreBilanLignes_FacteursEmission_FacteurEmissionId",
                        column: x => x.FacteurEmissionId,
                        principalTable: "FacteursEmission",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PreBilanLignes_PreBilansCarbone_PreBilanCarboneId",
                        column: x => x.PreBilanCarboneId,
                        principalTable: "PreBilansCarbone",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FacteursEmission_Code",
                table: "FacteursEmission",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PreBilanLignes_FacteurEmissionId",
                table: "PreBilanLignes",
                column: "FacteurEmissionId");

            migrationBuilder.CreateIndex(
                name: "IX_PreBilanLignes_PreBilanCarboneId",
                table: "PreBilanLignes",
                column: "PreBilanCarboneId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PreBilanLignes");

            migrationBuilder.DropTable(
                name: "FacteursEmission");

            migrationBuilder.DropTable(
                name: "PreBilansCarbone");
        }
    }
}

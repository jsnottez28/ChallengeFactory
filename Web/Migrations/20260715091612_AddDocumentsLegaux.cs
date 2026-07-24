using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Web.Migrations
{
    /// <inheritdoc />
    public partial class AddDocumentsLegaux : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DocumentsLegaux",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Version = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Contenu = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateEffective = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EstPublie = table.Column<bool>(type: "bit", nullable: false),
                    CreeLe = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentsLegaux", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AcceptationsDocumentsLegaux",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ApplicationUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    DocumentLegalId = table.Column<int>(type: "int", nullable: false),
                    DateAcceptation = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AdresseIp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AcceptationsDocumentsLegaux", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AcceptationsDocumentsLegaux_AspNetUsers_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AcceptationsDocumentsLegaux_DocumentsLegaux_DocumentLegalId",
                        column: x => x.DocumentLegalId,
                        principalTable: "DocumentsLegaux",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AcceptationsDocumentsLegaux_ApplicationUserId",
                table: "AcceptationsDocumentsLegaux",
                column: "ApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AcceptationsDocumentsLegaux_DocumentLegalId",
                table: "AcceptationsDocumentsLegaux",
                column: "DocumentLegalId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentsLegaux_Type_Version",
                table: "DocumentsLegaux",
                columns: new[] { "Type", "Version" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AcceptationsDocumentsLegaux");

            migrationBuilder.DropTable(
                name: "DocumentsLegaux");
        }
    }
}

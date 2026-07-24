using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Web.Migrations
{
    /// <inheritdoc />
    public partial class AddGestionDroitsEtPermissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "AspNetRoles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "GroupesDroit",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Libelle = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupesDroit", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Ressources",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Libelle = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ressources", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TypesAction",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Libelle = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TypesAction", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RoleGroupesDroit",
                columns: table => new
                {
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    GroupeDroitId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleGroupesDroit", x => new { x.RoleId, x.GroupeDroitId });
                    table.ForeignKey(
                        name: "FK_RoleGroupesDroit_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RoleGroupesDroit_GroupesDroit_GroupeDroitId",
                        column: x => x.GroupeDroitId,
                        principalTable: "GroupesDroit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Droits",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Libelle = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RessourceId = table.Column<int>(type: "int", nullable: false),
                    TypeActionId = table.Column<int>(type: "int", nullable: false),
                    GroupeDroitId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Droits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Droits_GroupesDroit_GroupeDroitId",
                        column: x => x.GroupeDroitId,
                        principalTable: "GroupesDroit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Droits_Ressources_RessourceId",
                        column: x => x.RessourceId,
                        principalTable: "Ressources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Droits_TypesAction_TypeActionId",
                        column: x => x.TypeActionId,
                        principalTable: "TypesAction",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RoleDroits",
                columns: table => new
                {
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    DroitId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleDroits", x => new { x.RoleId, x.DroitId });
                    table.ForeignKey(
                        name: "FK_RoleDroits_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RoleDroits_Droits_DroitId",
                        column: x => x.DroitId,
                        principalTable: "Droits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Ressources",
                columns: new[] { "Id", "Code", "Description", "Libelle" },
                values: new object[,]
                {
                    { 1, "CONGRES", null, "Congrès" },
                    { 2, "COMMANDE", null, "Commande" },
                    { 3, "UTILISATEUR", null, "Utilisateur" },
                    { 4, "ORGANISATION", null, "Organisation" }
                });

            migrationBuilder.InsertData(
                table: "TypesAction",
                columns: new[] { "Id", "Code", "Description", "Libelle" },
                values: new object[,]
                {
                    { 1, "CONSULTER", null, "Consulter" },
                    { 2, "CREER", null, "Créer" },
                    { 3, "MODIFIER", null, "Modifier" },
                    { 4, "SUPPRIMER", null, "Supprimer" },
                    { 5, "VALIDER", null, "Valider" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Droits_GroupeDroitId",
                table: "Droits",
                column: "GroupeDroitId");

            migrationBuilder.CreateIndex(
                name: "IX_Droits_RessourceId_TypeActionId",
                table: "Droits",
                columns: new[] { "RessourceId", "TypeActionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Droits_TypeActionId",
                table: "Droits",
                column: "TypeActionId");

            migrationBuilder.CreateIndex(
                name: "IX_RoleDroits_DroitId",
                table: "RoleDroits",
                column: "DroitId");

            migrationBuilder.CreateIndex(
                name: "IX_RoleGroupesDroit_GroupeDroitId",
                table: "RoleGroupesDroit",
                column: "GroupeDroitId");

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RoleDroits");

            migrationBuilder.DropTable(
                name: "RoleGroupesDroit");

            migrationBuilder.DropTable(
                name: "Droits");

            migrationBuilder.DropTable(
                name: "GroupesDroit");

            migrationBuilder.DropTable(
                name: "Ressources");

            migrationBuilder.DropTable(
                name: "TypesAction");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "AspNetRoles");
        }
    }
}

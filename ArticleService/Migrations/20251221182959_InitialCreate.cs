using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ArticleService.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Articles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nom = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Reference = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Categorie = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Marque = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Modele = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Prix = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    DureeGarantie = table.Column<int>(type: "int", nullable: false),
                    EstDisponible = table.Column<bool>(type: "bit", nullable: false),
                    Stock = table.Column<int>(type: "int", nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    DateCreation = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateMiseAJour = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Articles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CustomerArticles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClientId = table.Column<int>(type: "int", nullable: false),
                    ArticleId = table.Column<int>(type: "int", nullable: false),
                    NumeroSerie = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DateAchat = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateFinGarantie = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EstSousGarantie = table.Column<bool>(type: "bit", nullable: false),
                    NumeroFacture = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Remarques = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DateCreation = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateMiseAJour = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerArticles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomerArticles_Articles_ArticleId",
                        column: x => x.ArticleId,
                        principalTable: "Articles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Articles",
                columns: new[] { "Id", "Categorie", "DateCreation", "DateMiseAJour", "Description", "DureeGarantie", "EstDisponible", "ImageUrl", "Marque", "Modele", "Nom", "Prix", "Reference", "Stock", "Type" },
                values: new object[,]
                {
                    { 1, "Sanitaire", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "Mitigeur chromé pour lavabo avec économiseur d'eau", 60, true, null, "Grohe", "Eurosmart", "Robinet Mitigeur Lavabo", 89.99m, "ROB-LAV-001", 25, "Robinetterie" },
                    { 2, "Sanitaire", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "Mitigeur thermostatique pour douche", 60, true, null, "Hansgrohe", "ShowerSelect", "Robinet Mitigeur Douche", 245.00m, "ROB-DOU-001", 15, "Robinetterie" },
                    { 3, "Sanitaire", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "WC suspendu avec chasse d'eau économique", 24, true, null, "Geberit", "Rimfree", "WC Suspendu Compact", 320.00m, "WC-SUS-001", 10, "WC" },
                    { 4, "Sanitaire", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "Lavabo rectangulaire 60x45cm", 24, true, null, "Ideal Standard", "Connect", "Lavabo en Céramique", 125.00m, "LAV-CER-001", 20, "Lavabo" },
                    { 5, "Sanitaire", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "Baignoire rectangulaire 170x75cm", 120, true, null, "Villeroy & Boch", "Oberon", "Baignoire Acrylique", 580.00m, "BAI-ACR-001", 8, "Baignoire" },
                    { 6, "Chauffage", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "Chaudière à condensation 24kW", 24, true, null, "Viessmann", "Vitodens 100-W", "Chaudière Gaz Murale", 2850.00m, "CHAU-GAZ-001", 5, "Chaudière" },
                    { 7, "Chauffage", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "Chaudière au sol à condensation 30kW", 24, true, null, "De Dietrich", "MCR-G 24/28 MI+", "Chaudière Gaz Sol", 3450.00m, "CHAU-GAZ-002", 3, "Chaudière" },
                    { 8, "Chauffage", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "Radiateur acier type 22 - 600x1000mm", 120, true, null, "Thermor", "Evidence", "Radiateur Acier Panneau", 185.00m, "RAD-ACR-001", 30, "Radiateur" },
                    { 9, "Chauffage", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "Radiateur design vertical 1800mm", 120, true, null, "Acova", "Fassane", "Radiateur Aluminium Design", 425.00m, "RAD-ALU-001", 12, "Radiateur" },
                    { 10, "Chauffage", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "Thermostat intelligent WiFi programmable", 24, true, null, "Nest", "Learning Thermostat", "Thermostat Connecté", 249.00m, "THER-CON-001", 18, "Thermostat" },
                    { 11, "Chauffage", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "Vanne thermostatique pour radiateur", 60, true, null, "Danfoss", "RA-N", "Vanne Thermostatique", 45.00m, "VAN-THER-001", 50, "Accessoire" },
                    { 12, "Chauffage", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "Pompe à chaleur air-eau 8kW", 36, true, null, "Daikin", "Altherma 3", "Pompe à Chaleur Air-Eau", 5200.00m, "PAC-AIR-001", 2, "Pompe à Chaleur" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Articles_Categorie",
                table: "Articles",
                column: "Categorie");

            migrationBuilder.CreateIndex(
                name: "IX_Articles_Marque",
                table: "Articles",
                column: "Marque");

            migrationBuilder.CreateIndex(
                name: "IX_Articles_Reference",
                table: "Articles",
                column: "Reference",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Articles_Type",
                table: "Articles",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerArticles_ArticleId",
                table: "CustomerArticles",
                column: "ArticleId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerArticles_ClientId",
                table: "CustomerArticles",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerArticles_DateAchat",
                table: "CustomerArticles",
                column: "DateAchat");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerArticles_NumeroSerie",
                table: "CustomerArticles",
                column: "NumeroSerie",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CustomerArticles");

            migrationBuilder.DropTable(
                name: "Articles");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InterventionService.Migrations
{
    /// <inheritdoc />
    public partial class AddFacturationFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "DureeIntervention",
                table: "Interventions",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "ModePaiement",
                table: "Interventions",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MontantDeplacement",
                table: "Interventions",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "StatutPaiement",
                table: "Interventions",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TarifHoraire",
                table: "Interventions",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TauxTVA",
                table: "Interventions",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DureeIntervention",
                table: "Interventions");

            migrationBuilder.DropColumn(
                name: "ModePaiement",
                table: "Interventions");

            migrationBuilder.DropColumn(
                name: "MontantDeplacement",
                table: "Interventions");

            migrationBuilder.DropColumn(
                name: "StatutPaiement",
                table: "Interventions");

            migrationBuilder.DropColumn(
                name: "TarifHoraire",
                table: "Interventions");

            migrationBuilder.DropColumn(
                name: "TauxTVA",
                table: "Interventions");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace F1Fantasy.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddRaceResultScoring : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "FantasyPoints",
                table: "RaceResultDrivers",
                type: "decimal(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "Grid",
                table: "RaceResultDrivers",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FantasyPoints",
                table: "RaceResultDrivers");

            migrationBuilder.DropColumn(
                name: "Grid",
                table: "RaceResultDrivers");
        }
    }
}

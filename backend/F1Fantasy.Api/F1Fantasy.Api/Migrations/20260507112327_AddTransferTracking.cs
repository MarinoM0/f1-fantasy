using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace F1Fantasy.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddTransferTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "HasUsedTransfer",
                table: "FantasyTeams",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "LockedInPoints",
                table: "FantasyTeams",
                type: "decimal(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PointsAtTransfer",
                table: "FantasyTeamDrivers",
                type: "decimal(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PointsAtTransfer",
                table: "FantasyTeamConstructors",
                type: "decimal(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HasUsedTransfer",
                table: "FantasyTeams");

            migrationBuilder.DropColumn(
                name: "LockedInPoints",
                table: "FantasyTeams");

            migrationBuilder.DropColumn(
                name: "PointsAtTransfer",
                table: "FantasyTeamDrivers");

            migrationBuilder.DropColumn(
                name: "PointsAtTransfer",
                table: "FantasyTeamConstructors");
        }
    }
}

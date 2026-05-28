using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace F1Fantasy.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddPredictions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Predictions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    RaceId = table.Column<int>(type: "int", nullable: false),
                    P1DriverId = table.Column<int>(type: "int", nullable: false),
                    P2DriverId = table.Column<int>(type: "int", nullable: false),
                    P3DriverId = table.Column<int>(type: "int", nullable: false),
                    Score = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true),
                    IsScored = table.Column<bool>(type: "bit", nullable: false),
                    ScoredAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Predictions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Predictions_Drivers_P1DriverId",
                        column: x => x.P1DriverId,
                        principalTable: "Drivers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Predictions_Drivers_P2DriverId",
                        column: x => x.P2DriverId,
                        principalTable: "Drivers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Predictions_Drivers_P3DriverId",
                        column: x => x.P3DriverId,
                        principalTable: "Drivers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Predictions_Races_RaceId",
                        column: x => x.RaceId,
                        principalTable: "Races",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Predictions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Predictions_P1DriverId",
                table: "Predictions",
                column: "P1DriverId");

            migrationBuilder.CreateIndex(
                name: "IX_Predictions_P2DriverId",
                table: "Predictions",
                column: "P2DriverId");

            migrationBuilder.CreateIndex(
                name: "IX_Predictions_P3DriverId",
                table: "Predictions",
                column: "P3DriverId");

            migrationBuilder.CreateIndex(
                name: "IX_Predictions_RaceId",
                table: "Predictions",
                column: "RaceId");

            migrationBuilder.CreateIndex(
                name: "IX_Predictions_UserId_RaceId",
                table: "Predictions",
                columns: new[] { "UserId", "RaceId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Predictions");
        }
    }
}

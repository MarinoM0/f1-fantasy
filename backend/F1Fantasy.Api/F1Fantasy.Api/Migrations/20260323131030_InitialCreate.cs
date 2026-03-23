using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace F1Fantasy.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Constructors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Price = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Constructors", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Races",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoundNumber = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CircuitName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Country = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    StartTimeUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsCompleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Races", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Username = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Drivers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FirstName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Price = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    ConstructorId = table.Column<int>(type: "int", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Drivers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Drivers_Constructors_ConstructorId",
                        column: x => x.ConstructorId,
                        principalTable: "Constructors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RaceResults",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RaceId = table.Column<int>(type: "int", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RaceResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RaceResults_Races_RaceId",
                        column: x => x.RaceId,
                        principalTable: "Races",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FantasyTeams",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    BudgetCap = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    RemainingBudget = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    ConstructorId = table.Column<int>(type: "int", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FantasyTeams", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FantasyTeams_Constructors_ConstructorId",
                        column: x => x.ConstructorId,
                        principalTable: "Constructors",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_FantasyTeams_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TeamScores",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    RaceId = table.Column<int>(type: "int", nullable: false),
                    Points = table.Column<int>(type: "int", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeamScores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TeamScores_Races_RaceId",
                        column: x => x.RaceId,
                        principalTable: "Races",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TeamScores_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RaceResultDrivers",
                columns: table => new
                {
                    RaceResultId = table.Column<int>(type: "int", nullable: false),
                    DriverId = table.Column<int>(type: "int", nullable: false),
                    ConstructorId = table.Column<int>(type: "int", nullable: false),
                    Position = table.Column<int>(type: "int", nullable: false),
                    DidFinish = table.Column<bool>(type: "bit", nullable: false),
                    FastestLap = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RaceResultDrivers", x => new { x.RaceResultId, x.DriverId });
                    table.ForeignKey(
                        name: "FK_RaceResultDrivers_Constructors_ConstructorId",
                        column: x => x.ConstructorId,
                        principalTable: "Constructors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RaceResultDrivers_Drivers_DriverId",
                        column: x => x.DriverId,
                        principalTable: "Drivers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RaceResultDrivers_RaceResults_RaceResultId",
                        column: x => x.RaceResultId,
                        principalTable: "RaceResults",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FantasyTeamConstructors",
                columns: table => new
                {
                    FantasyTeamId = table.Column<int>(type: "int", nullable: false),
                    ConstructorId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FantasyTeamConstructors", x => new { x.FantasyTeamId, x.ConstructorId });
                    table.ForeignKey(
                        name: "FK_FantasyTeamConstructors_Constructors_ConstructorId",
                        column: x => x.ConstructorId,
                        principalTable: "Constructors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FantasyTeamConstructors_FantasyTeams_FantasyTeamId",
                        column: x => x.FantasyTeamId,
                        principalTable: "FantasyTeams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FantasyTeamDrivers",
                columns: table => new
                {
                    FantasyTeamId = table.Column<int>(type: "int", nullable: false),
                    DriverId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FantasyTeamDrivers", x => new { x.FantasyTeamId, x.DriverId });
                    table.ForeignKey(
                        name: "FK_FantasyTeamDrivers_Drivers_DriverId",
                        column: x => x.DriverId,
                        principalTable: "Drivers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FantasyTeamDrivers_FantasyTeams_FantasyTeamId",
                        column: x => x.FantasyTeamId,
                        principalTable: "FantasyTeams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Constructors_Code",
                table: "Constructors",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Drivers_Code",
                table: "Drivers",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Drivers_ConstructorId",
                table: "Drivers",
                column: "ConstructorId");

            migrationBuilder.CreateIndex(
                name: "IX_FantasyTeamConstructors_ConstructorId",
                table: "FantasyTeamConstructors",
                column: "ConstructorId");

            migrationBuilder.CreateIndex(
                name: "IX_FantasyTeamDrivers_DriverId",
                table: "FantasyTeamDrivers",
                column: "DriverId");

            migrationBuilder.CreateIndex(
                name: "IX_FantasyTeams_ConstructorId",
                table: "FantasyTeams",
                column: "ConstructorId");

            migrationBuilder.CreateIndex(
                name: "IX_FantasyTeams_UserId",
                table: "FantasyTeams",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RaceResultDrivers_ConstructorId",
                table: "RaceResultDrivers",
                column: "ConstructorId");

            migrationBuilder.CreateIndex(
                name: "IX_RaceResultDrivers_DriverId",
                table: "RaceResultDrivers",
                column: "DriverId");

            migrationBuilder.CreateIndex(
                name: "IX_RaceResults_RaceId",
                table: "RaceResults",
                column: "RaceId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Races_RoundNumber",
                table: "Races",
                column: "RoundNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TeamScores_RaceId",
                table: "TeamScores",
                column: "RaceId");

            migrationBuilder.CreateIndex(
                name: "IX_TeamScores_UserId_RaceId",
                table: "TeamScores",
                columns: new[] { "UserId", "RaceId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FantasyTeamConstructors");

            migrationBuilder.DropTable(
                name: "FantasyTeamDrivers");

            migrationBuilder.DropTable(
                name: "RaceResultDrivers");

            migrationBuilder.DropTable(
                name: "TeamScores");

            migrationBuilder.DropTable(
                name: "FantasyTeams");

            migrationBuilder.DropTable(
                name: "Drivers");

            migrationBuilder.DropTable(
                name: "RaceResults");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Constructors");

            migrationBuilder.DropTable(
                name: "Races");
        }
    }
}

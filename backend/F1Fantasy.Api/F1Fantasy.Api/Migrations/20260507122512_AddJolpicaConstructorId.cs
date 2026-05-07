using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace F1Fantasy.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddJolpicaConstructorId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "JolpicaConstructorId",
                table: "Constructors",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            // Backfill the new column for constructors that were seeded
            // before this column existed. Values come straight from the
            // Jolpica API (https://api.jolpi.ca/ergast/f1/current/constructorstandings.json).
            migrationBuilder.Sql(@"
                UPDATE Constructors SET JolpicaConstructorId = 'mclaren'      WHERE Code = 'MCL';
                UPDATE Constructors SET JolpicaConstructorId = 'mercedes'     WHERE Code = 'MER';
                UPDATE Constructors SET JolpicaConstructorId = 'red_bull'     WHERE Code = 'RBR';
                UPDATE Constructors SET JolpicaConstructorId = 'ferrari'      WHERE Code = 'FER';
                UPDATE Constructors SET JolpicaConstructorId = 'williams'     WHERE Code = 'WIL';
                UPDATE Constructors SET JolpicaConstructorId = 'rb'           WHERE Code = 'RBT';
                UPDATE Constructors SET JolpicaConstructorId = 'aston_martin' WHERE Code = 'AMR';
                UPDATE Constructors SET JolpicaConstructorId = 'haas'         WHERE Code = 'HAA';
                UPDATE Constructors SET JolpicaConstructorId = 'audi'         WHERE Code = 'AUD';
                UPDATE Constructors SET JolpicaConstructorId = 'alpine'       WHERE Code = 'ALP';
                UPDATE Constructors SET JolpicaConstructorId = 'cadillac'     WHERE Code = 'CAD';
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "JolpicaConstructorId",
                table: "Constructors");
        }
    }
}

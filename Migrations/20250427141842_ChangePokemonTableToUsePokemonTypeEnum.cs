using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PokemonPocket.Migrations
{
    /// <inheritdoc />
    public partial class ChangePokemonTableToUsePokemonTypeEnum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Name",
                table: "Pokemon");

            migrationBuilder.AlterColumn<int>(
                name: "PokemonType",
                table: "Pokemon",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 5);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "PokemonType",
                table: "Pokemon",
                type: "TEXT",
                maxLength: 5,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Pokemon",
                type: "TEXT",
                nullable: true);
        }
    }
}

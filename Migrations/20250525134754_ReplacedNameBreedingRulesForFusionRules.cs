using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PokemonPocket.Migrations
{
    /// <inheritdoc />
    public partial class ReplacedNameBreedingRulesForFusionRules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BreedingRules");

            migrationBuilder.CreateTable(
                name: "FusionRules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    parentAName = table.Column<string>(type: "TEXT", nullable: true),
                    parentACount = table.Column<int>(type: "INTEGER", nullable: false),
                    parentBName = table.Column<string>(type: "TEXT", nullable: true),
                    parentBCount = table.Column<int>(type: "INTEGER", nullable: false),
                    childName = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FusionRules", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FusionRules");

            migrationBuilder.CreateTable(
                name: "BreedingRules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    childName = table.Column<string>(type: "TEXT", nullable: true),
                    parentACount = table.Column<int>(type: "INTEGER", nullable: false),
                    parentAName = table.Column<string>(type: "TEXT", nullable: true),
                    parentBCount = table.Column<int>(type: "INTEGER", nullable: false),
                    parentBName = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BreedingRules", x => x.Id);
                });
        }
    }
}

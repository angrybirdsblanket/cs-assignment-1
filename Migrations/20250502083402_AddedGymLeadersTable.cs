using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PokemonPocket.Migrations
{
    /// <inheritdoc />
    public partial class AddedGymLeadersTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "GymLeaderId",
                table: "Pokemon",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "GymLeaders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BadgeId = table.Column<int>(type: "INTEGER", nullable: false),
                    GymName = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GymLeaders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GymLeaders_Badges_BadgeId",
                        column: x => x.BadgeId,
                        principalTable: "Badges",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Pokemon_GymLeaderId",
                table: "Pokemon",
                column: "GymLeaderId");

            migrationBuilder.CreateIndex(
                name: "IX_GymLeaders_BadgeId",
                table: "GymLeaders",
                column: "BadgeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Pokemon_GymLeaders_GymLeaderId",
                table: "Pokemon",
                column: "GymLeaderId",
                principalTable: "GymLeaders",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Pokemon_GymLeaders_GymLeaderId",
                table: "Pokemon");

            migrationBuilder.DropTable(
                name: "GymLeaders");

            migrationBuilder.DropIndex(
                name: "IX_Pokemon_GymLeaderId",
                table: "Pokemon");

            migrationBuilder.DropColumn(
                name: "GymLeaderId",
                table: "Pokemon");
        }
    }
}

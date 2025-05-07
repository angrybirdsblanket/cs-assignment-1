using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PokemonPocket.Migrations
{
    /// <inheritdoc />
    public partial class AddedBulbasaurAndIvysaurToDB : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GymLeaders_Badges_BadgeId",
                table: "GymLeaders");

            migrationBuilder.DropForeignKey(
                name: "FK_Pokemon_GymLeaders_GymLeaderId",
                table: "Pokemon");

            migrationBuilder.DropIndex(
                name: "IX_GymLeaders_BadgeId",
                table: "GymLeaders");

            migrationBuilder.DropColumn(
                name: "BadgeId",
                table: "GymLeaders");

            migrationBuilder.AddForeignKey(
                name: "FK_Pokemon_GymLeaders_GymLeaderId",
                table: "Pokemon",
                column: "GymLeaderId",
                principalTable: "GymLeaders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Pokemon_GymLeaders_GymLeaderId",
                table: "Pokemon");

            migrationBuilder.AddColumn<int>(
                name: "BadgeId",
                table: "GymLeaders",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_GymLeaders_BadgeId",
                table: "GymLeaders",
                column: "BadgeId");

            migrationBuilder.AddForeignKey(
                name: "FK_GymLeaders_Badges_BadgeId",
                table: "GymLeaders",
                column: "BadgeId",
                principalTable: "Badges",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Pokemon_GymLeaders_GymLeaderId",
                table: "Pokemon",
                column: "GymLeaderId",
                principalTable: "GymLeaders",
                principalColumn: "Id");
        }
    }
}

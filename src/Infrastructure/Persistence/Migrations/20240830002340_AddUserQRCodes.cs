using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SSW.Rewards.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddUserQRCodes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AchievementId",
                table: "Users",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_AchievementId",
                table: "Users",
                column: "AchievementId");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Achievements_AchievementId",
                table: "Users",
                column: "AchievementId",
                principalTable: "Achievements",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Achievements_AchievementId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_AchievementId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "AchievementId",
                table: "Users");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

namespace SSW.Consulting.Persistence.Migrations
{
    public partial class AddUserAchievementUniqueIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserAchievements_UserId",
                table: "UserAchievements");

            migrationBuilder.CreateIndex(
                name: "IX_UserAchievements_UserId_AchievementId",
                table: "UserAchievements",
                columns: new[] { "UserId", "AchievementId" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserAchievements_UserId_AchievementId",
                table: "UserAchievements");

            migrationBuilder.CreateIndex(
                name: "IX_UserAchievements_UserId",
                table: "UserAchievements",
                column: "UserId");
        }
    }
}

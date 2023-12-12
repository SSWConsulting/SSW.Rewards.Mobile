using Microsoft.EntityFrameworkCore.Migrations;

namespace SSW.Rewards.Persistence.Migrations
{
    public partial class LinkAchievementsAndStaff : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "StaffAchievementId",
                table: "StaffMembers",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_StaffMembers_StaffAchievementId",
                table: "StaffMembers",
                column: "StaffAchievementId");

            migrationBuilder.AddForeignKey(
                name: "FK_StaffMembers_Achievements_StaffAchievementId",
                table: "StaffMembers",
                column: "StaffAchievementId",
                principalTable: "Achievements",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StaffMembers_Achievements_StaffAchievementId",
                table: "StaffMembers");

            migrationBuilder.DropIndex(
                name: "IX_StaffMembers_StaffAchievementId",
                table: "StaffMembers");

            migrationBuilder.DropColumn(
                name: "StaffAchievementId",
                table: "StaffMembers");
        }
    }
}

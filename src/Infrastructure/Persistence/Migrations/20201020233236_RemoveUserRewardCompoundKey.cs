using Microsoft.EntityFrameworkCore.Migrations;

namespace SSW.Rewards.Persistence.Migrations
{
    public partial class RemoveUserRewardCompoundKey : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserRewards_UserId_RewardId",
                table: "UserRewards");

            migrationBuilder.CreateIndex(
                name: "IX_UserRewards_UserId",
                table: "UserRewards",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserRewards_UserId",
                table: "UserRewards");

            migrationBuilder.CreateIndex(
                name: "IX_UserRewards_UserId_RewardId",
                table: "UserRewards",
                columns: new[] { "UserId", "RewardId" },
                unique: true);
        }
    }
}

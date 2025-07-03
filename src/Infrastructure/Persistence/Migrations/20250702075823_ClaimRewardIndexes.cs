using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SSW.Rewards.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ClaimRewardIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserRewards_UserId",
                table: "UserRewards");

            migrationBuilder.CreateIndex(
                name: "IX_UserRewards_UserId",
                table: "UserRewards",
                column: "UserId")
                .Annotation("SqlServer:Include", new[] { "RewardId" });

            migrationBuilder.CreateIndex(
                name: "IX_Rewards_Id",
                table: "Rewards",
                column: "Id")
                .Annotation("SqlServer:Include", new[] { "Cost" });

            migrationBuilder.CreateIndex(
                name: "IX_Achievements_Id",
                table: "Achievements",
                column: "Id")
                .Annotation("SqlServer:Include", new[] { "Value" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserRewards_UserId",
                table: "UserRewards");

            migrationBuilder.DropIndex(
                name: "IX_Rewards_Id",
                table: "Rewards");

            migrationBuilder.DropIndex(
                name: "IX_Achievements_Id",
                table: "Achievements");

            migrationBuilder.CreateIndex(
                name: "IX_UserRewards_UserId",
                table: "UserRewards",
                column: "UserId");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace SSW.Consulting.Persistence.Migrations
{
    public partial class AddUserRewardConfiguration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserRewards_UserId",
                table: "UserRewards");

            migrationBuilder.AlterColumn<DateTime>(
                name: "AwardedAt",
                table: "UserRewards",
                nullable: false,
                defaultValueSql: "getdate()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.CreateIndex(
                name: "IX_UserRewards_UserId_RewardId",
                table: "UserRewards",
                columns: new[] { "UserId", "RewardId" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserRewards_UserId_RewardId",
                table: "UserRewards");

            migrationBuilder.AlterColumn<DateTime>(
                name: "AwardedAt",
                table: "UserRewards",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldDefaultValueSql: "getdate()");

            migrationBuilder.CreateIndex(
                name: "IX_UserRewards_UserId",
                table: "UserRewards",
                column: "UserId");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace SSW.Consulting.Persistence.Migrations
{
    public partial class AddAchievementAwardedAt : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "AwardedAt",
                table: "UserAchievements",
                nullable: false,
                defaultValueSql: "getdate()");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AwardedAt",
                table: "UserAchievements");
        }
    }
}

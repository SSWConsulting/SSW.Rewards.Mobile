using Microsoft.EntityFrameworkCore.Migrations;

namespace SSW.Consulting.Persistence.Migrations
{
    public partial class AddAchievementCode : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "Achievements",
                nullable: true);

            migrationBuilder.Sql("update achievements set code = lower(replace([name],' ',''))");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Code",
                table: "Achievements");
        }
    }
}

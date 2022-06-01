using Microsoft.EntityFrameworkCore.Migrations;

namespace SSW.Rewards.Persistence.Migrations
{
    public partial class AddStaffSkillLevel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Level",
                table: "StaffMemberSkills",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Level",
                table: "StaffMemberSkills");
        }
    }
}

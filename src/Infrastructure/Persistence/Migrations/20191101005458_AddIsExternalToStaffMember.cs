using Microsoft.EntityFrameworkCore.Migrations;

namespace SSW.Rewards.Persistence.Migrations
{
    public partial class AddIsExternalToStaffMember : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsExternal",
                table: "StaffMembers",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsExternal",
                table: "StaffMembers");
        }
    }
}

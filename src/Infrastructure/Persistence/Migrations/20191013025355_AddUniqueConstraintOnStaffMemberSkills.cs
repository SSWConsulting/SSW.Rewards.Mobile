using Microsoft.EntityFrameworkCore.Migrations;

namespace SSW.Rewards.Persistence.Migrations
{
    public partial class AddUniqueConstraintOnStaffMemberSkills : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("delete from staffmemberskills");

            migrationBuilder.DropIndex(
                name: "IX_StaffMemberSkills_SkillId",
                table: "StaffMemberSkills");

            migrationBuilder.CreateIndex(
                name: "IX_StaffMemberSkills_SkillId_StaffMemberId",
                table: "StaffMemberSkills",
                columns: new[] { "SkillId", "StaffMemberId" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StaffMemberSkills_SkillId_StaffMemberId",
                table: "StaffMemberSkills");

            migrationBuilder.CreateIndex(
                name: "IX_StaffMemberSkills_SkillId",
                table: "StaffMemberSkills",
                column: "SkillId");
        }
    }
}

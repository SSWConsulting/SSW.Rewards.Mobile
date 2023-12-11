using Microsoft.EntityFrameworkCore.Migrations;

namespace SSW.Rewards.Persistence.Migrations
{
    public partial class SeedRoles : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            UpsertRole(migrationBuilder, "User");
            UpsertRole(migrationBuilder, "Staff");
            UpsertRole(migrationBuilder, "Admin");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }

        private void UpsertRole(MigrationBuilder migrationBuilder, string roleName)
        {
            migrationBuilder.Sql(@$"
                IF NOT EXISTS (SELECT 1 FROM dbo.Roles WHERE Name = '{roleName}')
                BEGIN
                    INSERT INTO dbo.Roles (Name, CreatedUtc) values ('{roleName}', GETDATE())
                END
            ");
        }
    }
}

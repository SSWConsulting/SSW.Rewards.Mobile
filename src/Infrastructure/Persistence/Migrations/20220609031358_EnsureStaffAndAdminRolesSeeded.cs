using Microsoft.EntityFrameworkCore.Migrations;

namespace SSW.Rewards.Persistence.Migrations
{
    public partial class EnsureStaffAndAdminRolesSeeded : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // map user role
            migrationBuilder.Sql(@"
                INSERT INTO dbo.UserRoles (UserId, RoleId, CreatedUtc)
                SELECT
                    [User].Id,
                    [Role].Id,
                    GETDATE()
                FROM
                    dbo.Users as [User]
                CROSS JOIN dbo.Roles as [Role]
                WHERE
                    [Role].Name = 'User'
            ");
            
            // map staff role
            migrationBuilder.Sql(@"
                INSERT INTO dbo.UserRoles (UserId, RoleId, CreatedUtc)
                SELECT
                    [User].Id,
                    [Role].Id,
                    GETDATE()
                FROM
                    dbo.Users as [User]
                CROSS JOIN dbo.Roles as [Role]
                WHERE
                    [Role].Name = 'Staff'
                AND [User].[Email] like '%@ssw.com.au'
            ");

        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SSW.Rewards.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSoftDeleteIndexesToTenantEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_TenantSettings_DeletedUtc",
                table: "TenantSettings",
                column: "DeletedUtc",
                filter: "[DeletedUtc] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_DeletedUtc",
                table: "Tenants",
                column: "DeletedUtc",
                filter: "[DeletedUtc] IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TenantSettings_DeletedUtc",
                table: "TenantSettings");

            migrationBuilder.DropIndex(
                name: "IX_Tenants_DeletedUtc",
                table: "Tenants");
        }
    }
}

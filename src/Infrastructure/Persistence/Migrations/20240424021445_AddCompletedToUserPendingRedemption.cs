using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SSW.Rewards.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCompletedToUserPendingRedemption : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Completed",
                table: "PendingRedemptions",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Completed",
                table: "PendingRedemptions");
        }
    }
}

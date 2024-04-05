using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SSW.Rewards.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAchievementCodes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //TODO: Update to Base64
            migrationBuilder.Sql("UPDATE Achievements SET Code = 'ach:' + CONVERT(NVARCHAR(36), NEWID())");
        }
    }
}

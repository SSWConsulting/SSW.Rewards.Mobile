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
            migrationBuilder.Sql("UPDATE Achievements SET Code = 'ach:' + CONVERT(NVARCHAR(36), NEWID())");
            migrationBuilder.Sql("UPDATE Rewards SET Code = 'rwd:' + CONVERT(NVARCHAR(36), NEWID())");
        }
    }
}

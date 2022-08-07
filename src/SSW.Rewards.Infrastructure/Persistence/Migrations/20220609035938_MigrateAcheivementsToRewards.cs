using Microsoft.EntityFrameworkCore.Migrations;

namespace SSW.Rewards.Persistence.Migrations
{
    public partial class MigrateAcheivementsToRewards : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // fix acheivement data
            migrationBuilder.Sql(@"
                UPDATE UserAchievements
                SET CreatedUtc = CASE
                    WHEN CreatedUtc < AwardedAt THEN AwardedAt
                    ELSE CreatedUtc
                END
            ");
            
            // move "reward" acheivements to UserRewards
            migrationBuilder.Sql(@"
                INSERT INTO UserRewards (UserId, RewardId, AwardedAt, CreatedUtc)
                SELECT
                    ua.UserId,
                    r.Id,
                    ua.AwardedAt,
                    ua.CreatedUtc
                FROM UserAchievements ua
                INNER JOIN Achievements a on ua.AchievementId = a.Id and a.Value = 0
                INNER JOIN Rewards r on a.Name = r.Name
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}

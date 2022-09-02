using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SSW.Rewards.Persistence.Migrations
{
    public partial class SeedSocialMediaPlatforms : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            UpsertSocialMediaPlatforms(migrationBuilder, "GitHub", "Follow SSW on");
            UpsertSocialMediaPlatforms(migrationBuilder, "LinkedIn", "Follow SSW on");
            UpsertSocialMediaPlatforms(migrationBuilder, "Twitter", "Follow SSW TV on");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
        private void UpsertSocialMediaPlatforms(MigrationBuilder migrationBuilder, string platformName, string achievementNameString)
        {
            migrationBuilder.Sql(@$"
                IF NOT EXISTS (SELECT 1 FROM dbo.SocialMediaPlatforms WHERE Name = '{platformName}')
                BEGIN
                    INSERT INTO dbo.SocialMediaPlatforms (Name, CreatedUtc, AchievementId) 
                    values ('{platformName}', GETDATE(), (SELECT Id FROM Achievements WHERE Name = '{achievementNameString} {platformName}'))
                END
            ");
        }
    }
}

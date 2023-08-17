using Microsoft.EntityFrameworkCore.Migrations;
using SSW.Rewards.Domain.Enums;

#nullable disable

namespace SSW.Rewards.Persistence.Migrations
{
    public partial class SeedSocialMediaPlatforms : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            UpsertSocialMediaPlatforms(migrationBuilder, "GitHub", "Follow SSW on", AchievementType.Linked, Icons.Github, true, 150);
            UpsertSocialMediaPlatforms(migrationBuilder, "LinkedIn", "Follow SSW on", AchievementType.Linked, Icons.Linkedin, true, 150);
            UpsertSocialMediaPlatforms(migrationBuilder, "Twitter", "Follow SSW TV on", AchievementType.Linked, Icons.Twitter, true, 150);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
        private void UpsertSocialMediaPlatforms(MigrationBuilder migrationBuilder, string platformName, string achievementNameString, AchievementType type, Icons icon, bool branded, int value)
        {
            string achievementName = $"{achievementNameString} {platformName}";

            migrationBuilder.Sql(@$"
                -- First, ensure the Achievement exists
                IF NOT EXISTS (SELECT 1 FROM dbo.Achievements WHERE Name = '{achievementName}')
                BEGIN
                    INSERT INTO dbo.Achievements (Name, Value, Type, Icon, IconIsBranded, IsDeleted)
                    VALUES ('{achievementName}', {value}, {(int)type}, {(int)icon}, '{branded}', 'false')
                END

                -- Then, ensure the SocialMediaPlatform exists
                IF NOT EXISTS (SELECT 1 FROM dbo.SocialMediaPlatforms WHERE Name = '{platformName}')
                BEGIN
                    INSERT INTO dbo.SocialMediaPlatforms (Name, CreatedUtc, AchievementId) 
                    VALUES ('{platformName}', GETDATE(), (SELECT Id FROM dbo.Achievements WHERE Name = '{achievementName}'))
                END
            ");
        }
    }
}

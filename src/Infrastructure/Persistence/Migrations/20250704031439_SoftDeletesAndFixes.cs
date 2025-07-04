using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SSW.Rewards.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SoftDeletesAndFixes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Inject raw SQL before schema changes
            migrationBuilder.Sql(@"
                UPDATE OpenProfileDeletionRequests
                SET CreatedUtc = Created
                WHERE CreatedUtc IS NULL;
            ");

            migrationBuilder.DropColumn(
                name: "Created",
                table: "OpenProfileDeletionRequests");

            migrationBuilder.RenameColumn(
                name: "LastModified",
                table: "OpenProfileDeletionRequests",
                newName: "LastModifiedUtc");

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "UserSocialMediaIds",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "UserRoles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "UserRewards",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "UserAchievements",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "UnclaimedAchievements",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "SubmittedAnswers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "StaffMemberSkills",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "StaffMembers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "SocialMediaPlatforms",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Skills",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Roles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Rewards",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "QuizQuestions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "QuizAnswers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "PendingRedemptions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "OpenProfileDeletionRequests",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedUtc",
                table: "OpenProfileDeletionRequests",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Notifications",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "DeviceTokens",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "CompletedQuizzes",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Addresses",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Achievements",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_OpenProfileDeletionRequests_DeletedUtc",
                table: "OpenProfileDeletionRequests",
                column: "DeletedUtc",
                filter: "[DeletedUtc] IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_OpenProfileDeletionRequests_DeletedUtc",
                table: "OpenProfileDeletionRequests");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "UserSocialMediaIds");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "UserRoles");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "UserRewards");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "UserAchievements");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "UnclaimedAchievements");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "SubmittedAnswers");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "StaffMemberSkills");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "StaffMembers");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "SocialMediaPlatforms");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Skills");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Roles");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Rewards");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "QuizQuestions");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "QuizAnswers");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "PendingRedemptions");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "OpenProfileDeletionRequests");

            migrationBuilder.DropColumn(
                name: "DeletedUtc",
                table: "OpenProfileDeletionRequests");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "DeviceTokens");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "CompletedQuizzes");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Addresses");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Achievements");

            migrationBuilder.RenameColumn(
                name: "LastModifiedUtc",
                table: "OpenProfileDeletionRequests",
                newName: "LastModified");

            migrationBuilder.AddColumn<DateTime>(
                name: "Created",
                table: "OpenProfileDeletionRequests",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SSW.Rewards.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddNotificationSoftDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "Notifications",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedUtc",
                table: "Notifications",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastModifiedBy",
                table: "Notifications",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedUtc",
                table: "Notifications",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_DeletedUtc",
                table: "Notifications",
                column: "DeletedUtc",
                filter: "[DeletedUtc] IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Notifications_DeletedUtc",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "DeletedUtc",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "LastModifiedBy",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "LastModifiedUtc",
                table: "Notifications");
        }
    }
}

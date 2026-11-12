using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SSW.Rewards.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddRewardClaimDelivery : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RewardClaimDeliveries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserRewardId = table.Column<int>(type: "int", nullable: false),
                    FreeformAddress = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    StreetNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    StreetName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Municipality = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    State = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PostalCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Country = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Attempts = table.Column<int>(type: "int", nullable: false),
                    LastError = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    LastAttemptUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeliveredUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RewardClaimDeliveries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RewardClaimDeliveries_UserRewards_UserRewardId",
                        column: x => x.UserRewardId,
                        principalTable: "UserRewards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RewardClaimDeliveries_Status",
                table: "RewardClaimDeliveries",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_RewardClaimDeliveries_UserRewardId",
                table: "RewardClaimDeliveries",
                column: "UserRewardId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RewardClaimDeliveries");
        }
    }
}

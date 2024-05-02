using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SSW.Rewards.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPendingRedemption : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PendingRedemptions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RewardId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    CancelledByUser = table.Column<bool>(type: "bit", nullable: false),
                    CancelledByAdmin = table.Column<bool>(type: "bit", nullable: false),
                    CreatedUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PendingRedemptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PendingRedemptions_Rewards_RewardId",
                        column: x => x.RewardId,
                        principalTable: "Rewards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PendingRedemptions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PendingRedemptions_RewardId",
                table: "PendingRedemptions",
                column: "RewardId");

            migrationBuilder.CreateIndex(
                name: "IX_PendingRedemptions_UserId",
                table: "PendingRedemptions",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PendingRedemptions");
        }
    }
}

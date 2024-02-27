using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SSW.Rewards.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddRewardCarouselImage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CarouselImageUri",
                table: "Rewards",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsCarousel",
                table: "Rewards",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CarouselImageUri",
                table: "Rewards");

            migrationBuilder.DropColumn(
                name: "IsCarousel",
                table: "Rewards");
        }
    }
}

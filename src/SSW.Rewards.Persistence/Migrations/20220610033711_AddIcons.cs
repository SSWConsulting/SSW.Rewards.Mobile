using Microsoft.EntityFrameworkCore.Migrations;

namespace SSW.Rewards.Persistence.Migrations
{
    public partial class AddIcons : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Icon",
                table: "Rewards",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IconIsBranded",
                table: "Rewards",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "Icon",
                table: "Achievements",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IconIsBranded",
                table: "Achievements",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Icon",
                table: "Rewards");

            migrationBuilder.DropColumn(
                name: "IconIsBranded",
                table: "Rewards");

            migrationBuilder.DropColumn(
                name: "Icon",
                table: "Achievements");

            migrationBuilder.DropColumn(
                name: "IconIsBranded",
                table: "Achievements");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SSW.Rewards.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CreateTenantsAndTenantSettingsTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Tenants",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tenants", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TenantSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    CompanyName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CompanyLegalName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CompanyWebsiteUrl = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    ApplicationName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ApplicationShortName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ApplicationTagline = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    LogoUrl = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    FaviconUrl = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    PrimaryColor = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    SecondaryColor = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    AccentColor = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    BackgroundColor = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TextColor = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    SupportEmail = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    MarketingEmail = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    StaffEmailDomain = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DefaultSenderEmail = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DefaultSenderName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ProfileDeletionRecipient = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ApiBaseUrl = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    IdentityServerUrl = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    QuizServiceUrl = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    AdminPortalUrl = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    LinkedInUrl = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    TwitterUrl = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    FacebookUrl = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    InstagramUrl = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    YouTubeUrl = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenantSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TenantSettings_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TenantSettings_TenantId",
                table: "TenantSettings",
                column: "TenantId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TenantSettings");

            migrationBuilder.DropTable(
                name: "Tenants");
        }
    }
}

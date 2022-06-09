using Microsoft.EntityFrameworkCore.Migrations;

namespace SSW.Rewards.Persistence.Migrations
{
    public partial class MapStaffAchievements : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // remove duplicates
            migrationBuilder.Sql(@"
                delete A
                FROM Achievements A
                INNER JOIN (SELECT Name, MIN(CreatedUtc) as CreatedUtc FROM Achievements GROUP BY Name HAVING COUNT(0) > 1) dupe on A.Name = dupe.Name and A.CreatedUtc = dupe.CreatedUtc
            ");
            
            // map staff to achievements
            migrationBuilder.Sql(@"
                UPDATE sm
                SET
                    sm.StaffAchievementId = a.Id
                FROM
                    StaffMembers as sm
                INNER JOIN Achievements as a on sm.Name = a.Name
            ");
            
            // map staff onto achievement type
            migrationBuilder.Sql(@"
                UPDATE A
                SET
                    [Type] = CASE
                        WHEN SM.Id IS NULL THEN 1
                        ELSE 0
                    END
                FROM
                    Achievements A
                LEFT JOIN StaffMembers SM ON A.Id = SM.StaffAchievementId
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}

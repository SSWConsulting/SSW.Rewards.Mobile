using Microsoft.EntityFrameworkCore.Migrations;

namespace SSW.Rewards.Persistence.Migrations
{
    public partial class MapStaffAchievements : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // fix typo
            migrationBuilder.Sql(@"
                UPDATE Achievements
                SET Name = 'Kosta Madorsky'
                WHERE Name = 'Kosta Madorski'
            ");

            // remove duplicates
            migrationBuilder.Sql(@"
                delete A
                FROM Achievements A
                WHERE Id NOT IN (
				SELECT Achievements.Id  FROM Achievements 
				INNER JOIN staffMembers on StaffAchievementId=Achievements.Id
				)
				AND Name IN (SELECT Name FROM StaffMembers)
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

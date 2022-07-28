using SSW.Rewards.Application.Achievements.Queries.Common;
using System.Collections.Generic;
using System.Linq;

namespace SSW.Rewards.Application.Staff.Queries.GetStaffList
{
    public class StaffDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Title { get; set; }
        public string Email { get; set; }
        public string Profile { get; set; }
        public string ProfilePhoto { get; set; }
        public bool IsDeleted { get; set; }
        public string TwitterUsername { get; set; }
        public string GitHubUsername { get; set; }
        public string LinkedInUrl { get; set; }
        public bool IsExternal { get; set; }
        public AchievementDto StaffAchievement { get; set; }
        public bool Scanned { get; set; } = false;
        public IEnumerable<StaffSkillDto> Skills { get; set; } = Enumerable.Empty<StaffSkillDto>();
    }
}

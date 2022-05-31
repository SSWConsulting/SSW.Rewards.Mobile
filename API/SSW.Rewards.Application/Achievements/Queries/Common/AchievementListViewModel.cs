using System.Collections.Generic;

namespace SSW.Rewards.Application.Achievements.Queries.Common
{
    public class AchievementListViewModel
    {
        public IEnumerable<AchievementDto> Achievements { get; set; }
    }
}
using System.Collections.Generic;

namespace SSW.Rewards.Application.Achievements.Queries.GetAchievementList
{
    public class AchievementListViewModel
    {
        public IEnumerable<AchievementViewModel> Achievements { get; set; }
    }
}
using System.Collections.Generic;

namespace SSW.Rewards.Application.Achievement.Queries.GetAchievementList
{
    public class AchievementListViewModel
    {
        public IEnumerable<AchievementViewModel> Achievements { get; set; }
    }
}
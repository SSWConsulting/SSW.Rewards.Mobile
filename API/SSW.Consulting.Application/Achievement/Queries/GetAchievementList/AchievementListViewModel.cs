using System.Collections.Generic;

namespace SSW.Consulting.Application.Achievement.Queries.GetAchievementList
{
    public class AchievementListViewModel
    {
        public IEnumerable<AchievementViewModel> Achievements { get; set; }
    }
}
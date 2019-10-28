using SSW.Consulting.Application.Achievement.Queries.GetAchievementList;
using System;
using System.Collections.Generic;
using System.Text;

namespace SSW.Consulting.Application.Achievement.Command.PostAchievement
{
    public class PostAchievementResult
    {
        public AchievementViewModel viewModel { get; set; }
        public AchievementStatus status { get; set; }
    }
    public enum AchievementStatus
    {
        Added,
        NotFound,
        Duplicate,
        Error
    }
}

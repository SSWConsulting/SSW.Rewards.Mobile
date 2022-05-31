using SSW.Rewards.Application.Achievements.Queries.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace SSW.Rewards.Application.Achievements.Command.PostAchievement
{
    public class PostAchievementResult
    {
        public AchievementDto viewModel { get; set; }
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

using SSW.Consulting.Application.Achievement.Queries.GetAchievementList;
using System;
using System.Collections.Generic;
using System.Text;

namespace SSW.Consulting.Application.Achievement.Command.AddAchievement
{
    public class AddAchievementResult
    {
        public AchievementViewModel ViewModel { get; set; } = new AchievementViewModel();
        public AchievementStatus Status { get; set; }
    }

    public enum AchievementStatus
    {
        Added,
        NotFound,
        Duplicate,
        Error
    }
}

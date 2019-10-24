using SSW.Consulting.Application.Achievement.Queries.GetAchievementList;
using System;
using System.Collections.Generic;
using System.Text;

namespace SSW.Consulting.Application.Achievement.Commands.AddAchievement
{
    public class AddAchievementResult
    {
        public AchievementViewModel viewModel { get; set; }
        public Status status { get; set; }
    }

    public enum Status
    {
        Added,
        NotFound,
        Duplicate,
        Error
    }
}

using SSW.Rewards.Application.Users.Queries.GetUserAchievements;
using SSW.Rewards.Application.Users.Queries.GetUserRewards;
using System.Collections.Generic;

namespace SSW.Rewards.Application.Users.Queries.GetUser
{
    public class UserViewModel
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string ProfilePic { get; set; }
        public int Points { get; set; }
        public int Balance { get; set; }
        public IEnumerable<UserRewardViewModel> Rewards { get; set; }
        public IEnumerable<UserAchievementViewModel> Achievements { get; set; }
    }
}
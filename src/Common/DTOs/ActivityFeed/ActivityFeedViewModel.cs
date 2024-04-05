using SSW.Rewards.Shared.DTOs.Users;

namespace SSW.Rewards.Shared.DTOs.ActivityFeed;

public class ActivityFeedViewModel
{
    public string UserAvatar { get; set; }
    public string UserName { get; set; }
    public string UserTitle { get; set; }
    public string AchievementMessage { get; set; }
    public string TimeElapsed { get; set; }
    public UserAchievementDto Achievement { get; set; }
    public DateTime AwardedAt { get; set; }
}
using SSW.Rewards.Shared.DTOs.Users;

namespace SSW.Rewards.Shared.DTOs.ActivityFeed;

public class ActivityFeedViewModel
{
    public string UserAvatar { get; set; }
    public string UserName { get; set; }
    public UserAchievementDto Achievement { get; set; }
    public DateTime AwardedAt { get; set; }
}
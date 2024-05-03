using SSW.Rewards.Shared.DTOs.Users;

namespace SSW.Rewards.Shared.DTOs.ActivityFeed;

public class ActivityFeedItemDto
{
    public string UserAvatar { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string UserTitle { get; set; } = string.Empty;
    public int UserId { get; set; }
    public string AchievementMessage { get; set; } = string.Empty;
    public string TimeElapsed { get; set; } = string.Empty;
    public UserAchievementDto? Achievement { get; set; }
    public DateTime AwardedAt { get; set; }
}
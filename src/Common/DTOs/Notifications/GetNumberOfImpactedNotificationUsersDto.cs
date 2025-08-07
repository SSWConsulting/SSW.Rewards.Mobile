namespace SSW.Rewards.Shared.DTOs.Notifications;

public class GetNumberOfImpactedNotificationUsersDto
{
    public List<int> AchievementIds { get; set; } = [];

    public List<int> UserIds { get; set; } = [];

    public List<int> RoleIds { get; set; } = [];
}
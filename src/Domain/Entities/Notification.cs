namespace SSW.Rewards.Domain.Entities;
public class Notification : BaseAuditableEntity
{
    public int SentByStaffMemberId { get; set; }
    public User SentByStaffMember { get; set; } = null!;
    public string? Title { get; set; } = string.Empty;
    public string? Message { get; set; }
    public string? NotificationTag { get; set; }
    public string? NotificationAction { get; set; } = string.Empty;

    public DateTimeOffset? Scheduled { get; set; }
    public DateTimeOffset? SentOn { get; set; }
    public bool WasSent { get; set; } = false;
    public int NumberOfUsersTargeted { get; set; }
    public int NumberOfUsersSent { get; set; }
    public bool HasError { get; set; } = false;
    public List<int> FailedUserIds { get; set; } = [];
}

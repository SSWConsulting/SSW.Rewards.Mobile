namespace SSW.Rewards.Shared.DTOs.Notifications;

public class NotificationHistoryDto
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Message { get; set; }
    public string Tags { get; set; }
    public DateTime CreatedDateUtc { get; set; }
    public DateTime? ScheduledDate { get; set; }
    public NotificationStatus Status { get; set; }
    public string EmailAddress { get; set; }
    public bool WasSent { get; set; }
    public bool HasError { get; set; }
    public DateTime? SentOn { get; set; }
    public int NumberOfUsersTargeted { get; set; }
    public int NumberOfUsersSent { get; set; }
}

public enum NotificationStatus
{
    NotSent = 0,
    Sent = 1,
    Failed = 2,
    Scheduled = 3
}

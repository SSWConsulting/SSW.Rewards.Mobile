using Microsoft.AspNetCore.Components;

namespace SSW.Rewards.Shared.DTOs.Notifications;

public class NotificationHistoryDto
{
    public string Title { get; set; }
    public string Message { get; set; }
    public string Tags { get; set; }
    public DateTime CreatedDateUtc { get; set; }
    public DateTime? ScheduledDate { get; set; }
    public string EmailAddress { get; set; }
    public bool WasSent { get; set; }
    public bool HasError { get; set; }
    public DateTime? SentOn { get; set; }
    public int NumberOfUsersTargeted { get; set; }
    public int NumberOfUsersSent { get; set; }
}

namespace SSW.Rewards.Shared.DTOs.Notifications;

public class NotificationHistoryDto
{
    public string Message { get; set; }
    public DateTime CreatedDate { get; set; }
    public string EmailAddress { get; set; }
    public string Title { get; set; }
    public bool WasSent { get; set; }
    public bool HasError { get; set; }
    public DateTime? SentOn { get; set; }
    public int NumberOfUsersTargeted { get; set; }
    public int NumberOfUsersSent { get; set; }
}

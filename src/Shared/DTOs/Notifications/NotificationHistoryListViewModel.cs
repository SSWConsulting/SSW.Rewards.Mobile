using SSW.Rewards.Shared.DTOs.Pagination;

namespace SSW.Rewards.Shared.DTOs.Notifications;

public class NotificationHistoryListViewModel : PaginationResult<NotificationHistoryDto>
{
    // You can add additional properties if needed
}

public class NotificationHistoryDto
{
    public string? Title { get; set; }
    public string? Message { get; set; }
    public string? NotificationTag { get; set; }
    public string? NotificationAction { get; set; }
    public DateTimeOffset? Scheduled { get; set; }
    public DateTimeOffset? SentOn { get; set; }
    public bool WasSent { get; set; }
    public int NumberOfUsersTargeted { get; set; }
    public int NumberOfUsersSent { get; set; }
    public bool HasError { get; set; }
    public string EmailAddress { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
}

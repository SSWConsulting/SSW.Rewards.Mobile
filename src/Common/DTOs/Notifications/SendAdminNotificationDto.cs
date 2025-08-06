using System.ComponentModel.DataAnnotations;

namespace SSW.Rewards.Shared.DTOs.Notifications;

public class SendAdminNotificationDto : GetNumberOfImpactedNotificationUsersDto
{
    public DateTimeOffset? ScheduleAt { get; set; }

    [Required]
    [MaxLength(100)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MaxLength(250)]
    public string Body { get; set; } = string.Empty;

    [Url]
    public string? ImageUrl { get; set; }
}

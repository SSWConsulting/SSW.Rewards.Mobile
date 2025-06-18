using System.ComponentModel.DataAnnotations;
using SSW.Rewards.Shared.DTOs.Achievements;
using SSW.Rewards.Shared.DTOs.Roles;

namespace SSW.Rewards.Admin.UI.Models;

public enum Delivery { Now, Schedule }

public class NotificationViewModel
{
    public Delivery DeliveryOption { get; set; } = Delivery.Now;

    public DateTime? ScheduleDate { get; set; } = DateTime.Today;
    public TimeSpan? ScheduleTime { get; set; } = DateTime.Now.TimeOfDay;
    public string? SelectedTimeZone { get; set; }

    public AchievementDto? SelectedAchievement { get; set; }
    public RoleDto? SelectedRole { get; set; }

    [Required(ErrorMessage = "Notification title is required.")]
    [MaxLength(100, ErrorMessage = "Title cannot exceed 100 characters.")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Notification body is required.")]
    [MaxLength(250, ErrorMessage = "Body cannot exceed 250 characters.")]
    public string Body { get; set; } = string.Empty;

    [Url(ErrorMessage = "Please enter a valid URL for the image.")]
    public string? ImageUrl { get; set; }
}

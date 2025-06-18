using System.ComponentModel.DataAnnotations;
using MudBlazor;
using SSW.Rewards.Shared.DTOs.Achievements;
using SSW.Rewards.Shared.DTOs.Notifications;

namespace SSW.Rewards.Admin.UI.Pages;

public partial class Notifications
{
    private NotificationViewModel _model = new();
    private Dictionary<string, string> _timezones = [];
    private bool _isSending = false;

    public enum Delivery { Now, Schedule }

    public class NotificationViewModel
    {
        public Delivery DeliveryOption { get; set; } = Delivery.Now;

        public DateTime? ScheduleDate { get; set; } = DateTime.Today;
        public TimeSpan? ScheduleTime { get; set; } = DateTime.Now.TimeOfDay;
        public string? SelectedTimeZone { get; set; }

        public AchievementDto? SelectedAchievement { get; set; }
        public string? SelectedRole { get; set; }

        [Required(ErrorMessage = "Notification title is required.")]
        [MaxLength(100, ErrorMessage = "Title cannot exceed 100 characters.")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Notification body is required.")]
        [MaxLength(250, ErrorMessage = "Body cannot exceed 250 characters.")]
        public string Body { get; set; } = string.Empty;

        [Url(ErrorMessage = "Please enter a valid URL for the image.")]
        public string? ImageUrl { get; set; }
    }

    protected override async Task OnInitializedAsync()
    {
        LoadTimezones();
        _model.SelectedTimeZone = TimeZoneInfo.Local.Id;
    }

    private void LoadTimezones()
    {
        _timezones = TimeZoneInfo.GetSystemTimeZones().ToDictionary(x => x.Id, x => x.DisplayName);
    }

    private async Task HandleValidSubmit()
    {
        _isSending = true;
        StateHasChanged();

        var command = new SendAdminNotificationDto
        {
            Title = _model.Title,
            Body = _model.Body,
            ImageUrl = _model.ImageUrl,
            //Achievements = _model.SelectedAchievement?.Id,
            //UserId = _model.SelectedUser?.Id,
            //Role = _model.SelectedRole
        };

        if (_model.SelectedAchievement is not null)
        {
            command.AchievementIds.Add(_model.SelectedAchievement.Id);
        }

        if (_model.DeliveryOption == Delivery.Schedule)
        {
            if (!_model.ScheduleDate.HasValue || !_model.ScheduleTime.HasValue || string.IsNullOrWhiteSpace(_model.SelectedTimeZone))
            {
                Snackbar.Add("Please select a valid date, time, and timezone for scheduled notifications.", Severity.Error);
                _isSending = false;
                StateHasChanged();
                return;
            }

            DateTime combinedDateTime = _model.ScheduleDate.Value.Date + _model.ScheduleTime.Value;
            TimeZoneInfo selectedTzi = TimeZoneInfo.FindSystemTimeZoneById(_model.SelectedTimeZone);
            command.ScheduleAt = new DateTimeOffset(combinedDateTime, selectedTzi.GetUtcOffset(combinedDateTime));
        }

        try
        {
            // Send notification through the API client service
            //await NotificationsService.SendAdminNotification(command, CancellationToken.None);

            if (_model.DeliveryOption == Delivery.Schedule)
            {
                Snackbar.Add($"Notification SCHEDULED for: {command.ScheduleAt:g} ({_model.SelectedTimeZone}). Title: '{_model.Title}'", Severity.Success, config => { config.VisibleStateDuration = 10000; });
            }
            else
            {
                Snackbar.Add($"Notification SENT. Title: '{_model.Title}'", Severity.Success, config => { config.VisibleStateDuration = 10000; });
            }

            _model = new NotificationViewModel { SelectedTimeZone = TimeZoneInfo.Local.Id }; // Reset form
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Error sending notification: {ex.Message}", Severity.Error);
        }
        finally
        {
            _isSending = false;
            StateHasChanged();
        }
    }

    private async Task<IEnumerable<AchievementDto>> SearchAchievements(string value, CancellationToken cancellationToken)
    {
        try
        {
            var achievements = await AchievementService.SearchAchievements(value, cancellationToken);
            return achievements.Achievements;
        }
        catch (Exception)
        {
            //ShowError("Failed to search achievements");
            return [];
        }
    }

    private static string GetAchievementName(AchievementDto? achievement) => achievement?.Name ?? string.Empty;
}

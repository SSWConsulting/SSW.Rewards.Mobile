using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;
using SSW.Rewards.Admin.UI.Models;
using SSW.Rewards.ApiClient.Services;
using SSW.Rewards.Shared.DTOs.Achievements;
using SSW.Rewards.Shared.DTOs.Notifications;
using SSW.Rewards.Shared.DTOs.Roles;
using SSW.Rewards.Admin.UI.Components.Dialogs.Confirmations;
using SSW.Rewards.Admin.UI.Helpers;

namespace SSW.Rewards.Admin.UI.Pages;

public partial class SendNotification
{
    [Inject] private IRoleService RoleService { get; set; } = default!;
    [Inject] private IAchievementService AchievementService { get; set; } = default!;
    [Inject] private INotificationsService NotificationsService { get; set; } = default!;
    [Inject] private ISnackbar Snackbar { get; set; } = default!;
    [Inject] private IDialogService DialogService { get; set; } = default!;
    [Inject] private NavigationManager NavigationManager { get; set; } = default!;

    private NotificationViewModel _model = new();
    private Dictionary<string, string> _timezones = [];
    private bool _isSending = false;
    private List<RoleDto>? _roles;

    protected override async Task OnInitializedAsync()
    {
        LoadTimezones();
        _model.SelectedTimeZone = TimeZoneInfo.Local.Id;

        await LoadRoles();
    }

    private async Task LoadRoles()
    {
        try
        {
            var roles = await RoleService.GetRoles();
            _roles = roles.ToList();
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Failed to load roles: {ex.Message}", Severity.Error, options => { });
            _roles = [];
        }
    }

    private void LoadTimezones()
    {
        _timezones = TimeZoneInfo.GetSystemTimeZones().ToDictionary(x => x.Id, x => x.DisplayName);
    }

    private Task<IEnumerable<RoleDto>> SearchRoles(string value, CancellationToken cancellationToken)
        => Task.FromResult(_roles != null && !string.IsNullOrWhiteSpace(value)
            ? _roles.Where(r => r.Name.Contains(value, StringComparison.OrdinalIgnoreCase))
            : _roles ?? []);

    private async Task HandleValidSubmit()
    {
        _isSending = true;
        StateHasChanged();

        try
        {
            SendAdminNotificationDto command = new()
            {
                Title = _model.Title,
                Body = _model.Body,
                ImageUrl = _model.ImageUrl
            };

            string targetGroup;
            switch (_model.TargetingOption)
            {
                case Targeting.Achievement when _model.SelectedAchievement is not null:
                    command.AchievementIds = [_model.SelectedAchievement.Id];
                    targetGroup = $"users with the '{_model.SelectedAchievement.Name}' achievement";
                    break;
                case Targeting.Role when _model.SelectedRole is not null:
                    command.RoleIds = [_model.SelectedRole.Id];
                    targetGroup = $"users with the '{_model.SelectedRole.Name}' role";
                    break;
                default:
                    targetGroup = "users";
                    break;
            }

            var impactedUsers = await NotificationsService.GetNumberOfImpactedUsers(command, CancellationToken.None);
            var deliveryTime = _model.DeliveryOption == Delivery.Schedule && _model.ScheduleDate.HasValue && _model.ScheduleTime.HasValue
                ? $"at {DateTimeFormatter.FormatLongDate(DateTime.SpecifyKind((_model.ScheduleDate.Value.Date + _model.ScheduleTime.Value), DateTimeKind.Utc))} ({_model.SelectedTimeZone})"
                : "immediately";

            RenderFragment confirmationContent = builder =>
            {
                builder.OpenElement(0, "div");
                builder.AddMarkupContent(1, $"<p><strong>Title:</strong> {_model.Title}</p>");
                builder.AddMarkupContent(2, $"<p><strong>Target:</strong> {impactedUsers} {targetGroup}</p>");
                builder.AddMarkupContent(3, $"<p><strong>Delivery:</strong> {deliveryTime}</p>");
                builder.AddMarkupContent(4, "<br>");
                builder.AddMarkupContent(5, "<p>Are you sure you want to continue?</p>");
                builder.CloseElement();
            };

            string confirmationText = $"{(_model.DeliveryOption == Delivery.Now ? "Send" : "Schedule")} Notification";
            var parameters = new DialogParameters
            {
                { "Content", confirmationContent },
                { "ButtonText", confirmationText },
                { "CancelText", "Cancel" },
                { "Color", Color.Primary }
            };

            var dialog = await DialogService.ShowAsync<ConfirmationDialog>($"Create notification for {impactedUsers} users", parameters);
            var result = await dialog.Result;

            if (result.Canceled || !(result.Data is bool confirmed && confirmed))
            {
                _isSending = false;
                StateHasChanged();
                return;
            }

            if (_model.DeliveryOption == Delivery.Schedule)
            {
                if (!_model.ScheduleDate.HasValue || !_model.ScheduleTime.HasValue || string.IsNullOrWhiteSpace(_model.SelectedTimeZone))
                {
                    Snackbar.Add("Please select a valid date, time, and timezone for scheduled notifications.", Severity.Error, options => { });
                    _isSending = false;
                    StateHasChanged();
                    return;
                }

                // Combine date and time as Unspecified kind
                DateTime combinedDateTime = DateTime.SpecifyKind(_model.ScheduleDate.Value.Date + _model.ScheduleTime.Value, DateTimeKind.Unspecified);
                TimeZoneInfo selectedTzi = TimeZoneInfo.FindSystemTimeZoneById(_model.SelectedTimeZone);
                // Convert to UTC
                DateTime utcDateTime = TimeZoneInfo.ConvertTimeToUtc(combinedDateTime, selectedTzi);
                command.ScheduleAt = new DateTimeOffset(utcDateTime, TimeSpan.Zero);
            }

            // Send notification through the API client service
            await NotificationsService.SendAdminNotification(command, CancellationToken.None);

            if (_model.DeliveryOption == Delivery.Schedule)
            {
                Snackbar.Add($"Notification scheduled for: {command.ScheduleAt:g} ({_model.SelectedTimeZone}). Title: '{_model.Title}'", Severity.Success, options => { options.VisibleStateDuration = 10000; });
            }
            else
            {
                Snackbar.Add($"Notification sent. Title: '{_model.Title}'", Severity.Success, options => { options.VisibleStateDuration = 10000; });
            }

            _model = new NotificationViewModel { SelectedTimeZone = TimeZoneInfo.Local.Id };

            // Navigate to Notifications page after sending
            NavigationManager.NavigateTo("/notifications");
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Error sending notification: {ex.Message}", Severity.Error, options => { });
        }
        finally
        {
            _isSending = false;
            StateHasChanged();
        }
    }

    private async Task<IEnumerable<AchievementDto>> SearchAchievements(string value)
    {
        try
        {
            var achievements = await AchievementService.SearchAchievements(value, CancellationToken.None);
            return achievements.Achievements;
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Error searching achievements: {ex.Message}", Severity.Error, options => { });
            return [];
        }
    }

    private static string GetAchievementName(AchievementDto? achievement)
        => achievement?.Name ?? string.Empty;

    private void OnDateChanged(DateTime? date)
    {
        _model.ScheduleDate = date;
        StateHasChanged();
    }

    private void OnTimeChanged(TimeSpan? time)
    {
        _model.ScheduleTime = time;
        StateHasChanged();
    }
}

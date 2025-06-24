using Microsoft.AspNetCore.Components;
using MudBlazor;
using SSW.Rewards.Admin.UI.Models;
using SSW.Rewards.ApiClient.Services;
using SSW.Rewards.Shared.DTOs.Achievements;
using SSW.Rewards.Shared.DTOs.Notifications;
using SSW.Rewards.Shared.DTOs.Roles;
using SSW.Rewards.Admin.UI.Components.Dialogs.Confirmations;

namespace SSW.Rewards.Admin.UI.Pages;

public partial class SendNotification
{
    [Inject] private IRoleService RoleService { get; set; } = default!;
    [Inject] private IAchievementService AchievementService { get; set; } = default!;
    [Inject] private INotificationsService NotificationsService { get; set; } = default!;
    [Inject] private ISnackbar Snackbar { get; set; } = default!;
    [Inject] private IDialogService DialogService { get; set; } = default!;

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

    private async Task<IEnumerable<RoleDto>> SearchRoles(string value)
        => _roles != null && !string.IsNullOrWhiteSpace(value)
            ? _roles.Where(r => r.Name.Contains(value, StringComparison.OrdinalIgnoreCase))
            : _roles ?? [];

    private async Task HandleValidSubmit()
    {
        // Check if no role, achievement, or user is selected
        bool isTargetingEveryone = _model.SelectedAchievement is null && _model.SelectedRole is null;

        if (isTargetingEveryone)
        {
            var parameters = new DialogParameters
            {
                { "ContentText", "No role, achievement, or user is selected. This will send the notification to EVERYONE. Are you sure you want to continue?" },
                { "ButtonText", "Send" },
                { "Color", Color.Primary }
            };
            var dialog = DialogService.Show<SimpleConfirmationDialog>("Send to Everyone?", parameters);
            var result = await dialog.Result;
            if (result.Canceled || !(result.Data is bool confirmed && confirmed))
            {
                // User cancelled, do not send or clear
                return;
            }
        }

        _isSending = true;
        StateHasChanged();

        SendAdminNotificationDto command = new()
        {
            Title = _model.Title,
            Body = _model.Body,
            ImageUrl = _model.ImageUrl,
        };

        if (_model.SelectedAchievement is not null)
        {
            command.AchievementIds.Add(_model.SelectedAchievement.Id);
        }

        if (_model.SelectedRole is not null)
        {
            command.RoleIds.Add(_model.SelectedRole.Id);
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

        try
        {
            // Send notification through the API client service
            await NotificationsService.SendAdminNotification(command, CancellationToken.None);

            if (_model.DeliveryOption == Delivery.Schedule)
            {
                Snackbar.Add($"Notification SCHEDULED for: {command.ScheduleAt:g} ({_model.SelectedTimeZone}). Title: '{_model.Title}'", Severity.Success, options => { options.VisibleStateDuration = 10000; });
            }
            else
            {
                Snackbar.Add($"Notification SENT. Title: '{_model.Title}'", Severity.Success, options => { options.VisibleStateDuration = 10000; });
            }

            _model = new NotificationViewModel { SelectedTimeZone = TimeZoneInfo.Local.Id };
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

    private async Task<IEnumerable<AchievementDto>> SearchAchievements(string value, CancellationToken cancellationToken)
    {
        try
        {
            var achievements = await AchievementService.SearchAchievements(value, cancellationToken);
            return achievements.Achievements;
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Error searching achievements: {ex.Message}", Severity.Error, options => { });
            return [];
        }
    }

    private static string GetAchievementName(AchievementDto? achievement) => achievement?.Name ?? string.Empty;
}

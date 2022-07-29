using Microsoft.Azure.NotificationHubs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SSW.Rewards.Application.Common.Interfaces;
using SSW.Rewards.Application.Common.Models;

namespace SSW.Rewards.Infrastructure.Services;
public class NotificationsService : INotificationService
{
    readonly NotificationHubClient _hub;
    readonly Dictionary<string, NotificationPlatform> _installationPlatform;
    readonly ILogger<NotificationsService> _logger;

    public NotificationsService(IOptions<NotificationHubOptions> options, ILogger<NotificationsService> logger)
    {
        _logger = logger;
        _hub = NotificationHubClient.CreateClientFromConnectionString(
            options.Value.ConnectionString,
            options.Value.Name);

        _installationPlatform = new Dictionary<string, NotificationPlatform>
            {
                { nameof(NotificationPlatform.Apns).ToLower(), NotificationPlatform.Apns },
                { nameof(NotificationPlatform.Fcm).ToLower(), NotificationPlatform.Fcm }
            };
    }

    public async Task<bool> CreateOrUpdateInstallationAsync(DeviceInstall deviceInstallation, CancellationToken token)
    {
        if (string.IsNullOrWhiteSpace(deviceInstallation?.InstallationId) ||
            string.IsNullOrWhiteSpace(deviceInstallation?.Platform) ||
            string.IsNullOrWhiteSpace(deviceInstallation?.PushChannel))
            return false;

        var installation = new Installation()
        {
            InstallationId = deviceInstallation.InstallationId,
            PushChannel = deviceInstallation.PushChannel,
            Tags = deviceInstallation.Tags
        };

        try
        {
            if (!_installationPlatform.TryGetValue(deviceInstallation.Platform, out var platform))
            {
                return false;
            }
            installation.Platform = platform;

            await _hub.CreateOrUpdateInstallationAsync(installation, token);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine("\nStackTrace ---\n{0}", ex.StackTrace);
        }

        return false;
    }

    public async Task<bool> DeleteInstallationByIdAsync(string installationId, CancellationToken token)
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(installationId))
            {
                await _hub.DeleteInstallationAsync(installationId, token);
                return true;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("\nStackTrace ---\n{0}", ex.StackTrace);
        }

        return false;
    }

    public async Task<bool> RequestNotificationAsync(NotificationRequest notificationRequest, CancellationToken token)
    {
        if ((notificationRequest.Silent && string.IsNullOrWhiteSpace(notificationRequest?.Action)) ||
            (!notificationRequest.Silent && (string.IsNullOrWhiteSpace(notificationRequest?.Text)) ||
            string.IsNullOrWhiteSpace(notificationRequest?.Action)))
            return false;
        var androidPushTemplate = Convert.ToBoolean(notificationRequest.Silent) ?
            PushTemplates.Silent.Android :
            PushTemplates.Generic.Android;

        var iOSPushTemplate = Convert.ToBoolean(notificationRequest.Silent) ?
            PushTemplates.Silent.iOS :
            PushTemplates.Generic.iOS;

        var androidPayload = PrepareNotificationPayload(
            androidPushTemplate,
            notificationRequest.Text,
            notificationRequest.Action);

        var iOSPayload = PrepareNotificationPayload(
            iOSPushTemplate,
            notificationRequest.Text,
            notificationRequest.Action);

        try
        {
            if (notificationRequest.Tags.Length == 0)
            {
                // This will broadcast to all users registered in the notification hub
                await SendPlatformNotificationsAsync(androidPayload, iOSPayload, token);
            }
            else if (notificationRequest.Tags.Length <= 20)
            {
                await SendPlatformNotificationsAsync(androidPayload, iOSPayload, notificationRequest.Tags, token);
            }
            else
            {
                var notificationTasks = notificationRequest.Tags
                    .Select((value, index) => (value, index))
                    .GroupBy(g => g.index / 20, i => i.value)
                    .Select(tags => SendPlatformNotificationsAsync(androidPayload, iOSPayload, tags, token));

                await Task.WhenAll(notificationTasks);
            }

            return true;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Unexpected error sending notification");
            return false;
        }
    }

    string PrepareNotificationPayload(string template, string text, string action) => template
        .Replace("$(alertMessage)", text, StringComparison.InvariantCulture)
        .Replace("$(alertAction)", action, StringComparison.InvariantCulture);

    Task SendPlatformNotificationsAsync(string androidPayload, string iOSPayload, CancellationToken token)
    {
        var sendTasks = new Task[]
        {
                _hub.SendFcmNativeNotificationAsync(androidPayload, token),
                _hub.SendAppleNativeNotificationAsync(iOSPayload, token)
        };

        return Task.WhenAll(sendTasks);
    }

    /// <summary>The tag expression provided to SendTemplateNotificationAsync is limited to 20 tags.</summary>
    /// <remarks>
    /// If there are more than 20 tags in the request then they must be split into multiple requests.
    /// </remarks>
    Task SendPlatformNotificationsAsync(string androidPayload, string iOSPayload, IEnumerable<string> tags, CancellationToken token)
    {
        var sendTasks = new Task[]
        {
                _hub.SendFcmNativeNotificationAsync(androidPayload, tags, token),
                _hub.SendAppleNativeNotificationAsync(iOSPayload, tags, token)
        };

        return Task.WhenAll(sendTasks);
    }
}

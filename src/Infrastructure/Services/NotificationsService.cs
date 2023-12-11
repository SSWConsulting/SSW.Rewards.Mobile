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

    public async Task CreateOrUpdateInstallationAsync(DeviceInstall deviceInstallation, CancellationToken token)
    {
        var installation = new Installation()
        {
            InstallationId = deviceInstallation.InstallationId,
            PushChannel = deviceInstallation.PushChannel,
            Tags = deviceInstallation.Tags
        };

        _installationPlatform.TryGetValue(deviceInstallation.Platform, out var platform);

        installation.Platform = platform;

        await _hub.CreateOrUpdateInstallationAsync(installation, token);
    }

    public async Task DeleteInstallationByIdAsync(string installationId, CancellationToken token)
    {
        await _hub.DeleteInstallationAsync(installationId, token);
    }

    public async Task RequestNotificationAsync(NotificationRequest notificationRequest, CancellationToken token)
    {
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

        if (notificationRequest.Tags.Length == 0)
        {
            // This will broadcast to all users registered in the notification hub
            await SendPlatformNotificationsAsync(androidPayload, iOSPayload, token);
        }
        else
        {
            var chunkSize = 20;

            foreach (var chunk in notificationRequest.Tags.Chunk(chunkSize))
            {
                await SendPlatformNotificationsAsync(androidPayload, iOSPayload, chunk, token);
            }
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

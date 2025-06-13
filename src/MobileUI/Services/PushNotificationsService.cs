using Microsoft.Extensions.DependencyInjection.Notifications.Commands.UploadDeviceToken;
using Microsoft.Extensions.Logging;
using SSW.Rewards.ApiClient.Services;

namespace SSW.Rewards.Mobile.Services;

public interface IPushNotificationsService
{
    public Task<bool> UploadDeviceToken(string token, DateTime lastTimeUpdated, string deviceId);
}

public class PushNotificationsService : IPushNotificationsService
{
    private readonly INotificationsService _notificationsService;
    private readonly ILogger<PushNotificationsService> _logger;

    public PushNotificationsService(INotificationsService notificationsService, ILogger<PushNotificationsService> logger)
    {
        _notificationsService = notificationsService;
        _logger = logger;
    }

    public async Task<bool> UploadDeviceToken(string token, DateTime lastTimeUpdated, string deviceId)
    {
        try
        {
            var dto = new DeviceTokenDto { DeviceToken = token, LastTimeUpdated = lastTimeUpdated, DeviceId = deviceId };
            await _notificationsService.UploadDeviceToken(dto, CancellationToken.None);
            return true;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Couldn't upload device token at {LastTimeUpdated}. Error: {Exception}", lastTimeUpdated, e);
            return false;
        }
    }
}
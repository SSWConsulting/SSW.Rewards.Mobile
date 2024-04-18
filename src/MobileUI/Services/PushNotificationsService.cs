using Microsoft.AppCenter.Crashes;
using Microsoft.Extensions.DependencyInjection.Notifications.Commands.UploadDeviceToken;
using SSW.Rewards.ApiClient.Services;

namespace SSW.Rewards.Mobile.Services;

public interface IPushNotificationsService
{
    public Task<bool> UploadDeviceToken(string token, DateTime lastTimeUpdated, string deviceId);
}

public class PushNotificationsService : IPushNotificationsService
{
    private readonly INotificationsService _notificationsService;

    public PushNotificationsService(INotificationsService notificationsService)
    {
        _notificationsService = notificationsService;
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
            Crashes.TrackError(new Exception($"Couldn't upload device token at {lastTimeUpdated}. Error: {e}"));
            return false;
        }
    }
}
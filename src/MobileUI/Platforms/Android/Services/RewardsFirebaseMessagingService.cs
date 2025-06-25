using Android.App;
using Firebase.Messaging;
using Microsoft.Extensions.Logging;
using SSW.Rewards.Mobile.Services.Authentication;

namespace SSW.Rewards.Mobile.Platforms.Android;

[Service(Exported = false)]
[IntentFilter(["com.google.firebase.MESSAGING_EVENT"])]
public class RewardsFirebaseMessagingService : FirebaseMessagingService
{
    private IServiceProvider? _serviceProvider;
    private ILogger<RewardsFirebaseMessagingService>? _logger;

    private IServiceProvider ServiceProvider => 
        _serviceProvider ??= IPlatformApplication.Current?.Services;

    private ILogger<RewardsFirebaseMessagingService> Logger =>
        _logger ??= ServiceProvider?.GetService<ILogger<RewardsFirebaseMessagingService>>();

    public override async void OnNewToken(string token)
    {
        base.OnNewToken(token);

        try
        {
            var authStorageService = ServiceProvider?.GetService<IAuthStorageService>();
            
            if (authStorageService == null)
            {
                Logger?.LogWarning("IAuthStorageService not available, unable to store device token");
                return;
            }

            await authStorageService.StoreDeviceTokenAsync(token);
            authStorageService.SetDeviceTokenLastUpdated(DateTime.MinValue);
            
            Logger?.LogInformation("Device token updated successfully");
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "Failed to store device token: {Token}", token);
        }
    }
}
using Firebase.CloudMessaging;
using Foundation;
using Microsoft.Extensions.Logging;
using UIKit;
using UserNotifications;
using SSW.Rewards.Mobile.Services.Authentication;

namespace SSW.Rewards.Mobile;

[Register("AppDelegate")]
public class AppDelegate : MauiUIApplicationDelegate, IUNUserNotificationCenterDelegate, IMessagingDelegate
{
    private IAuthStorageService _authStorageService;
    private ILogger<AppDelegate> _logger;

    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

    public override bool FinishedLaunching(UIApplication app, NSDictionary options)
    {
        var serviceProvider = IPlatformApplication.Current?.Services;
        _authStorageService = serviceProvider?.GetService<IAuthStorageService>();
        _logger = serviceProvider?.GetService<ILogger<AppDelegate>>();

        ConfigureFirebaseMessaging();
        ConfigurePushNotifications();
        
        return base.FinishedLaunching(app, options);
    }

    private void ConfigureFirebaseMessaging()
    {
        Messaging.SharedInstance.Delegate = this;
        Messaging.SharedInstance.AutoInitEnabled = true;
    }

    private void ConfigurePushNotifications()
    {
        var authOptions = UNAuthorizationOptions.Alert | UNAuthorizationOptions.Badge | UNAuthorizationOptions.Sound;
        
        UNUserNotificationCenter.Current.RequestAuthorization(authOptions, (granted, error) =>
        {
            if (error is not null)
            {
                _logger?.LogError("Failed to request notification authorization: {Error}", error.LocalizedDescription);
            }
            else if (granted)
            {
                _logger?.LogInformation("Push notification authorization granted");
            }
            else
            {
                _logger?.LogInformation("Push notification authorization denied");
            }
        });

        UNUserNotificationCenter.Current.Delegate = this;
        UIApplication.SharedApplication.RegisterForRemoteNotifications();
    }
    
    [Export("messaging:didReceiveRegistrationToken:")]
    public async void DidReceiveRegistrationToken(Messaging message, string token)
    {
        try
        {
            if (_authStorageService is null)
            {
                _logger?.LogError("AuthStorageService is not available for storing device token");
                return;
            }

            await _authStorageService.StoreDeviceTokenAsync(token);
            _authStorageService.SetDeviceTokenLastUpdated(DateTime.MinValue);
            
            _logger?.LogInformation("Firebase registration token received and stored");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to store Firebase registration token");
        }
    }
}
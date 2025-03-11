using Firebase.CloudMessaging;
using Foundation;
using UIKit;
using UserNotifications;

namespace SSW.Rewards.Mobile;

[Register("AppDelegate")]
public class AppDelegate : MauiUIApplicationDelegate, IUNUserNotificationCenterDelegate, IMessagingDelegate
{
	protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

    public override bool FinishedLaunching(UIApplication app, NSDictionary options)
    {
        Messaging.SharedInstance.Delegate = this;
        Messaging.SharedInstance.AutoInitEnabled = true;

        var authOptions = UNAuthorizationOptions.Alert | UNAuthorizationOptions.Badge | UNAuthorizationOptions.Sound;
        UNUserNotificationCenter.Current.RequestAuthorization(authOptions, (granted, error) =>
        {
            if (error is not null)
            {
                Console.WriteLine("Received Remote Notification");
            }
        });

        UNUserNotificationCenter.Current.Delegate = this;
        
        UIApplication.SharedApplication.RegisterForRemoteNotifications();
        
        return base.FinishedLaunching(app, options);
    }
    
    [Export("messaging:didReceiveRegistrationToken:")]
    public void DidReceiveRegistrationToken(Messaging message, string token)
    {
        SecureStorage.SetAsync("DeviceToken", token);
        Preferences.Set("DeviceTokenLastTimeUpdated", DateTime.MinValue);
        Console.WriteLine ($"Firebase registration token: {token}");
    }
}

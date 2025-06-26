using Android.App;
using Android.Content.PM;
using Android.OS;
using Plugin.Firebase.CloudMessaging;

namespace SSW.Rewards.Mobile;

[IntentFilter(
    [Android.Content.Intent.ActionView],
    AutoVerify = true,
    Categories =
    [
        Android.Content.Intent.CategoryDefault,
        Android.Content.Intent.CategoryBrowsable
    ],
    DataScheme = "https",
    DataHost = "rewards.ssw.com.au",
    DataPath = "/redeem"
)]
[IntentFilter(
    [Android.Content.Intent.ActionView],
    Categories =
    [
        Android.Content.Intent.CategoryDefault,
        Android.Content.Intent.CategoryBrowsable
    ],
    DataScheme = "sswrewards"
)]
[Activity(Theme = "@style/MyTheme.Splash", MainLauncher = true,
    ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode |
                           ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density,
    ScreenOrientation = ScreenOrientation.Portrait, LaunchMode = LaunchMode.SingleTop)]
public class MainActivity : MauiAppCompatActivity
{
    internal static readonly string ChannelId = "General";
    internal static readonly int NotificationId = 101;

    protected override void OnCreate(Bundle savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        CreateNotificationChannel();
    }

    private void CreateNotificationChannel()
    {
        if (!OperatingSystem.IsOSPlatformVersionAtLeast("android", 26))
        {
            return;
        }

        var channel = new NotificationChannel(ChannelId, "General", NotificationImportance.Default);

        var notificationManager = (NotificationManager)GetSystemService(NotificationService);
        notificationManager?.CreateNotificationChannel(channel);
        FirebaseCloudMessagingImplementation.ChannelId = ChannelId;
    }
}

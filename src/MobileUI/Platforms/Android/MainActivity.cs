using Android.App;
using Android.Content.PM;
using Android.OS;
using Microsoft.Maui.Controls.Compatibility.Platform.Android;
using Plugin.Firebase.CloudMessaging;
using Color = Microsoft.Maui.Graphics.Color;

namespace SSW.Rewards.Mobile;

[IntentFilter(
    [Android.Content.Intent.ActionView],
    AutoVerify = true,
    Categories = new[]
    {
        Android.Content.Intent.CategoryDefault,
        Android.Content.Intent.CategoryBrowsable
    },
    DataScheme = "https",
    DataHost = "rewards.ssw.com.au"
)]
[IntentFilter(
    [Android.Content.Intent.ActionView],
    Categories = new[]
    {
        Android.Content.Intent.CategoryDefault,
        Android.Content.Intent.CategoryBrowsable
    },
    DataScheme = "sswrewards"
)]
[Activity(Theme = "@style/MyTheme.Splash", MainLauncher = true,
    ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode |
                           ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density,
    ScreenOrientation = ScreenOrientation.Portrait, LaunchMode = LaunchMode.SingleTop)]
public class MainActivity : MauiAppCompatActivity
{
    internal static readonly string Channel_ID = "General";
    internal static readonly int NotificationID = 101;

    protected async override void OnCreate(Bundle savedInstanceState)
    {
        var MainBackground = Color.FromArgb("#181818").ToAndroid();
        Window!.SetNavigationBarColor(MainBackground);
        base.OnCreate(savedInstanceState);
        CreateNotificationChannel();
    }

    private void CreateNotificationChannel()
    {
        if (OperatingSystem.IsOSPlatformVersionAtLeast("android", 26))
        {
            var channel = new NotificationChannel(Channel_ID, "General", NotificationImportance.Default);

            var notificationManager = (NotificationManager)GetSystemService(Android.Content.Context.NotificationService);
            notificationManager.CreateNotificationChannel(channel);
            FirebaseCloudMessagingImplementation.ChannelId = Channel_ID;
        }
    }
}

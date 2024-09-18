using Android.App;
using Android.Content.PM;
using Android.Gms.Extensions;
using Android.OS;
using Firebase.Messaging;
using Microsoft.Maui.Controls.Compatibility.Platform.Android;
using Microsoft.Maui.Controls.PlatformConfiguration;
using SSW.Rewards.Mobile.Platforms.Android;
using Color = Microsoft.Maui.Graphics.Color;

namespace SSW.Rewards.Mobile;

[IntentFilter([Android.Content.Intent.ActionView],
    Categories =
    [
        Android.Content.Intent.ActionView,
        Android.Content.Intent.CategoryDefault,
        Android.Content.Intent.CategoryBrowsable
    ],
    DataScheme = "sswrewards", DataHost = "", DataPathPrefix = "/")]
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
        }
    }
}

using Android.App;
using Android.Content.PM;
using Android.OS;
using Microsoft.Maui.Controls.Compatibility.Platform.Android;
using Color = Microsoft.Maui.Graphics.Color;

namespace SSW.Rewards.Mobile;

[Activity(Theme = "@style/MyTheme.Splash", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    protected override void OnCreate(Bundle savedInstanceState)
    {
        var MainBackground = Color.FromArgb("#FF121212").ToAndroid();
        Window!.SetNavigationBarColor(MainBackground);
        base.OnCreate(savedInstanceState);
    }
}

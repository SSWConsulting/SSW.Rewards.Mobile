using CommunityToolkit.Maui.Behaviors;
using CommunityToolkit.Maui.Core;
using Mopups.Services;

namespace SSW.Rewards.PopupPages;

public partial class AboutSswPage
{
    private readonly Color _parentPageStatusBarColor;
    private readonly IFirebaseAnalyticsService _firebaseAnalyticsService;
    private readonly IAlertService _alertService;

    public AboutSswPage(IFirebaseAnalyticsService firebaseAnalyticsService, IAlertService alertService, Color parentPageStatusBarColor = null)
    {
        _firebaseAnalyticsService = firebaseAnalyticsService;
        _alertService = alertService;
        _parentPageStatusBarColor = parentPageStatusBarColor ?? Colors.Black;
        InitializeComponent();
    }
    
    protected override void OnAppearing()
    {
        base.OnAppearing();
        _firebaseAnalyticsService.Log("AboutSswPage");
        VersionLabel.Text = $"Version {AppInfo.VersionString}";
    }

    private async void Handle_CloseTapped(object sender, EventArgs args)
    {
        await MopupService.Instance.PopAllAsync();
    }

    private async void FindoutMore_Tapped(object sender, EventArgs e)
    {
        try
        {
            await Browser.OpenAsync("https://www.ssw.com.au/ssw/Company/AboutUs.aspx", BrowserLaunchMode.External);
        }
        catch (Exception)
        {
            await _alertService.DisplayAlert("Error", "There was an error trying to launch the default browser.", "OK");
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        // Change status bar back
        this.Behaviors.Clear();
        this.Behaviors.Add(new StatusBarBehavior
        {
            StatusBarColor = _parentPageStatusBarColor,
            StatusBarStyle = StatusBarStyle.LightContent,
        });
    }
}

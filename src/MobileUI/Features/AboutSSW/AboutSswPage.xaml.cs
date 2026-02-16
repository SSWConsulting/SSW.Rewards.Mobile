using Mopups.Services;
using SSW.Rewards.Mobile.Common;

namespace SSW.Rewards.PopupPages;

public partial class AboutSswPage
{
    private readonly IFirebaseAnalyticsService _firebaseAnalyticsService;

    public AboutSswPage(IFirebaseAnalyticsService firebaseAnalyticsService)
    {
        _firebaseAnalyticsService = firebaseAnalyticsService;
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _firebaseAnalyticsService.Log("AboutSswPage");
        VersionLabel.Text = $"Version {AppInfo.VersionString} ({AppInfo.BuildString})";
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
            await IPlatformApplication.Current.DisplayAlertAsync("Error", "There was an error trying to launch the default browser.", "OK");
        }
    }
}

using Mopups.Services;

namespace SSW.Rewards.PopupPages;

public partial class AboutSSW
{
    public AboutSSW()
    {
        InitializeComponent();
    }

    private async void Handle_CloseTapped(object sender, EventArgs args)
    {
        await MopupService.Instance.PopAllAsync();
    }

    private async void FindoutMore_Tapped(object sender, EventArgs e)
    {
        await Browser.OpenAsync("https://www.ssw.com.au/ssw/Company/AboutUs.aspx", BrowserLaunchMode.External);
    }
}

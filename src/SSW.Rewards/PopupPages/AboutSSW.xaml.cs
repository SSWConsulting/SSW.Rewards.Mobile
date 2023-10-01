using Mopups.Pages;
using Mopups.Services;

namespace SSW.Rewards.PopupPages;

public partial class AboutSSW : PopupPage
{
    public async void Handle_CloseTapped(object sender, EventArgs args)
    {
//            DisplayAlert("Close Tapped", "Close", "OK");
        await MopupService.Instance.PopAllAsync();
    }

    public AboutSSW()
    {
        InitializeComponent();
    }

    private async void FindoutMore_Tapped(object sender, EventArgs e)
    {
        await Browser.OpenAsync("https://www.ssw.com.au/ssw/Company/AboutUs.aspx", BrowserLaunchMode.External);
    }
}

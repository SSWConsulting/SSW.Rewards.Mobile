using Mopups.Pages;
using Mopups.Services;

namespace SSW.Rewards.PopupPages
{
    public partial class Events : PopupPage
    {
        public Events()
        {
            InitializeComponent();
        }

        private async void FindoutMore_Tapped(object sender, EventArgs e)
        {
            await Browser.OpenAsync("https://www.ssw.com.au/ssw/Events", BrowserLaunchMode.External);
        }

        public async void Handle_CloseTapped(object sender, EventArgs args)
        {
            //            DisplayAlert("Close Tapped", "Close", "OK");
            await MopupService.Instance.PopAllAsync();
        }
    }
}

using Rg.Plugins.Popup.Services;
using SSW.Rewards.PopupPages;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using ZXing;

namespace SSW.Rewards.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ScanPage : ContentPage
    {
        public ScanPage()
        {
            InitializeComponent();
        }

        public async void Handle_OnScanResult(Result result)
        {
            scannerView.IsScanning = false;
            await PopupNavigation.Instance.PushAsync(new ScanResult(result.Text));
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            scannerView.IsScanning = false;
        }
    }
}
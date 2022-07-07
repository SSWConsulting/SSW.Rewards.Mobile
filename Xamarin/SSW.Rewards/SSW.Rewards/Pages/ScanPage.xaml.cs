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
        public const string EnableScannerMessage = "EnableScanner";
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
            MessagingCenter.Unsubscribe<object>(this, EnableScannerMessage);
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            MessagingCenter.Subscribe<object>(this, EnableScannerMessage, (obj) => EnableScanner());
            scannerView.IsScanning = true;
        }

        private void EnableScanner()
        {
            try
            {
                scannerView.IsScanning = true;
            }
            catch (System.Exception ex)
            {

                throw;
            }
        }
    }
}
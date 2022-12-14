using Rg.Plugins.Popup.Services;
using SSW.Rewards.PopupPages;
using ZXing;
using Microsoft.Maui;
using Microsoft.Maui.Controls;

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
            scannerView.IsAnalyzing = false;
            await PopupNavigation.Instance.PushAsync(new ScanResult(result.Text));
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            scannerView.IsAnalyzing = false;
            MessagingCenter.Unsubscribe<object>(this, Constants.EnableScannerMessage);
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            MessagingCenter.Subscribe<object>(this, Constants.EnableScannerMessage, (obj) => EnableScanner());
            scannerView.IsAnalyzing = true;
        }

        private void EnableScanner()
        {
            scannerView.IsAnalyzing = true;
        }
    }
}
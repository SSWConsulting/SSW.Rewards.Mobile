using System;
using System.Collections.Generic;
using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using SSW.Consulting.ViewModels;
using Xamarin.Forms;
using ZXing;

namespace SSW.Consulting.PopupPages
{
    public partial class QRScannerPage : PopupPage
    {
        public async void Handle_Tapped(object sender, EventArgs e)
        {
            await PopupNavigation.Instance.PopAllAsync();
        }

        private QRScannerPageViewModel _viewModel { get; set; }

        public QRScannerPage()
        {
            InitializeComponent();
            _viewModel = Resolver.Resolve<QRScannerPageViewModel>();
            _viewModel.Navigation = Navigation;
            BindingContext = _viewModel;
        }

        public async void Handle_OnScanResult(Result result)
        {
            scannerView.IsScanning = false;
            await _viewModel.CheckAchievement(result);
        }
    }
}

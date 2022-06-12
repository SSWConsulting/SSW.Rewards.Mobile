using Rg.Plugins.Popup.Pages;
using SSW.Rewards.ViewModels;

namespace SSW.Rewards.PopupPages
{
    public partial class ScanResult : PopupPage
    {
        ScanResultViewModel _viewModel;

        public ScanResult(string scanData)
        {
            InitializeComponent();
            _viewModel = new ScanResultViewModel(scanData);
            _viewModel.Navigation = Navigation;
            BindingContext = _viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.CheckScanData();
        }
    }
}

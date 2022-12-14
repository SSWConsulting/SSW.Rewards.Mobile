using Mopups.Pages;

namespace SSW.Rewards.PopupPages;

public partial class ScanResult : PopupPage
{
    ScanResultViewModel _viewModel;

    public ScanResult(ScanResultViewModel vm, string scanData)
    {
        InitializeComponent();
        _viewModel = vm;
        _viewModel.SetScanData(scanData);
        _viewModel.Navigation = App.Current.MainPage.Navigation;
        BindingContext = _viewModel;

        _ = _viewModel.CheckScanData();
    }
}

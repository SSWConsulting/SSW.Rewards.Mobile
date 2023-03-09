using Mopups.Pages;

namespace SSW.Rewards.PopupPages;

public partial class ScanResult
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

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await Task.Delay(100); // TODO: MAUI, timing issue in SKLottieView https://github.com/mono/SkiaSharp.Extended/issues/142
        ResultAnimation.IsAnimationEnabled = true;
    }
}

using Mopups.Services;
using ZXing.Net.Maui;

namespace SSW.Rewards.Pages;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class ScanPage : ContentPage
{
    private readonly ScanResultViewModel _viewModel;

    public ScanPage(ScanResultViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
    }

    public void Handle_OnScanResult(object sender, BarcodeDetectionEventArgs e)
    {
        scannerView.IsDetecting = false;

        var result = e.Results.FirstOrDefault().Value;

        var popup = new PopupPages.ScanResult(_viewModel, result);
        MopupService.Instance.PushAsync(popup);
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        scannerView.IsDetecting = false;
        MessagingCenter.Unsubscribe<object>(this, Constants.EnableScannerMessage);
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        MessagingCenter.Subscribe<object>(this, Constants.EnableScannerMessage, (obj) => EnableScanner());
        scannerView.IsDetecting = true;
    }

    private void EnableScanner()
    {
        scannerView.IsDetecting = true;
    }
}
using CommunityToolkit.Mvvm.Messaging;
using Mopups.Services;
using SSW.Rewards.Mobile.Messages;
using ZXing.Net.Maui;

namespace SSW.Rewards.Mobile.Pages;

public partial class ScanPage : IRecipient<EnableScannerMessage>
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
        WeakReferenceMessenger.Default.Unregister<EnableScannerMessage>(this);
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        WeakReferenceMessenger.Default.Register(this);
        scannerView.IsDetecting = true;
    }

    private void EnableScanner()
    {
        scannerView.IsDetecting = true;
    }

    public void Receive(EnableScannerMessage message)
    {
        EnableScanner();
    }
}
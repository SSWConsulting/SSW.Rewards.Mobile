using System.Diagnostics;
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
        // the handler is called on a thread-pool thread
        App.Current.Dispatcher.Dispatch(() =>
        {
            if (!scannerView.IsDetecting)
            {
                return;
            }

            ToggleScanner(false);

            var result = e.Results.FirstOrDefault().Value;

            var popup = new PopupPages.ScanResult(_viewModel, result);
            MopupService.Instance.PushAsync(popup);
        });
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        ToggleScanner(false);
        WeakReferenceMessenger.Default.Unregister<EnableScannerMessage>(this);
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        WeakReferenceMessenger.Default.Register(this);
        ToggleScanner(true);
    }


    public void Receive(EnableScannerMessage message)
    {
        ToggleScanner(true);
    }

    private void ToggleScanner(bool toggleOn)
    {
        if (toggleOn)
        {
            scannerView.IsDetecting = true;
            FlipCameras();
        }
        else
        {
            scannerView.IsDetecting = false;
        }
    }

    /// <summary>
    /// There is a bug in ZXing.Net.Maui on Android
    /// where the preview is displayed as black screen if the user navigates between tabs a few times
    /// https://github.com/Redth/ZXing.Net.Maui/issues/67
    /// There are 2 possible workarounds:
    /// 1. Manually Add/Remove CameraBarcodeReaderView from the page every time we navigate to it
    /// 2. Switch camera to Front and then back to Rear
    ///
    /// Decided to go with the latter as the bug is only on Android 
    /// and it's inconvenient to build UI in code-behind.
    /// </summary>
    [Conditional("ANDROID")]
    private void FlipCameras()
    {
        scannerView.CameraLocation = CameraLocation.Front;
        scannerView.CameraLocation = CameraLocation.Rear;
    }
}
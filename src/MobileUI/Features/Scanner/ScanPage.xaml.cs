using System.Diagnostics;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Mopups.Services;
using SSW.Rewards.Mobile.Messages;
using ZXing.Net.Maui;

namespace SSW.Rewards.Mobile.Pages;

public partial class ScanPage : IRecipient<EnableScannerMessage>
{
    private readonly ScanResultViewModel _viewModel;
    
    public BarcodeReaderOptions BarcodeReaderOptions { get; set; } = new()
    {
        Formats = BarcodeFormat.QrCode,
        TryHarder = true,
    };

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
            SetCameraZoom();
        }
        else
        {
            scannerView.IsDetecting = false;
        }
    }
    
    private static async void SetCameraZoom()
    {
#if IOS15_0_OR_GREATER
        // Delay is required to ensure the camera is ready
        await Task.Delay(300);
        
        // Set camera zoom depending on device's minimum focus distance as per Apple's recommendation
        // for scanning barcodes.
        // Adapted from https://stackoverflow.com/questions/74381985/choosing-suitable-camera-for-barcode-scanning-when-using-avcapturedevicetypebuil
        // and https://forums.developer.apple.com/forums/thread/715568
        //
        // Example final VideoZoomFactors:
        // iPhone 14 Pro and 15 Pro (20cm focus distance): ~2.0
        // iPhone 13 Pro (~15cm focus distance): ~1.5
        // iPhone 12 and below: 1.0 - ~1.2
        var captureDevice = AVFoundation.AVCaptureDevice.GetDefaultDevice(AVFoundation.AVMediaTypes.Video);
        
        if (captureDevice == null)
        {
            return;
        }
        
        var focusDistance = captureDevice.MinimumFocusDistance.ToInt32();
        var deviceFieldOfView = captureDevice.ActiveFormat.VideoFieldOfView;
        const float previewFillPercentage = 0.6f; // fill 60% of preview window
        const float minimumTargetObjectSize = 40.0f; // min width 40mm
        double radians = Double.DegreesToRadians(deviceFieldOfView);
        const float filledTargetObjectSize = minimumTargetObjectSize / previewFillPercentage;
        double minimumSubjectDistance = filledTargetObjectSize / Math.Tan(radians / 2.0); // Field of view
        
        if (minimumSubjectDistance < focusDistance)
        {
            captureDevice.LockForConfiguration(out _);
            captureDevice.VideoZoomFactor = (float)(focusDistance / minimumSubjectDistance);
            captureDevice.UnlockForConfiguration();
        }
#endif
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

    [RelayCommand]
    private async Task Dismiss()
    {
        if (Navigation.ModalStack.Count > 0)
        {
            await Navigation.PopModalAsync();
        }
    }
}
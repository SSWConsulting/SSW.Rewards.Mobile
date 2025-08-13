using BarcodeScanning;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Logging;
using Mopups.Services;
using Plugin.Maui.ScreenBrightness;
using SSW.Rewards.Mobile.Controls;
using SSW.Rewards.Mobile.Messages;

namespace SSW.Rewards.Mobile.ViewModels;

public enum ScanPageSegments
{
    Scan,
    MyCode,
}

public partial class ScanViewModel : BaseViewModel, IRecipient<EnableScannerMessage>
{
    private readonly ScanResultViewModel _resultViewModel;
    private readonly ILogger<ScanViewModel> _logger;
    private readonly float _defaultBrightness;
    private const float ZoomFactorStep = 1.0f;
    private const float MaxBrightness = 1.0f;

    private readonly SemaphoreSlim _cameraToggleSemaphore = new(1, 1);
    private readonly SemaphoreSlim _detectionSemaphore = new(1, 1);
    private DateTime _lastCameraToggle = DateTime.MinValue;
    private const int CameraToggleDelayMs = 500;
    
    public ScanPageSegments CurrentSegment { get; set; }

    public List<Segment> Segments { get; set; } =
    [
        new()
        {
            Name = "Scan",
            Value = ScanPageSegments.Scan,
            Icon = new FontImageSource { FontFamily = "FluentIcons", Glyph = "\uf255" }
        },
        new()
        {
            Name = "My Code",
            Value = ScanPageSegments.MyCode,
            Icon = new FontImageSource { FontFamily = "FluentIcons", Glyph = "\uf636" }
        }
    ];
    
    [ObservableProperty]
    private Segment? _selectedSegment;
    
    [ObservableProperty]
    private bool _isCameraEnabled;
    
    [ObservableProperty]
    private float _currentZoomFactor;
    
    [ObservableProperty]
    private float _requestZoomFactor;
    
    [ObservableProperty]
    private float _minZoomFactor;
    
    [ObservableProperty]
    private float _maxZoomFactor;
    
    [ObservableProperty]
    private bool _userHasQrCode;
    
    [ObservableProperty]
    private bool _isScanVisible = true;
    
    [ObservableProperty]
    private string _profilePic;
    
    [ObservableProperty]
    private string _userName;
    
    [ObservableProperty]
    private ImageSource _qrCode;

    [ObservableProperty]
    private bool _hasScanPermissions;

    public ScanViewModel(IUserService userService, ILogger<ScanViewModel> logger, ScanResultViewModel resultViewModel)
    {
        _resultViewModel = resultViewModel;
        _logger = logger;
        
        _defaultBrightness = ScreenBrightness.Default.Brightness;
        
        userService.MyProfilePicObservable().Subscribe(myProfilePage => ProfilePic = myProfilePage);
        userService.MyNameObservable().Subscribe(myName => UserName = myName);

        userService.MyQrCodeObservable().Subscribe(myQrCode =>
        {
            QrCode = ImageHelpers.GenerateQrCode(myQrCode);
            UserHasQrCode = true;
        });
    }

    public async Task OnAppearing()
    {
        WeakReferenceMessenger.Default.Register(this);
        SetSegment(ScanPageSegments.Scan);
        
        HasScanPermissions = await Methods.AskForRequiredPermissionAsync();

        if (HasScanPermissions && IsScanVisible)
        {
            await SetCameraEnabledAsync(true);
        }
    }
    
    public async Task OnDisappearing()
    {
        await SetCameraEnabledAsync(false);
        ScreenBrightness.Default.Brightness = _defaultBrightness;
        WeakReferenceMessenger.Default.Unregister<EnableScannerMessage>(this);
    }
    
    private async Task SetCameraEnabledAsync(bool enabled)
    {
        // Don't allow toggling the camera too quickly, as this can lock up the app

        if (!await _cameraToggleSemaphore.WaitAsync(0))
        {
            return;
        }

        try
        {
            var timeSinceLastToggle = DateTime.UtcNow - _lastCameraToggle;

            if (timeSinceLastToggle.TotalMilliseconds < CameraToggleDelayMs)
            {
                var remainingDelay = CameraToggleDelayMs - (int)timeSinceLastToggle.TotalMilliseconds;
                await Task.Delay(remainingDelay);
            }

            _lastCameraToggle = DateTime.UtcNow;
            IsCameraEnabled = enabled;
        }
        finally
        {
            _cameraToggleSemaphore.Release();
        }
    }

    private async Task ToggleScanner(bool toggleOn)
    {
        IsScanVisible = toggleOn;
        await SetCameraEnabledAsync(toggleOn);
        
        ScreenBrightness.Default.Brightness = toggleOn ? _defaultBrightness : MaxBrightness;
    }
    
    public void Receive(EnableScannerMessage message)
    {
        ToggleScanner(true);
    }

    private void SetSegment(ScanPageSegments segment)
    {
        var matchingSegment = Segments.FirstOrDefault(s => (ScanPageSegments)s.Value == segment);

        if (matchingSegment == null)
        {
            return;
        }

        SelectedSegment = matchingSegment;
        CurrentSegment = segment;
        FilterBySegment();
    }

    [RelayCommand]
    private async Task DetectionFinished(IReadOnlySet<BarcodeResult> result)
    {
        if (!IsCameraEnabled || result.Count == 0)
        {
            return;
        }

        // Prevent concurrent processing of detections
        if (!await _detectionSemaphore.WaitAsync(0))
        {
            return;
        }

        try
        {
            // Go through all detected barcodes and find the first valid QR code.
            var validBarCode = result.FirstOrDefault(x => _resultViewModel.IsQRCodeValid(x?.RawValue));
            string rawValue = validBarCode?.RawValue;
            if (string.IsNullOrWhiteSpace(rawValue))
            {
                return;
            }

            if (Vibration.Default.IsSupported)
                Vibration.Default.Vibrate();

            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                await SetCameraEnabledAsync(false);

                var popup = new PopupPages.ScanResult(_resultViewModel, rawValue);
                await MopupService.Instance.PushAsync(popup);
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while processing barcode");
        }
        finally
        {
            _detectionSemaphore.Release();
        }
    }

    [RelayCommand]
    private void FilterBySegment()
    {
        if (SelectedSegment == null)
            return;

        CurrentSegment = (ScanPageSegments)SelectedSegment.Value;

        switch (CurrentSegment)
        {
            case ScanPageSegments.MyCode:
                ToggleScanner(false);
                break;
            case ScanPageSegments.Scan:
            default:
                ToggleScanner(true);
                break;
        }
    }
    
    [RelayCommand]
    private async Task Dismiss()
    {
        if (Navigation.ModalStack.Count > 0)
        {
            await Navigation.PopModalAsync();
        }
    }
    
    [RelayCommand]
    private void ZoomIn()
    {
        // CurrentZoomFactor can default to -1, so we start at the MinZoomFactor in this case
        var currentZoom = Math.Max(CurrentZoomFactor, MinZoomFactor);
        var maxZoom = MaxZoomFactor;
        
        RequestZoomFactor = Math.Min(currentZoom + ZoomFactorStep, maxZoom);
    }
    
    [RelayCommand]
    private void ZoomOut()
    {
        var currentZoom = CurrentZoomFactor;
        var minZoom = MinZoomFactor;
        
        RequestZoomFactor = Math.Max(currentZoom - ZoomFactorStep, minZoom);
    }
    
    [RelayCommand]
    private static void OpenSettings()
    {
        AppInfo.Current.ShowSettingsUI();
    }
}
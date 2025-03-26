using BarcodeScanning;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
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
    private readonly float _defaultBrightness;
    private const float ZoomFactorStep = 1.0f;
    private const float MaxBrightness = 1.0f;
    
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
    private Segment _selectedSegment;
    
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

    public ScanViewModel(IUserService userService, ScanResultViewModel resultViewModel)
    {
        _resultViewModel = resultViewModel;
        
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
        
        var hasPermissions = await Methods.AskForRequiredPermissionAsync();

        if (hasPermissions && IsScanVisible)
        {
            IsCameraEnabled = true;
        }
    }
    
    public void OnDisappearing()
    {
        IsCameraEnabled = false;
        ScreenBrightness.Default.Brightness = _defaultBrightness;
        WeakReferenceMessenger.Default.Unregister<EnableScannerMessage>(this);
    }
    
    private void ToggleScanner(bool toggleOn)
    {
        IsScanVisible = toggleOn;
        IsCameraEnabled = toggleOn;
        
        ScreenBrightness.Default.Brightness = toggleOn ? _defaultBrightness : MaxBrightness;
    }
    
    public void Receive(EnableScannerMessage message)
    {
        ToggleScanner(true);
    }
    
    public void SetSegment(ScanPageSegments segment)
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
    private void DetectionFinished(BarcodeResult[] result)
    {
        if (!IsCameraEnabled || result.Length == 0)
        {
            return;
        }

        // Go through all detected barcodes and find the first valid QR code.
        var validBarCode = result.FirstOrDefault(x => _resultViewModel.IsQRCodeValid(x?.RawValue));
        string rawValue = validBarCode?.RawValue;
        if (string.IsNullOrWhiteSpace(rawValue))
        {
            return;
        }

        // the handler is called on a thread-pool thread
        App.Current.Dispatcher.Dispatch(() =>
        {
            IsCameraEnabled = false;
            
            var popup = new PopupPages.ScanResult(_resultViewModel, rawValue);
            MopupService.Instance.PushAsync(popup);
        });
    }

    [RelayCommand]
    private void FilterBySegment()
    {
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
}

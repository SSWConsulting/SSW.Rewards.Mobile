using BarcodeScanning;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Mopups.Services;
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
    private const float ZoomFactorStep = 1.0f;
    
    public ScanPageSegments CurrentSegment { get; set; } = ScanPageSegments.Scan;

    public List<Segment> Segments { get; set; } =
    [
        new Segment
        {
            Name = "Scan",
            Value = ScanPageSegments.Scan,
            Icon = new FontImageSource { FontFamily = "FluentIcons", Glyph = "\uf255" }
        },
        new Segment
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
        
        userService.MyProfilePicObservable().Subscribe(myProfilePage => ProfilePic = myProfilePage);
        userService.MyNameObservable().Subscribe(myName => UserName = myName);

        userService.MyQrCodeObservable().Subscribe(myQrCode =>
        {
            QrCode = ImageHelpers.GenerateQrCode(myQrCode);
            UserHasQrCode = true;
        });
    }

    public void OnAppearing()
    {
        WeakReferenceMessenger.Default.Register(this);
        
        if (CurrentSegment == ScanPageSegments.Scan)
        {
            ToggleScanner(true);
        }
    }
    
    public void OnDisappearing()
    {
        // Reset zoom when exiting camera
        if (CurrentZoomFactor > -1)
        {
            RequestZoomFactor = MinZoomFactor;
        }
        
        ToggleScanner(false);
        WeakReferenceMessenger.Default.Unregister<EnableScannerMessage>(this);
    }
    
    private void ToggleScanner(bool toggleOn)
    {
        IsCameraEnabled = toggleOn;
    }
    
    public void Receive(EnableScannerMessage message)
    {
        ToggleScanner(true);
    }

    [RelayCommand]
    private void DetectionFinished(BarcodeResult[] result)
    {
        // the handler is called on a thread-pool thread
        App.Current.Dispatcher.Dispatch(() =>
        {
            if (!IsCameraEnabled || result.Length == 0)
            {
                return;
            }
            
            ToggleScanner(false);
            
            var rawValue = result.FirstOrDefault()?.RawValue;

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
            case ScanPageSegments.Scan:
                IsScanVisible = true;
                ToggleScanner(true);
                break;
            case ScanPageSegments.MyCode:
            default:
                IsScanVisible = false;
                ToggleScanner(false);
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

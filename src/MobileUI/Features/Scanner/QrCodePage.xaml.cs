using CommunityToolkit.Maui.Behaviors;
using CommunityToolkit.Maui.Core;
using Plugin.Maui.ScreenBrightness;

namespace SSW.Rewards.Mobile.PopupPages;

public partial class QrCodePage
{
    private readonly Color _parentPageStatusBarColor;
    private readonly IFirebaseAnalyticsService _firebaseAnalyticsService;

    private float _prevValue;
    public QrCodePage(QrCodeViewModel viewModel, IFirebaseAnalyticsService firebaseAnalyticsService, Color parentPageStatusBarColor = null)
    {
        InitializeComponent();
        BindingContext = viewModel;
        _parentPageStatusBarColor = parentPageStatusBarColor ?? Colors.Black;
        _firebaseAnalyticsService = firebaseAnalyticsService;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _firebaseAnalyticsService.Log("QrCodePage");
        _prevValue = ScreenBrightness.Default.Brightness;
        ScreenBrightness.Default.Brightness = 1;
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        ScreenBrightness.Default.Brightness = _prevValue;
        
        // Change status bar back
        this.Behaviors.Clear();
        this.Behaviors.Add(new StatusBarBehavior
        {
            StatusBarColor = _parentPageStatusBarColor,
            StatusBarStyle = StatusBarStyle.LightContent,
        });
    }
}
using CommunityToolkit.Maui.Behaviors;
using CommunityToolkit.Maui.Core;

namespace SSW.Rewards.Mobile.PopupPages;

public partial class AddSocialMediaPage
{
    private readonly Color _parentPageStatusBarColor;
    private readonly IFirebaseAnalyticsService _firebaseAnalyticsService;

    public AddSocialMediaPage(IFirebaseAnalyticsService firebaseAnalyticsService, IServiceProvider provider,
        SocialMediaPlatform socialMediaPlatform,
        Color parentPageStatusBarColor = null)
    {
        _parentPageStatusBarColor = parentPageStatusBarColor ?? Colors.Black;
        _firebaseAnalyticsService = firebaseAnalyticsService;
        InitializeComponent();
        BindingContext = ActivatorUtilities.CreateInstance<AddSocialMediaViewModel>(provider, socialMediaPlatform);
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _firebaseAnalyticsService.Log("AddSocialMediaPage");
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        // Change status bar back
        this.Behaviors.Clear();
        this.Behaviors.Add(new StatusBarBehavior
        {
            StatusBarColor = _parentPageStatusBarColor, StatusBarStyle = StatusBarStyle.LightContent,
        });
    }
}
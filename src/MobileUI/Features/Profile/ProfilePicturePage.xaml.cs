using CommunityToolkit.Maui.Behaviors;
using CommunityToolkit.Maui.Core;

namespace SSW.Rewards.PopupPages;

public partial class ProfilePicturePage
{
    private readonly Color _parentPageStatusBarColor;
    private readonly IFirebaseAnalyticsService _firebaseAnalyticsService;

    public ProfilePicturePage(IFirebaseAnalyticsService firebaseAnalyticsService, ProfilePictureViewModel profilePictureViewModel, Color parentPageStatusBarColor = null)
    {
        _parentPageStatusBarColor = parentPageStatusBarColor ?? Colors.Black;
        _firebaseAnalyticsService = firebaseAnalyticsService;
        InitializeComponent();
        profilePictureViewModel.Navigation = Navigation;
        BindingContext = profilePictureViewModel;
    }
    
    protected override void OnAppearing()
    {
        base.OnAppearing();
        _firebaseAnalyticsService.Log("ProfilePicturePage");
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        // Change status bar back
        this.Behaviors.Clear();
        this.Behaviors.Add(new StatusBarBehavior
        {
            StatusBarColor = _parentPageStatusBarColor,
            StatusBarStyle = StatusBarStyle.LightContent,
        });
    }
}

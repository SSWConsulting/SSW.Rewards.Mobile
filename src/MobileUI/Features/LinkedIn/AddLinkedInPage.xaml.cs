using CommunityToolkit.Maui.Behaviors;
using CommunityToolkit.Maui.Core;

namespace SSW.Rewards.Mobile.PopupPages;

public partial class AddLinkedInPage
{
    private readonly Color _parentPageStatusBarColor;
    private readonly IFirebaseAnalyticsService _firebaseAnalyticsService;

    public AddLinkedInPage(IUserService userService, ISnackbarService snackbarService, IFirebaseAnalyticsService firebaseAnalyticsService, Color parentPageStatusBarColor = null)
    {
        _parentPageStatusBarColor = parentPageStatusBarColor ?? Colors.Black;
        _firebaseAnalyticsService = firebaseAnalyticsService;
        InitializeComponent();
        BindingContext = new AddLinkedInViewModel(userService, snackbarService);
    }
    
    protected override void OnAppearing()
    {
        base.OnAppearing();
        _firebaseAnalyticsService.Log("AddLinkedInPage");
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
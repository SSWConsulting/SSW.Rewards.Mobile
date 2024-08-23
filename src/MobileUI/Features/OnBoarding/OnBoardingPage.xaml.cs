using CommunityToolkit.Maui.Behaviors;
using CommunityToolkit.Maui.Core;

namespace SSW.Rewards.Mobile.Pages;

public partial class OnBoardingPage
{
    private readonly OnBoardingViewModel _viewModel;
    private readonly Color _parentPageStatusBarColor;
    private readonly IFirebaseAnalyticsService _firebaseAnalyticsService;

    public OnBoardingPage(IFirebaseAnalyticsService firebaseAnalyticsService, bool isFirstRun = false, Color parentPageStatusBarColor = null)
    {
        _firebaseAnalyticsService = firebaseAnalyticsService;
        _parentPageStatusBarColor = parentPageStatusBarColor ?? Colors.Black;
        InitializeComponent();
        _viewModel = new OnBoardingViewModel(isFirstRun);
        BindingContext = _viewModel;
    }

    protected override bool OnBackButtonPressed()
    {
        _viewModel.ClosePageCommand.ExecuteAsync(null);
        return true;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _firebaseAnalyticsService.Log("OnBoardingPage");
        _viewModel.Initialise();
        _viewModel.ScrollToRequested += ScrollToIndex;
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _viewModel.ScrollToRequested -= ScrollToIndex;
        _viewModel.Items.Clear();

        // Change status bar back
        this.Behaviors.Clear();
        this.Behaviors.Add(new StatusBarBehavior
        {
            StatusBarColor = _parentPageStatusBarColor,
            StatusBarStyle = StatusBarStyle.LightContent,
        });
    }

    private void ScrollToIndex(object sender, int index)
    {
        RewardsCarousel.ScrollTo(index);
    }
}
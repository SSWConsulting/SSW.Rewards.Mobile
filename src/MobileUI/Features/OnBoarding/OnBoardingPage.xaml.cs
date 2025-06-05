namespace SSW.Rewards.Mobile.Pages;

public partial class OnBoardingPage
{
    private readonly OnBoardingViewModel _viewModel;
    private readonly IFirebaseAnalyticsService _firebaseAnalyticsService;

    public OnBoardingPage(IFirebaseAnalyticsService firebaseAnalyticsService)
    {
        _firebaseAnalyticsService = firebaseAnalyticsService;
        InitializeComponent();
        _viewModel = new OnBoardingViewModel();
        BindingContext = _viewModel;
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
    }

    private void ScrollToIndex(object sender, int index)
    {
        RewardsCarousel.ScrollTo(index);
    }
}
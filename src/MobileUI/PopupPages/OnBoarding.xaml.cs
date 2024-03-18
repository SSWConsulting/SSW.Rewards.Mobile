namespace SSW.Rewards.Mobile.Pages;

public partial class OnBoarding
{
    private readonly OnBoardingViewModel _viewModel;

    public OnBoarding()
    {
        InitializeComponent();
        _viewModel = new OnBoardingViewModel();
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.ScrollToRequested += ScrollToIndex;
        await Task.Delay(300);
        _viewModel.IsOverlayVisible = true;
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _viewModel.ScrollToRequested -= ScrollToIndex;
    }

    private void ScrollToIndex(object sender, int index)
    {
        RewardsCarousel.ScrollTo(index);
    }
}
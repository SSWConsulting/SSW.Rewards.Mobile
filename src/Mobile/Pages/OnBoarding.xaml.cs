namespace SSW.Rewards.Mobile.Pages;

public partial class OnBoarding : ContentPage
{
    private readonly OnBoardingViewModel _viewModel;

    public OnBoarding(OnBoardingViewModel viewModel)
    {
        InitializeComponent();
        viewModel.Navigation = Navigation;
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.ScrollToRequested += ScrollToIndex;
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
using CommunityToolkit.Maui.Behaviors;
using CommunityToolkit.Maui.Core;

namespace SSW.Rewards.Mobile.Pages;

public partial class OnBoardingPage
{
    private readonly OnBoardingViewModel _viewModel;

    public OnBoardingPage(bool isFirstRun = false)
    {
        InitializeComponent();
        _viewModel = new OnBoardingViewModel(isFirstRun);
        BindingContext = _viewModel;
    }

    protected override bool OnBackButtonPressed()
    {
        _viewModel.ClosePageCommand.ExecuteAsync(null);
        return true;
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

        // Change status bar back
        Application.Current.Resources.TryGetValue("Background", out var color);
        this.Behaviors.Clear();
        this.Behaviors.Add(new StatusBarBehavior
        {
            StatusBarColor = (Color) color ?? Colors.Black,
            StatusBarStyle = StatusBarStyle.LightContent,
        });
    }

    private void ScrollToIndex(object sender, int index)
    {
        RewardsCarousel.ScrollTo(index);
    }
}
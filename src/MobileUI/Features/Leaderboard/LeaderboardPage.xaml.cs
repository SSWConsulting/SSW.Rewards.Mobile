namespace SSW.Rewards.Mobile.Pages;

public partial class LeaderboardPage : ContentPage
{
    private readonly LeaderboardViewModel _viewModel;

    public LeaderboardPage(LeaderboardViewModel leaderboardViewModel)
    {
        _viewModel = leaderboardViewModel;
        _viewModel.Navigation = Navigation;
        BindingContext = _viewModel;
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.Initialise();
        await Animate();
        _viewModel.ScrollTo += ScrollTo;
    }

    private void ScrollTo(int i)
    {
        LeadersCollection.ScrollTo(i, position: ScrollToPosition.Center);
    }

    private async Task Animate()
    {
        if (!_viewModel.IsRunning)
        {
            LeadersCollection.IsVisible = true;
            return;
        }

        _viewModel.IsRunning = true;

        await Task.Delay(1000);
        LeadersCollection.Opacity = 0;
        LeadersCollection.IsVisible = true;
        await LeadersCollection.FadeTo(1, 800, Easing.CubicIn);

        _viewModel.IsRunning = false;
    }
}
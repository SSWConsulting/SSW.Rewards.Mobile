namespace SSW.Rewards.Mobile.Pages;

public partial class LeaderboardPage : ContentPage
{
    private readonly LeaderBoardViewModel _viewModel;

    public LeaderboardPage(LeaderBoardViewModel leaderBoardViewModel)
    {
        _viewModel = leaderBoardViewModel;
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
        LeadersCollection.Opacity = 0;
        LeadersCollection.TranslationY = 400;
        
        await Task.WhenAll
        (
            LeadersCollection.FadeTo(1, 1000, Easing.SinIn),
        	LeadersCollection.TranslateTo(0, 0, 1000, Easing.SinIn)
        );

    }
}
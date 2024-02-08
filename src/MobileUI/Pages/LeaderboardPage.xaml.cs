namespace SSW.Rewards.Mobile.Pages;

public partial class LeaderboardPage : ContentPage
{
    private readonly LeaderBoardViewModel _viewModel;
    private bool _isLoaded;

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
        if (_isLoaded)
        {
            LeadersCollection.IsVisible = true;
            return;
        }
        
        await Task.Delay(1000);
        LeadersCollection.Opacity = 0;
        LeadersCollection.TranslationY = 400;
        LeadersCollection.IsVisible = true;
        
        await Task.WhenAll
        (
            LeadersCollection.FadeTo(1, 800, Easing.CubicIn),
            LeadersCollection.TranslateTo(0, 0, 800, Easing.SinIn)
        );
        
        _isLoaded = true;
    }

    private double _screenHeight = 1;

    protected override void OnSizeAllocated(double width, double height)
    {
        _screenHeight = height;

        FirstRow.Height = height * 16;

        SecondRow.Height = (height * 16) * 7;

        ThirdRow.Height = height / 2;

        base.OnSizeAllocated(width, height);
    }

    private void LeadersCollection_Scrolled(object sender, ItemsViewScrolledEventArgs e)
    {
        SetScaleFactor(e.VerticalDelta);
    }

    private void SetScaleFactor (double offset)
    {

    }
}
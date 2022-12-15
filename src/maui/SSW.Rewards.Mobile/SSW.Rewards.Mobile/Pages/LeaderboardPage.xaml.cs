namespace SSW.Rewards.Mobile.Pages;

[XamlCompilation(XamlCompilationOptions.Compile)]
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
        _viewModel.ScrollTo += ScrollTo;
    }

    private void FilterChanged(object sender, CheckedChangedEventArgs e)
    {
        if (e.Value)
        {
            var radio = (RadioButton)sender;

            switch (radio.Content as string)
            {
                case "This Month":
                    _viewModel.SortLeaders("month");
                    break;
                case "This Year":
                    _viewModel.SortLeaders("year");
                    break;
                default:
                    _viewModel.SortLeaders("all");
                    break;
            }
        }
    }


    private void ScrollTo(int i)
    {
        LeadersCollection.ScrollTo(i, position: ScrollToPosition.Center);
    }

    private void CollectionView_Scrolled(object sender, ItemsViewScrolledEventArgs e)
    {
        if (e.FirstVisibleItemIndex > 1)
        {
            // show the scrolling controls
            ScrollButtons.IsVisible = true;
        }
        else
        {
            // hide the scrolling controls
            ScrollButtons.IsVisible = false;
        }
    }
}
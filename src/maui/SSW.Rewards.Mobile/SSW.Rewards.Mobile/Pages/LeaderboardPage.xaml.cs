using SSW.Rewards.Mobile.Controls;

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

    // TODO:    This method has been hacked a little and removed from teh CheckedChanged handler
    //          of the RadioButtons, as well as the Radio_Tapped method added below. This is to
    //          workaround a bug in .NET MAUI. see: https://github.com/dotnet/maui/issues/6938.
    //          We can remove this when this issue gets resolved.
    //private void FilterChanged(string value)
    //{
    //    //if (e.Value)
    //    //{
    //    //    var radio = (RadioButton)sender;

    //    //    switch (radio. as string)
    //    //    {
    //    //        case "This Month":
    //    //            _viewModel.SortLeaders("month");
    //    //            break;
    //    //        case "This Year":
    //    //            _viewModel.SortLeaders("year");
    //    //            break;
    //    //        default:
    //    //            _viewModel.SortLeaders("all");
    //    //            break;
    //    //    }
    //    //}

    //    switch (value)
    //    {
    //        case "This Month":
    //            _viewModel.SortLeaders("month");
    //            break;
    //        case "This Year":
    //            _viewModel.SortLeaders("year");
    //            break;
    //        default:
    //            _viewModel.SortLeaders("all");
    //            break;
    //    }
    //}


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

    //private void Radio_Tapped(object sender, TappedEventArgs e)
    //{
    //    RadioButton button = default;

    //    button = ((Label)sender).Parent as RadioButton;

    //    button.IsChecked = true;

    //    var blah = (Label)sender;

    //    FilterChanged(blah.Text);
    //}

    private void TabHeader_FilterChanged(object sender, FilterChangedEventArgs e)
    {
        var filter = e.FilterRange;

        switch (filter)
        {
            case FilterRange.Month:
                _viewModel.SortLeaders("month");
                break;
            case FilterRange.Year:
                _viewModel.SortLeaders("year");
                break;
            case FilterRange.AllTime:
            default:
                _viewModel.SortLeaders("all");
                break;
        }
    }
}
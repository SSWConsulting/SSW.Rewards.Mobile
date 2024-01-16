using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Input;
using SSW.Rewards.Enums;
using SSW.Rewards.Mobile.Controls;
using SSW.Rewards.Mobile.Messages;

namespace SSW.Rewards.Mobile.ViewModels;

public partial class LeaderBoardViewModel : BaseViewModel, IRecipient<PointsAwardedMessage>
{
    private ILeaderService _leaderService;
    private IUserService _userService;
    private bool _loaded;

    [ObservableProperty]
    private ObservableCollection<LeaderViewModel> searchResults = new ();
    
    public LeaderBoardViewModel(ILeaderService leaderService, IUserService userService)
    {
        Title = "Leaderboard";
        OnRefreshCommand = new Command(async () => await Refresh());
        _leaderService = leaderService;
        _userService = userService;
        ProfilePic = _userService.MyProfilePic;
        MyPoints = _userService.MyPoints;
        MyBalance = _userService.MyBalance;
        Leaders = new ObservableCollection<LeaderViewModel>();
        WeakReferenceMessenger.Default.Register(this);
    }
    
    public ICommand LeaderTapped => new Command<LeaderViewModel>(async (x) => await HandleLeaderTapped(x));
    public ICommand OnRefreshCommand { get; set; }
    public ICommand ScrollToTopCommand => new Command(ScrollToFirstCard);
    public ICommand ScrollToEndCommand => new Command(ScrollToLastCard);
    public ICommand ScrollToMeCommand => new Command(ScrollToMyCard);
    public ICommand RefreshCommand => new Command(async () => await RefreshLeaderboard());
    public ICommand SearchTextCommand => new Command<string>(SearchTextHandler);
    public IAsyncRelayCommand GoToMyProfileCommand => new AsyncRelayCommand(() => Shell.Current.GoToAsync("//main"));

    public ObservableCollection<LeaderViewModel> Leaders { get; set; }

    [ObservableProperty]
    private List<Segment> _periods;
    
    [ObservableProperty]
    private bool _isRunning;
    
    [ObservableProperty]
    private bool _isRefreshing;
    
    public string ProfilePic { get; set; }
    public Action<int> ScrollTo { get; set; }

    public LeaderboardFilter CurrentPeriod { get; set; }
    
    [ObservableProperty]
    private int _totalLeaders;
    
    [ObservableProperty]
    private int _myRank;
    
    public int MyPoints { get; set; }
    public int MyBalance { get; set; }

    /// <summary>
    /// Flip the value to clear search input field
    /// </summary>
    [ObservableProperty]
    private bool _clearSearch;

    public async Task Initialise()
    {
        if (!_loaded)
        {
            IsRunning = true;

            await LoadLeaderboard();
            _loaded = true;

            await FilterAndSortLeaders(Leaders, LeaderboardFilter.ThisWeek);

            IsRunning = false;
        }

        Periods = new List<Segment>
        {
            new Segment { Name = "This Week", Value = LeaderboardFilter.ThisWeek },
            new Segment { Name = "This Month", Value = LeaderboardFilter.ThisMonth },
            new Segment { Name = "This Year", Value = LeaderboardFilter.ThisYear },
            new Segment { Name = "All Time", Value = LeaderboardFilter.Forever },
        };

        Console.WriteLine($"Period count: {Periods.Count}");
    }

    private async Task RefreshLeaderboard()
    {
        await LoadLeaderboard();
        IsRefreshing = false;
    }

    private async Task LoadLeaderboard()
    {
        var summaries = await _leaderService.GetLeadersAsync(false);

        int myId = _userService.MyUserId;

        Leaders.Clear();

        foreach (var summary in summaries)
        {
            var isMe = myId == summary.UserId;
            var vm = new LeaderViewModel(summary, isMe);

            Leaders.Add(vm);
        }

        TotalLeaders = summaries.Count();

        ScrollTo?.Invoke(0);
    }

    private async Task UpdateSearchResults(IEnumerable<LeaderViewModel> sortedLeaders)
    {
        var newList = new ObservableCollection<LeaderViewModel>(sortedLeaders);
        await App.Current.MainPage.Dispatcher.DispatchAsync(() =>
        {
            SearchResults = newList;
        });
    }

    [RelayCommand]
    private async Task FilterByPeriod()
    {
        ClearSearch = !ClearSearch;
        await FilterAndSortLeaders(Leaders, CurrentPeriod);
    }

    public async Task FilterAndSortLeaders(IEnumerable<LeaderViewModel> list, LeaderboardFilter period, bool keepRank = false)
    {
        Func<LeaderViewModel, int> sortKeySelector;

        switch (period)
        {
            case LeaderboardFilter.ThisMonth:
                sortKeySelector = l => l.PointsThisMonth;
                break;
            case LeaderboardFilter.ThisYear:
                sortKeySelector = l => l.PointsThisYear;
                break;
            case LeaderboardFilter.Forever:
                sortKeySelector = l => l.TotalPoints;
                break;
            case LeaderboardFilter.ThisWeek:
            default:
                sortKeySelector = l => l.PointsThisWeek;
                break;
        }

        await FilterAndSortBy(list, sortKeySelector, keepRank);
    }

    private async Task FilterAndSortBy(IEnumerable<LeaderViewModel> list, Func<LeaderViewModel, int> sortKeySelector, bool keepRank)
    {
        var leaders = list.OrderByDescending(sortKeySelector).ToList();
        int rank = 1;

        foreach (var leader in leaders)
        {
            if (!keepRank)
            {
                leader.Rank = rank;
                rank++;
            }

            leader.DisplayPoints = sortKeySelector(leader);
        }

        await UpdateSearchResults(leaders);
        UpdateMyRankIfRequired(leaders.FirstOrDefault(l => l.IsMe == true));
    }

    public async Task Refresh()
    {
        var summaries = await _leaderService.GetLeadersAsync(false);
        int myId = _userService.MyUserId;

        Leaders.Clear();
        
        foreach (var summary in summaries)
        {
            var isMe = myId == summary.UserId;
            var vm = new LeaderViewModel(summary, isMe);

            Leaders.Add(vm);
        }

        FilterAndSortLeaders(Leaders, CurrentPeriod);
        
        IsRefreshing = false;
    }

    private void ScrollToMyCard()
    {
        var myCard = SearchResults.FirstOrDefault(l => l.IsMe);
        var myIndex = SearchResults.IndexOf(myCard);

        ScrollTo.Invoke(myIndex);
    }

    private void ScrollToFirstCard()
    {
        ScrollTo.Invoke(0);
    }

    private void ScrollToLastCard()
    {
        ScrollTo.Invoke(SearchResults.Count() - 1);
    }

    private async Task HandleLeaderTapped(LeaderViewModel leader)
    {
        if (leader.IsMe)
            await Shell.Current.GoToAsync("//main");
        else
            await Shell.Current.Navigation.PushModalAsync<OthersProfilePage>(leader);
    }

    public async void Receive(PointsAwardedMessage message)
    {
        await Refresh();
    }

    private void UpdateMyRankIfRequired(LeaderViewModel mySummary)
    {
        if (mySummary is not null)
        {
            MyRank = mySummary.Rank;
        }
    }

    private void SearchTextHandler(string searchBarText)
    {
        // UserStoppedTypingBehavior fires the command on a threadPool thread
        // as internally it uses .ContinueWith
        App.Current.MainPage.Dispatcher.Dispatch(() =>
        {
            if (searchBarText != null)
            {
                var filtered = Leaders.Where(l => l.Name.ToLower().Contains(searchBarText.ToLower()));
                //SearchResults = new ObservableCollection<LeaderViewModel>(filtered);
                FilterAndSortLeaders(filtered, CurrentPeriod, true);
            }
        });
    }
}

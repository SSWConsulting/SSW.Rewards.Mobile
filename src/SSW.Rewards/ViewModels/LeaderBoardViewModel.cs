using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Input;
using SSW.Rewards.Mobile.Controls;
using SSW.Rewards.Mobile.Messages;

namespace SSW.Rewards.Mobile.ViewModels;

public class LeaderBoardViewModel : BaseViewModel, IRecipient<PointsAwardedMessage>
{
    private ILeaderService _leaderService;
    private IUserService _userService;
    private bool _loaded;
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
    public IAsyncRelayCommand FilterByPeriodCommand => new AsyncRelayCommand(FilterByPeriod);

    public ObservableCollection<LeaderViewModel> Leaders { get; set; }
    public bool IsRunning { get; set; }
    public bool IsRefreshing { get; set; }
    public string ProfilePic { get; set; }
    public Action<int> ScrollTo { get; set; }
    public PeriodFilter CurrentPeriod { get; set; }
    public int TotalLeaders { get; set; }
    public int MyRank { get; set; }
    public int MyPoints { get; set; }
    public int MyBalance { get; set; }
    /// <summary>
    /// Flip the value to clear search input field
    /// </summary>
    public bool ClearSearch { get; set; }
    
    public ObservableCollection<LeaderViewModel> SearchResults
    {
        get => searchResults;
        set
        {
            searchResults = value;
            RaisePropertyChanged("SearchResults");
        }
    }

    public async Task Initialise()
    {
        if (!_loaded)
        {
            IsRunning = true;
            RaisePropertyChanged(nameof(IsRunning));

            await LoadLeaderboard();
            _loaded = true;

            await FilterAndSortLeaders(Leaders, PeriodFilter.Month);

            IsRunning = false;
            RaisePropertyChanged(nameof(IsRunning));
        }
    }

    private async Task RefreshLeaderboard()
    {
        await LoadLeaderboard();
        IsRefreshing = false;
        OnPropertyChanged(nameof(IsRefreshing));
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

        OnPropertyChanged(nameof(TotalLeaders));

        ScrollTo?.Invoke(0);
    }

    private async Task UpdateSearchResults(IEnumerable<LeaderViewModel> sortedLeaders)
    {
        var newList = new ObservableCollection<LeaderViewModel>(sortedLeaders);
        await App.Current.MainPage.Dispatcher.DispatchAsync(() =>
        {
            SearchResults = newList;
            OnPropertyChanged(nameof(SearchResults));
        });
    }


    private async Task FilterByPeriod()
    {
        ClearSearch = !ClearSearch;
        RaisePropertyChanged(nameof(ClearSearch));
        await FilterAndSortLeaders(Leaders, CurrentPeriod);
    }

    public async Task FilterAndSortLeaders(IEnumerable<LeaderViewModel> list, PeriodFilter period, bool keepRank = false)
    {
        Func<LeaderViewModel, int> sortKeySelector;

        switch (period)
        {
            case PeriodFilter.Month:
                sortKeySelector = l => l.PointsThisMonth;
                break;
            case PeriodFilter.Year:
                sortKeySelector = l => l.PointsThisYear;
                break;
            case PeriodFilter.AllTime:
            default:
                sortKeySelector = l => l.TotalPoints;
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
        
        RaisePropertyChanged("IsRefreshing");
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
            await Shell.Current.Navigation.PushModalAsync<ProfilePage>(leader);
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
            OnPropertyChanged(nameof(MyRank));
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

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SSW.Rewards.Enums;
using SSW.Rewards.Mobile.Controls;
using SSW.Rewards.Shared.DTOs.ActivityFeed;
using SSW.Rewards.Shared.DTOs.Leaderboard;

namespace SSW.Rewards.Mobile.ViewModels;

public partial class LeaderboardViewModel : BaseViewModel
{
    private int _myUserId;

    private readonly ILeaderService _leaderService;
    private readonly IUserService _userService;
    private readonly IServiceProvider _provider;
    private bool _loaded;

    private const int Take = 50;
    private int _skip;
    private bool _limitReached;

    public ObservableRangeCollection<LeaderViewModel> SearchResults { get; set; } = [];

    public LeaderboardViewModel(ILeaderService leaderService, IUserService userService, IServiceProvider provider)
    {
        Title = "Leaderboard";
        _leaderService = leaderService;
        _userService = userService;
        _provider = provider;
        _userService.MyUserIdObservable().Subscribe(myUserId => _myUserId = myUserId);
        _userService.MyPointsObservable().Subscribe(myPoints => MyPoints = myPoints);
        _userService.MyBalanceObservable().Subscribe(HandleMyBalanceChange);
    }

    public ObservableCollection<LeaderViewModel> Leaders { get; } = [];

    [ObservableProperty]
    private List<Segment> _periods;

    [ObservableProperty]
    private bool _isRunning;

    [ObservableProperty]
    private bool _isRefreshing;

    public Action<int> ScrollTo { get; set; }

    public LeaderboardFilter CurrentPeriod { get; set; }

    [ObservableProperty]
    private Segment _selectedPeriod;

    [ObservableProperty]
    private int _myRank;

    [ObservableProperty]
    private LeaderViewModel _first;

    [ObservableProperty]
    private LeaderViewModel _second;

    [ObservableProperty]
    private LeaderViewModel _third;

    public int MyPoints { get; set; }
    public int MyBalance { get; set; }

    /// <summary>
    /// Flip the value to clear search input field
    /// </summary>
    [ObservableProperty]
    private bool _clearSearch;

    public async Task Initialise()
    {
        if (Periods is null || !Periods.Any())
        {
            Periods = new List<Segment>
            {
                new() { Name = "Week", Value = LeaderboardFilter.ThisWeek },
                new() { Name = "Month", Value = LeaderboardFilter.ThisMonth },
                new() { Name = "Year", Value = LeaderboardFilter.ThisYear },
                new() { Name = "All Time", Value = LeaderboardFilter.Forever },
            };
        }

        if (!_loaded)
        {
            Leaders.Clear();
            IsRunning = true;
            _skip = 0;

            await LoadLeaderboard();
            _loaded = true;

            IsRunning = false;
        }
    }

    [RelayCommand]
    private async Task RefreshLeaderboard()
    {
        Leaders.Clear();
        _skip = 0;

        await LoadLeaderboard();
        IsRefreshing = false;
    }

    [RelayCommand]
    private async Task LoadMore()
    {
        if (_limitReached)
            return;

        _skip += Take;
        var feed = await LoadLeaderboard();

        if (feed.Count == 0)
        {
            _limitReached = true;
            return;
        }
    }

    [RelayCommand]
    private async Task FilterByPeriod()
    {
        CurrentPeriod = (LeaderboardFilter)SelectedPeriod.Value;
        ClearSearch = !ClearSearch;

        Leaders.Clear();
        _skip = 0;

        await LoadLeaderboard();
    }

    [RelayCommand]
    private async Task LeaderTapped(LeaderViewModel leader)
    {
        if (leader.IsMe)
        {
            var page = ActivatorUtilities.CreateInstance<MyProfilePage>(_provider);
            await Shell.Current.Navigation.PushAsync(page);
        }
        else
        {
            var page = ActivatorUtilities.CreateInstance<OthersProfilePage>(_provider, leader.UserId);
            await Shell.Current.Navigation.PushAsync(page);
        }
    }

    [RelayCommand]
    private void ScrollToMe()
    {
        var myCard = SearchResults.FirstOrDefault(l => l.IsMe);
        var myIndex = SearchResults.IndexOf(myCard);
        ScrollTo(myIndex);
    }

    private async Task<List<LeaderboardUserDto>> LoadLeaderboard()
    {
        var summaries = await _leaderService.GetLeadersAsync(Take, _skip, CurrentPeriod, false);

        foreach (var summary in summaries)
        {
            var isMe = _myUserId == summary.UserId;
            var vm = new LeaderViewModel(summary, isMe);

            Leaders.Add(vm);
        }

        await FilterAndSortLeaders(Leaders, CurrentPeriod);

        return summaries.ToList();
    }

    private async Task UpdateSearchResults(IEnumerable<LeaderViewModel> sortedLeaders)
    {
        var newList = new ObservableCollection<LeaderViewModel>(sortedLeaders);
        await App.Current.MainPage.Dispatcher.DispatchAsync(() =>
        {
            SearchResults.ReplaceRange(newList);
        });
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
        var myProfile = leaders.FirstOrDefault(l => l.IsMe);
        UpdateMyRank(myProfile);
        UpdateMyAllTimeRank(myProfile);

        // setting to null to trigger PropertyChanged event
        First = null!;
        Second = null!;
        Third = null!;
        First = leaders.FirstOrDefault();
        Second = leaders.Skip(1).FirstOrDefault();
        Third = leaders.Skip(2).FirstOrDefault();
    }

    private async void HandleMyBalanceChange(int myBalance)
    {
        MyBalance = myBalance;

        // Don't attempt to refresh leaderboard until initial load is complete
        if (!_loaded)
        {
            return;
        }

        Leaders.Clear();
        _skip = 0;

        await LoadLeaderboard();
        IsRefreshing = false;
    }

    private void UpdateMyRank(LeaderViewModel mySummary)
    {
        if (mySummary is not null)
        {
            MyRank = mySummary.Rank;
        }
    }

    private void UpdateMyAllTimeRank(LeaderViewModel me)
    {
        if (me is not null)
        {
            _userService.UpdateMyAllTimeRank(me.AllTimeRank);
        }
    }
}

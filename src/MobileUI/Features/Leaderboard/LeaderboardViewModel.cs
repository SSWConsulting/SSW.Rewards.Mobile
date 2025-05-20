using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SSW.Rewards.Enums;
using SSW.Rewards.Mobile.Controls;
using SSW.Rewards.Shared.DTOs.Leaderboard;
using SSW.Rewards.Mobile.Services;

namespace SSW.Rewards.Mobile.ViewModels;

public partial class LeaderboardViewModel : BaseViewModel
{
    private int _myUserId;

    private readonly ILeaderService _leaderService;
    private readonly IUserService _userService;
    private readonly IServiceProvider _provider;
    private readonly IFileCacheService _fileCacheService;
    private bool _loaded;

    private const int Take = 50;
    private int _skip;
    private bool _limitReached;

    public LeaderboardViewModel(ILeaderService leaderService, IUserService userService, IServiceProvider provider, IFileCacheService fileCacheService)
    {
        Title = "Leaderboard";
        _leaderService = leaderService;
        _userService = userService;
        _provider = provider;
        _fileCacheService = fileCacheService;
        _userService.MyUserIdObservable().Subscribe(myUserId => _myUserId = myUserId);
    }

    public ObservableRangeCollection<LeaderViewModel> Leaders { get; } = [];
    
    public List<Segment> Periods { get; set; } = [
        new() { Name = "Week", Value = LeaderboardFilter.ThisWeek },
        new() { Name = "Month", Value = LeaderboardFilter.ThisMonth },
        new() { Name = "Year", Value = LeaderboardFilter.ThisYear },
        new() { Name = "All Time", Value = LeaderboardFilter.Forever }
    ];

    [ObservableProperty]
    private bool _isRunning;

    [ObservableProperty]
    private bool _isRefreshing;

    public Action<int> ScrollTo { get; set; }

    private LeaderboardFilter CurrentPeriod { get; set; } = LeaderboardFilter.ThisWeek;

    [ObservableProperty]
    private Segment _selectedPeriod;

    [ObservableProperty]
    private LeaderViewModel _first;

    [ObservableProperty]
    private LeaderViewModel _second;

    [ObservableProperty]
    private LeaderViewModel _third;

    public async Task Initialise()
    {
        if (!_loaded)
        {
            IsRunning = true;
            var leaders = await FetchLeaders();
            ProcessLeaders(leaders);
            _loaded = true;
            IsRunning = false;
        }
    }

    [RelayCommand]
    private async Task RefreshLeaderboard()
    {
        IsRefreshing = false;
        var leaders = await FetchLeaders();
        ProcessLeaders(leaders);
    }

    [RelayCommand]
    private async Task LoadMore()
    {
        if (!_loaded) return;
        if (_limitReached) return;
        _skip += Take;
        var leaders = await FetchLeaders();
        if (leaders.Count == 0)
        {
            _limitReached = true;
            return;
        }
        Leaders.AddRange(leaders);
    }

    [RelayCommand]
    private async Task FilterByPeriod()
    {
        CurrentPeriod = (LeaderboardFilter)SelectedPeriod.Value;
        var leaders = await FetchLeaders();
        ProcessLeaders(leaders);
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
    private async Task ScrollToMe()
    {
        LeaderViewModel myCard = null;
        IsRunning = true;

        while (myCard == null)
        {
            myCard = Leaders.FirstOrDefault(l => l.IsMe);

            if (myCard != null)
            {
                continue;
            }

            await LoadMore();

            if (_limitReached)
            {
                break;
            }
        }

        if (myCard != null)
        {
            var myIndex = Leaders.IndexOf(myCard);
            ScrollTo(myIndex);
        }
        
        IsRunning = false;
    }

    private async Task LoadLeaderboard()
    {
        _limitReached = false;
        _skip = 0;
        var leaders = await FetchLeaders();
        ProcessLeaders(leaders);
        _loaded = true;
    }
    
    private async Task<List<LeaderViewModel>> FetchLeaders()
    {
        var cacheKey = $"leaderboard_{CurrentPeriod}_{_skip}_{Take}";
        List<LeaderViewModel> leaderViewModels = new();
        await _fileCacheService.FetchAndRefresh(
            cacheKey,
            async () =>
            {
                var rank = _skip + 1;
                var leaders = await _leaderService.GetLeadersAsync(false, _skip, Take, CurrentPeriod);
                var vms = new List<LeaderViewModel>();
                foreach (var leader in leaders)
                {
                    var isMe = _myUserId == leader.UserId;
                    var vm = CreateLeaderViewModel(leader, isMe, rank);
                    rank++;
                    vms.Add(vm);
                }
                return vms;
            },
            (result, isFromCache) =>
            {
                if (result != null && result.Count > 0 && leaderViewModels.Count == 0)
                {
                    leaderViewModels.AddRange(result);
                }
            });
        return leaderViewModels;
    }

    private void ProcessLeaders(List<LeaderViewModel> leaders)
    {
        Leaders.ReplaceRange(leaders);
        First = Leaders.FirstOrDefault();
        Second = Leaders.Skip(1).FirstOrDefault();
        Third = Leaders.Skip(2).FirstOrDefault();
    }
    
    private LeaderViewModel CreateLeaderViewModel(LeaderboardUserDto leader, bool isMe, int rank)
    {
        return new LeaderViewModel(leader, isMe)
        {
            Rank = rank,
            DisplayPoints = CalculateDisplayPoints(leader)
        };
    }

    
    private int CalculateDisplayPoints(LeaderboardUserDto leader)
    {
        return CurrentPeriod switch
        {
            LeaderboardFilter.ThisMonth => leader.PointsThisMonth,
            LeaderboardFilter.ThisYear => leader.PointsThisYear,
            LeaderboardFilter.Forever => leader.TotalPoints,
            _ => leader.PointsThisWeek
        };
    }
}

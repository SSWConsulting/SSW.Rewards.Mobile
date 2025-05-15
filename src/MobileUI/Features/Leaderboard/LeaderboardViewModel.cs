using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SSW.Rewards.Enums;
using SSW.Rewards.Mobile.Controls;
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

    public LeaderboardViewModel(ILeaderService leaderService, IUserService userService, IServiceProvider provider)
    {
        Title = "Leaderboard";
        _leaderService = leaderService;
        _userService = userService;
        _provider = provider;
        _userService.MyUserIdObservable().Subscribe(myUserId => _myUserId = myUserId);
    }

    public ObservableCollection<LeaderViewModel> Leaders { get; } = [];
    
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
            
            await LoadLeaderboard();
            _loaded = true;

            IsRunning = false;
        }
    }

    [RelayCommand]
    private async Task RefreshLeaderboard()
    {
        await LoadLeaderboard();
        IsRefreshing = false;
    }

    [RelayCommand]
    private async Task LoadMore()
    {
        // Don't attempt to load more until initial load is complete
        if (!_loaded)
            return;

        if (_limitReached)
            return;
        
        _skip += Take;
    
        var leaders = await FetchLeaders();

        var newLeadersList = leaders.ToList();
        if (newLeadersList.Count == 0)
        {
            _limitReached = true;
            return;
        }

        AddLeadersToLeaderboard(newLeadersList);
    }

    [RelayCommand]
    private async Task FilterByPeriod()
    {
        CurrentPeriod = (LeaderboardFilter)SelectedPeriod.Value;
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
        Leaders.Clear();
        _limitReached = false;
        _skip = 0;

        var leaders = await FetchLeaders();

        AddLeadersToLeaderboard(leaders);

        // Update podium positions
        First = Leaders.FirstOrDefault();
        Second = Leaders.Skip(1).FirstOrDefault();
        Third = Leaders.Skip(2).FirstOrDefault();
        
        _loaded = true;
    }
    
    private async Task<IEnumerable<LeaderboardUserDto>> FetchLeaders()
    {
        return await _leaderService.GetLeadersAsync(
            false,
            _skip,
            Take,
            CurrentPeriod
        );
    }


    private void AddLeadersToLeaderboard(IEnumerable<LeaderboardUserDto> leaders)
    {
        var rank = Leaders.Count + 1;
        foreach (var leader in leaders)
        {
            var isMe = _myUserId == leader.UserId;
            var vm = CreateLeaderViewModel(leader, isMe, rank);
            rank++;
            Leaders.Add(vm);
        }
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

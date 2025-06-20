using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Plugin.Firebase.Crashlytics;
using SSW.Rewards.Enums;
using SSW.Rewards.Mobile.Controls;

namespace SSW.Rewards.Mobile.ViewModels;

public partial class LeaderboardViewModel : BaseViewModel
{
    private readonly ILeaderService _leaderService;
    private readonly IServiceProvider _provider;
    private readonly IFileCacheService _fileCacheService;
    private bool _loaded;

    private const int PageSize = 50;
    private int _page;
    private bool _limitReached;

    public LeaderboardViewModel(ILeaderService leaderService, IServiceProvider provider, IFileCacheService fileCacheService)
    {
        Title = "Leaderboard";
        _leaderService = leaderService;
        _provider = provider;
        _fileCacheService = fileCacheService;
    }

    public ObservableRangeCollection<LeaderViewModel> Leaders { get; } = [];
    
    public List<Segment> Periods { get; set; } = [
        new() { Name = "Week", Value = LeaderboardFilter.ThisWeek },
        new() { Name = "Month", Value = LeaderboardFilter.ThisMonth },
        new() { Name = "Year", Value = LeaderboardFilter.ThisYear },
        new() { Name = "All", Value = LeaderboardFilter.Forever }
    ];

    [ObservableProperty]
    private bool _isRunning;

    [ObservableProperty]
    private bool _isRefreshing;

    public Action<int> ScrollTo { get; set; }
    public Func<Task> ReadyEarly { get; set; }

    private LeaderboardFilter CurrentPeriod { get; set; } = LeaderboardFilter.ThisWeek;

    [ObservableProperty]
    private Segment? _selectedPeriod;

    [ObservableProperty]
    private LeaderViewModel _first;

    [ObservableProperty]
    private LeaderViewModel _second;

    [ObservableProperty]
    private LeaderViewModel _third;

    public async Task Initialise() => await UpdateLeaderboardByAction(LeaderboardAction.InitialLoad);

    [RelayCommand]
    private async Task RefreshLeaderboard() => await UpdateLeaderboardByAction(LeaderboardAction.ManualRefresh);

    [RelayCommand]
    private async Task LoadMore() => await UpdateLeaderboardByAction(LeaderboardAction.LoadMore);

    [RelayCommand]
    private async Task ScrollToMe() => await UpdateLeaderboardByAction(LeaderboardAction.ScrollToMe);

    [RelayCommand]
    private async Task FilterByPeriod()
    {
        if (SelectedPeriod == null)
            return;

        CurrentPeriod = (LeaderboardFilter)SelectedPeriod.Value;

        await UpdateLeaderboardByAction(LeaderboardAction.FullRefresh);
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

    private async Task UpdateLeaderboardByAction(LeaderboardAction leaderboardAction)
    {
        if (leaderboardAction is LeaderboardAction.InitialLoad && _loaded)
        {
            // Should only happen on the first load.
            return;
        }

        if (leaderboardAction is LeaderboardAction.ManualRefresh)
        {
            // IsRefreshing is controlled by the RefreshView, we only need to set it to false here on manual refresh.
            IsRefreshing = false;
        }

        if (leaderboardAction is LeaderboardAction.LoadMore)
        {
            if (_limitReached || !_loaded)
            {
                // Not yet loaded or limit reached.
                return;
            }

            ++_page;
        }

        IsRunning = true;

        if (ShouldDoFullRefresh(leaderboardAction))
        {
            // Manual refresh, changing period or first load.
            _page = 0;
            _limitReached = false;
        }

        try
        {
            if (leaderboardAction is LeaderboardAction.InitialLoad)
            {
                // Load from cache on first load.
                var cacheKey = $"leaderboard_{CurrentPeriod}_{_page}_{PageSize}";

                await _fileCacheService.FetchAndRefresh(
                    cacheKey,
                    async () => await FetchLeaderboard(CurrentPeriod, _page, PageSize),
                    async (result, isFromCache, _) =>
                    {
                        ProcessLeaders(result, true);

                        await ReadyEarly();
                    });
            }
            else if (leaderboardAction is LeaderboardAction.ScrollToMe)
            {
                LeaderViewModel myCard = Leaders.FirstOrDefault(l => l.IsMe);
                while (myCard == null)
                {
                    myCard = Leaders.FirstOrDefault(l => l.IsMe);

                    if (myCard != null)
                    {
                        continue;
                    }

                    ++_page;

                    var result = await FetchLeaderboard(CurrentPeriod, _page, PageSize);
                    ProcessLeaders(result);

                    if (_limitReached)
                    {
                        break;
                    }
                }

                ScrollToCard(myCard);
            }
            else
            {
                var result = await FetchLeaderboard(CurrentPeriod, _page, PageSize);
                ProcessLeaders(result);
            }
        }
        catch (Exception ex)
        {
            CrossFirebaseCrashlytics.Current.RecordException(ex);
        }

        IsRunning = false;
        _loaded = true;
    }

    private void ScrollToCard(LeaderViewModel card)
    {
        if (card != null)
        {
            int myIndex = Leaders.IndexOf(card);
            ScrollTo(myIndex);
        }
    }

    private void ProcessLeaders(List<LeaderViewModel> leaders, bool skipRefreshIfNotChanged = false)
    {
        if (leaders.Count == 0)
        {
            _limitReached = true;
            return;
        }

        if (_page == 0)
        {
            bool shouldUpdate = true;
            if (Leaders.Count == leaders.Count && skipRefreshIfNotChanged)
            {
                // When loading data from cache, we highly likely have same data as from web.
                // This prevents refreshing page if nothing changed.
                shouldUpdate = false;
                for (int i = 0; i < Leaders.Count; i++)
                {
                    LeaderViewModel item = Leaders[i];
                    if (item.Name != leaders[i].Name || item.DisplayPoints != leaders[i].DisplayPoints)
                    {
                        shouldUpdate = true;
                        break;
                    }
                }
            }

            if (shouldUpdate)
            {
                Leaders.ReplaceRange(leaders);
                First = Leaders.FirstOrDefault();
                Second = Leaders.Skip(1).FirstOrDefault();
                Third = Leaders.Skip(2).FirstOrDefault();
            }
        }
        else
        {
            Leaders.AddRange(leaders);
        }
    }
    
    private async Task<List<LeaderViewModel>> FetchLeaderboard(LeaderboardFilter period, int page, int pageSize)
    {
        var leaders = await _leaderService.GetLeadersAsync(false, page, pageSize, period);
        _limitReached = leaders.IsLastPage;

        return leaders.Items
            .Select(x => new LeaderViewModel(x))
            .ToList();
    }

    private static bool ShouldDoFullRefresh(LeaderboardAction action)
        => action is LeaderboardAction.InitialLoad or LeaderboardAction.FullRefresh or LeaderboardAction.ManualRefresh;

    /// <summary>
    /// Add flags like "ShouldDoFullRefresh" if InitialLoad, FullRefresh or ManualRefresh flag.
    /// </summary>
    public enum LeaderboardAction
    {
        InitialLoad = 0b0000,
        FullRefresh = 0b0001,
        LoadMore = 0b0010,
        ManualRefresh = 0b0100,
        ScrollToMe = 0b1000
    }
}

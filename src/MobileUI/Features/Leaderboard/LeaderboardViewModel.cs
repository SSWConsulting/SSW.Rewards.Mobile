using System.Collections.Specialized;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SSW.Rewards.ApiClient.Paging;
using SSW.Rewards.Enums;
using SSW.Rewards.Mobile.Common;
using SSW.Rewards.Mobile.Controls;

namespace SSW.Rewards.Mobile.ViewModels;

public partial class LeaderboardViewModel : BaseViewModel
{
    private const string LeaderboardSubject = "the leaderboard";
    private const int ScrollDelayMilliseconds = 50;

    private readonly IServiceProvider _provider;
    private readonly IApiErrorPresenter _errorPresenter;
    private bool _readySignalled;

    public LeaderboardViewModel(ILeaderService leaderService, IServiceProvider provider,
        IFileCacheService fileCacheService, IApiErrorPresenter errorPresenter)
    {
        Title = "Leaderboard";
        _provider = provider;
        _errorPresenter = errorPresenter;

        Leaders = new PagedListSource<LeaderViewModel>(new()
        {
            FetchPage = async (skip, take, _) =>
            {
                var leaders = await leaderService.GetLeadersAsync(false, PageNumberPaging.ToPageIndex(skip, take), take, CurrentPeriod);
                return leaders.Items.Select(x => new LeaderViewModel(x)).ToList();
            },
            AreSame = (a, b) => a.UserId == b.UserId
                && a.Rank == b.Rank
                && a.Name == b.Name
                && a.Title == b.Title
                && a.ProfilePic == b.ProfilePic
                && a.DisplayPoints == b.DisplayPoints,
            Cache = fileCacheService,
            CacheKey = "leaderboard_ThisWeek",

            // Only the default period is cached: it's the one shown on open.
            ShouldUseCache = () => CurrentPeriod == LeaderboardFilter.ThisWeek,
        });

        Leaders.Items.CollectionChanged += OnLeadersChanged;
    }

    public PagedListSource<LeaderViewModel> Leaders { get; }

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

    public async Task Initialise()
    {
        // Re-appearing keeps the list; only a page that never loaded tries again.
        if (Leaders.HasContent)
        {
            return;
        }

        IsRunning = true;
        try
        {
            var result = await Leaders.RefreshAsync();
            await _errorPresenter.HandleLoadAsync(result, userRequestedNewData: false, LeaderboardSubject);
        }
        finally
        {
            IsRunning = false;
        }
    }

    [RelayCommand]
    private async Task RefreshLeaderboard()
    {
        try
        {
            var result = await Leaders.RefreshAsync();
            await _errorPresenter.HandleLoadAsync(result, userRequestedNewData: false, LeaderboardSubject);
        }
        finally
        {
            IsRefreshing = false;
        }
    }

    [RelayCommand]
    private async Task LoadMore()
    {
        var result = await Leaders.LoadMoreAsync();

        // Loaded leaders stay on screen, so only a session expiry interrupts scrolling.
        await _errorPresenter.HandleLoadAsync(result, userRequestedNewData: false, LeaderboardSubject);
    }

    [RelayCommand]
    private async Task ScrollToMe()
    {
        IsRunning = true;
        try
        {
            var myCard = Leaders.Items.FirstOrDefault(l => l.IsMe);
            while (myCard == null && Leaders.CanLoadMore)
            {
                var result = await Leaders.LoadMoreAsync();
                if (result.Error is not null)
                {
                    // The user asked to find themselves further down the list, so a failure is reported.
                    await _errorPresenter.HandleLoadAsync(result, userRequestedNewData: true, LeaderboardSubject);
                    return;
                }

                myCard = Leaders.Items.FirstOrDefault(l => l.IsMe);
            }

            if (myCard != null)
            {
                await Task.Delay(ScrollDelayMilliseconds);
                ScrollTo(Leaders.Items.IndexOf(myCard));
            }
        }
        finally
        {
            IsRunning = false;
        }
    }

    // Concurrent so a tap made while the previous period loads isn't dropped; the newer refresh supersedes it.
    [RelayCommand(AllowConcurrentExecutions = true)]
    private async Task FilterByPeriod()
    {
        if (SelectedPeriod == null || CurrentPeriod == (LeaderboardFilter)SelectedPeriod.Value)
            return;

        var period = (LeaderboardFilter)SelectedPeriod.Value;
        CurrentPeriod = period;

        IsRunning = true;
        var result = await Leaders.RefreshAsync(queryChanged: true);
        if (period != CurrentPeriod)
        {
            return;
        }

        IsRunning = false;

        // A failed switch never leaves the previous period on screen: it's cleared (nothing to show,
        // so the failure is reported) or replaced by this period's cached leaders (quiet, like any
        // refresh behind cached data).
        await _errorPresenter.HandleLoadAsync(result, userRequestedNewData: false, LeaderboardSubject);
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

    private void OnLeadersChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        First = Leaders.Items.ElementAtOrDefault(0);
        Second = Leaders.Items.ElementAtOrDefault(1);
        Third = Leaders.Items.ElementAtOrDefault(2);

        // Cached leaders arrive before the network: the page can show them early.
        if (!_readySignalled && Leaders.Items.Count > 0 && ReadyEarly is { } ready)
        {
            _readySignalled = true;
            _ = ready();
        }
    }
}

#nullable enable

using System.Reactive.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SSW.Rewards.ApiClient.Services;
using SSW.Rewards.Mobile.Common;
using SSW.Rewards.Mobile.Controls;
using SSW.Rewards.Shared.DTOs.ActivityFeed;
using IUserService = SSW.Rewards.Mobile.Services.IUserService;

namespace SSW.Rewards.Mobile.ViewModels;

public enum ActivityPageSegments
{
    All,
    Friends,
    Posts
}

public partial class ActivityPageViewModel : BaseViewModel, IRefreshOnReconnect
{
    private const string FeedSubject = "the activity feed";

    private readonly IUserService _userService;
    private readonly IAppNavigator _navigator;
    private readonly IApiErrorPresenter _errorPresenter;

    private ActivityPageSegments _currentSegment;

    public PagedListSource<ActivityFeedItemDto> Feed { get; }

    public PostListViewModel PostsViewModel { get; }

    public List<Segment> Segments { get; } =
    [
        new() { Name = "All", Value = ActivityPageSegments.All },
        new() { Name = "Friends", Value = ActivityPageSegments.Friends },
        new() { Name = "Posts", Value = ActivityPageSegments.Posts }
    ];

    [ObservableProperty]
    public partial Segment? SelectedSegment { get; set; }

    [ObservableProperty]
    public partial bool IsRefreshing { get; set; }

    [ObservableProperty]
    public partial bool ShowActivityFeed { get; set; }

    [ObservableProperty]
    public partial bool ShowPosts { get; set; }

    public ActivityPageViewModel(
        IActivityFeedService activityService,
        IUserService userService,
        PostListViewModel postsViewModel,
        IAppNavigator navigator,
        IApiErrorPresenter errorPresenter,
        IFileCacheService fileCacheService)
    {
        _userService = userService;
        _navigator = navigator;
        _errorPresenter = errorPresenter;
        PostsViewModel = postsViewModel;
        ShowActivityFeed = true;

        Feed = new PagedListSource<ActivityFeedItemDto>(new()
        {
            FetchPage = async (skip, take, ct) =>
            {
                var result = _currentSegment == ActivityPageSegments.Friends
                    ? await activityService.GetFriendsActivities(take, skip, ct)
                    : await activityService.GetAllActivities(take, skip, ct);
                return result.Feed.ToList();
            },
            PrepareItem = item => item.PrepareForDisplay(),
            AreSame = (a, b) => a.HasSameDisplayAs(b),
            Cache = fileCacheService,
            CacheKey = "ActivityFeedCache",
            ShouldUseCache = () => _currentSegment == ActivityPageSegments.All,
        });
    }

    public async Task LoadFeed()
    {
        // The feed is hidden behind the Posts tab (switching back refreshes it); retry the posts
        // instead, so returning from a post detail replaces cached posts once back online.
        if (_currentSegment == ActivityPageSegments.Posts)
        {
            await PostsViewModel.InitialiseAsync();
            return;
        }

        var result = await RefreshFeedAsync();

        // Background refresh: a failure behind cached content stays quiet.
        await _errorPresenter.HandleLoadAsync(result, userRequestedNewData: false, FeedSubject);
    }

    private int _feedLoads;

    private async Task<ListLoadResult> RefreshFeedAsync(bool queryChanged = false)
    {
        // The spinner covers an empty feed only; cached items render straight away. Counted, so a
        // superseded load finishing first doesn't hide it while the newer one still runs.
        _feedLoads++;
        IsBusy = Feed.Items.Count == 0;
        try
        {
            return await Feed.RefreshAsync(queryChanged);
        }
        finally
        {
            if (--_feedLoads == 0)
            {
                IsBusy = false;
            }
        }
    }

    public Task RefreshOnReconnectAsync()
        => ShowPosts ? PostsViewModel.RefreshCommand.ExecuteAsync(null) : LoadFeed();

    [RelayCommand]
    private async Task Refresh()
    {
        try
        {
            await LoadFeed();
        }
        finally
        {
            IsRefreshing = false;
        }
    }

    [RelayCommand]
    private async Task LoadMore()
    {
        var result = await Feed.LoadMoreAsync();

        // Loaded items stay on screen, so only a session expiry interrupts scrolling.
        await _errorPresenter.HandleLoadAsync(result, userRequestedNewData: false, FeedSubject);
    }

    // Concurrent so a tap made while the previous segment loads isn't dropped; the newer refresh supersedes it.
    [RelayCommand(AllowConcurrentExecutions = true)]
    private async Task FilterBySegment()
    {
        if (SelectedSegment == null || _currentSegment == (ActivityPageSegments)SelectedSegment.Value)
        {
            return;
        }

        var segment = (ActivityPageSegments)SelectedSegment.Value;
        _currentSegment = segment;
        ShowPosts = _currentSegment == ActivityPageSegments.Posts;
        ShowActivityFeed = !ShowPosts;

        if (ShowPosts)
        {
            await PostsViewModel.InitialiseAsync();
            return;
        }

        var result = await RefreshFeedAsync(queryChanged: true);
        if (segment != _currentSegment)
        {
            return;
        }

        // The user asked for different data — report a failure even though the
        // previous segment's items are still visible.
        await _errorPresenter.HandleLoadAsync(result, userRequestedNewData: true, FeedSubject);
    }

    [RelayCommand]
    private async Task ActivityTapped(ActivityFeedItemDto item)
    {
        // Read on demand: a subscription would tie this transient VM to the singleton UserService.
        var myUserId = await _userService.MyUserIdObservable().FirstAsync();
        if (myUserId == item.UserId)
        {
            await _navigator.GoToMyProfileAsync();
        }
        else
        {
            await _navigator.GoToUserProfileAsync(item.UserId);
        }
    }

    [RelayCommand]
    private Task ClosePage() => _navigator.PopModalAsync();
}

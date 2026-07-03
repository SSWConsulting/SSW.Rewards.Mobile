#nullable enable

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

public partial class ActivityPageViewModel : BaseViewModel
{
    private const string FeedTitle = "Activity Feed";
    private const string OfflineMessage = "You're offline. The activity feed will load once you're back online.";
    private const string GenericMessage = "There seems to be a problem loading the activity feed. Please try again soon.";

    private readonly IAppNavigator _navigator;
    private readonly OfflineAwareListErrorHandler _errorHandler;

    private ActivityPageSegments _currentSegment;
    private int _myUserId;
    private bool _loaded;

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
        OfflineAwareListErrorHandler errorHandler,
        IFileCacheService fileCacheService)
    {
        _navigator = navigator;
        _errorHandler = errorHandler;
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
            AreSame = (a, b) => a.UserId == b.UserId && a.AwardedAt == b.AwardedAt,
            Cache = fileCacheService,
            CacheKey = "ActivityFeedCache",
            ShouldUseCache = () => _currentSegment == ActivityPageSegments.All,
        });

        userService.MyUserIdObservable().Subscribe(myUserId => _myUserId = myUserId);
    }

    public async Task LoadFeed()
    {
        var result = await Feed.RefreshAsync();
        _loaded = true;

        // Background refresh: a failure behind cached content stays quiet.
        await _errorHandler.HandleAsync(result, userRequestedNewData: false, FeedTitle, OfflineMessage, GenericMessage);
    }

    [RelayCommand]
    private async Task Refresh()
    {
        await LoadFeed();
        IsRefreshing = false;
    }

    [RelayCommand]
    private Task LoadMore() => Feed.LoadMoreAsync();

    [RelayCommand]
    private async Task FilterBySegment()
    {
        if (!_loaded || SelectedSegment == null || _currentSegment == (ActivityPageSegments)SelectedSegment.Value)
        {
            return;
        }

        _currentSegment = (ActivityPageSegments)SelectedSegment.Value;
        ShowPosts = _currentSegment == ActivityPageSegments.Posts;
        ShowActivityFeed = !ShowPosts;

        if (ShowPosts)
        {
            await PostsViewModel.InitialiseAsync();
            return;
        }

        var result = await Feed.RefreshAsync();

        // The user asked for different data — report a failure even though the
        // previous segment's items are still visible.
        await _errorHandler.HandleAsync(result, userRequestedNewData: true, FeedTitle, OfflineMessage, GenericMessage);
    }

    [RelayCommand]
    private async Task ActivityTapped(ActivityFeedItemDto item)
    {
        if (_myUserId == item.UserId)
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

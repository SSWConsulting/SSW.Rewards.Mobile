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
    private readonly IServiceProvider _serviceProvider;
    private readonly IAlertService _alertService;

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
        IServiceProvider serviceProvider,
        PostListViewModel postsViewModel,
        IAlertService alertService,
        IFileCacheService fileCacheService)
    {
        _serviceProvider = serviceProvider;
        _alertService = alertService;
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

        // A failed background refresh behind cached content stays quiet.
        if (result.Error is not null && !result.HasContent)
        {
            await ShowLoadFailedAlert(result.Error);
        }
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

        // The user asked for different data — tell them when it couldn't load,
        // even though the previous segment's items are still visible.
        if (result.Error is not null)
        {
            await ShowLoadFailedAlert(result.Error);
        }
    }

    private async Task ShowLoadFailedAlert(Exception error)
    {
        if (await ExceptionHandler.HandleApiException(error))
        {
            return;
        }

        string message = Connectivity.Current.NetworkAccess != NetworkAccess.Internet
            ? "You're offline. The activity feed will load once you're back online."
            : "There seems to be a problem loading the activity feed. Please try again soon.";
        await _alertService.DisplayAlertAsync("Activity Feed", message, "OK");
    }

    [RelayCommand]
    private async Task ActivityTapped(ActivityFeedItemDto item)
    {
        var page = _myUserId == item.UserId
            ? (Page)ActivatorUtilities.CreateInstance<MyProfilePage>(_serviceProvider)
            : ActivatorUtilities.CreateInstance<OthersProfilePage>(_serviceProvider, item.UserId);
        await Shell.Current.Navigation.PushAsync(page);
    }

    [RelayCommand]
    private static async Task ClosePage()
    {
        await Shell.Current.Navigation.PopModalAsync();
    }
}

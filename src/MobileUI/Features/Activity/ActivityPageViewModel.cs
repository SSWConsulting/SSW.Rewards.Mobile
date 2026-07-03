using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SSW.Rewards.ApiClient.Services;
using SSW.Rewards.Enums;
using SSW.Rewards.Mobile.Common;
using SSW.Rewards.Mobile.Controls;
using SSW.Rewards.Shared.DTOs.ActivityFeed;
using SSW.Rewards.Shared.DTOs.Users;
using SSW.Rewards.Shared.Utils;
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
    private readonly IActivityFeedService _activityService;
    private readonly IServiceProvider _serviceProvider;
    private readonly IAlertService _alertService;

    private ActivityPageSegments CurrentSegment { get; set; }

    public AdvancedObservableCollection<ActivityFeedItemDto> Feed { get; } = new();

    public PostListViewModel PostsViewModel { get; }

    public List<Segment> Segments { get; set; } =
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

    [ObservableProperty]
    public partial bool IsShowingCachedData { get; set; }

    private bool _loaded;

    private const int Take = 50;
    private int _skip;
    private bool _limitReached;

    // -1 = fetch failed/cancelled, otherwise the item count of the last completed network fetch.
    private int _lastNetworkFetchCount = -1;

    private int _myUserId;

    public ActivityPageViewModel(IActivityFeedService activityService, IUserService userService, IServiceProvider serviceProvider, PostListViewModel postsViewModel, IAlertService alertService, IFileCacheService fileCacheService)
    {
        _activityService = activityService;
        _serviceProvider = serviceProvider;
        _alertService = alertService;
        PostsViewModel = postsViewModel;
        ShowActivityFeed = true;

        // Cache the first page of the "All" segment only; Friends and pagination are network-only.
        Feed.InitializeInitialCaching(fileCacheService, "ActivityFeedCache", () => CurrentSegment == ActivityPageSegments.All && _skip == 0);
        Feed.CompareItems = AreSameActivity;
        Feed.OnDataReceived += OnFeedDataReceived;
        Feed.OnCollectionUpdated += OnFeedUpdated;
        Feed.OnError += OnFeedError;

        userService.MyUserIdObservable().Subscribe(myUserId => _myUserId = myUserId);
    }

    private static string GetMessage(UserAchievementDto achievement)
    {
        string name = achievement.AchievementName;
        string action;
        string scored = $"just scored {achievement.AchievementValue}pts for";

        switch (achievement.AchievementType)
        {
            case AchievementType.Attended:
                action = "checked into";
                break;

            case AchievementType.Linked:
                action = $"{scored} linking";
                name = name.Split(' ').Last();
                break;

            case AchievementType.Scanned:
                action = $"{scored} scanning";
                break;

            case AchievementType.Completed:
            default:
                action = $"{scored} completing";
                break;
        }

        action = char.ToUpper(action[0]) + action[1..];
        return $"{action} {name}";
    }

    public async Task LoadFeed()
    {
        _skip = 0;
        _limitReached = false;
        await Feed.LoadAsync(FetchPage, reload: true);
    }

    // Runs for cache and network results alike, before items reach the UI,
    // so cached entries get fresh relative timestamps and display fields.
    private void OnFeedDataReceived(List<ActivityFeedItemDto> items, bool isFromCache)
    {
        if (!isFromCache)
        {
            _lastNetworkFetchCount = items.Count;
        }

        foreach (var item in items)
        {
            item.UserAvatar = string.IsNullOrWhiteSpace(item.UserAvatar)
                ? "v2sophie"
                : item.UserAvatar;
            if (item.Achievement is not null)
            {
                item.AchievementMessage = GetMessage(item.Achievement);
            }
            item.TimeElapsed = DateTimeHelpers.GetTimeElapsed(item.AwardedAt);
            item.UserTitle = RegexHelpers.WebsiteRegex().Replace(item.UserTitle, string.Empty);
        }
    }

    private void OnFeedUpdated(List<ActivityFeedItemDto> items, bool isFromCache)
    {
        IsShowingCachedData = isFromCache;
        IsRefreshing = false;
        _loaded = true;
    }

    private bool OnFeedError(Exception ex)
    {
        IsRefreshing = false;
        _ = HandleFeedErrorAsync(ex);
        return true;
    }

    private async Task HandleFeedErrorAsync(Exception ex)
    {
        if (await ExceptionHandler.HandleApiException(ex))
        {
            return;
        }

        // A failed background refresh with cached items on screen stays quiet.
        if (Feed.Collection.Count > 0)
        {
            return;
        }

        string message = Connectivity.Current.NetworkAccess != NetworkAccess.Internet
            ? "You're offline. The activity feed will load once you're back online."
            : "There seems to be a problem loading the activity feed. Please try again soon.";
        await _alertService.DisplayAlertAsync("Activity Feed", message, "OK");
    }

    private static bool AreSameActivity(ActivityFeedItemDto a, ActivityFeedItemDto b) =>
        a.UserId == b.UserId
        && a.AwardedAt == b.AwardedAt
        && a.UserName == b.UserName
        && a.UserTitle == b.UserTitle
        && a.UserAvatar == b.UserAvatar
        && a.AchievementMessage == b.AchievementMessage;

    private async Task<List<ActivityFeedItemDto>> FetchPage(CancellationToken ct)
    {
        var result = CurrentSegment == ActivityPageSegments.Friends
            ? await _activityService.GetFriendsActivities(Take, _skip, ct)
            : await _activityService.GetAllActivities(Take, _skip, ct);
        return result.Feed.ToList();
    }

    [RelayCommand]
    private async Task LoadMore()
    {
        if (_limitReached)
            return;

        _skip += Take;
        _lastNetworkFetchCount = -1;
        await Feed.LoadAsync(FetchPage);

        if (_lastNetworkFetchCount == -1)
        {
            // Fetch failed (e.g. offline) — allow this page to be retried.
            _skip -= Take;
        }
        else if (_lastNetworkFetchCount == 0)
        {
            // Only a successful empty page marks the end of the feed.
            _limitReached = true;
        }
    }

    [RelayCommand]
    private async Task FilterBySegment()
    {
        if (!_loaded || SelectedSegment == null || CurrentSegment == (ActivityPageSegments)SelectedSegment.Value)
        {
            return;
        }

        CurrentSegment = (ActivityPageSegments)SelectedSegment.Value;

        // Update visibility based on selected segment
        ShowPosts = CurrentSegment == ActivityPageSegments.Posts;
        ShowActivityFeed = !ShowPosts;

        if (ShowPosts)
        {
            // Load posts when switching to Posts tab
            await PostsViewModel.InitialiseAsync();
        }
        else
        {
            await LoadFeed();
        }
    }

    [RelayCommand]
    private async Task Refresh()
    {
        await LoadFeed();
        IsRefreshing = false;
    }

    [RelayCommand]
    private async Task ActivityTapped(ActivityFeedItemDto item)
    {
        if (_myUserId == item.UserId)
        {
            var page = ActivatorUtilities.CreateInstance<MyProfilePage>(_serviceProvider);
            await Shell.Current.Navigation.PushAsync(page);
        }
        else
        {
            var page = ActivatorUtilities.CreateInstance<OthersProfilePage>(_serviceProvider, item.UserId);
            await Shell.Current.Navigation.PushAsync(page);
        }
    }

    [RelayCommand]
    private static async Task ClosePage()
    {
        await Shell.Current.Navigation.PopModalAsync();
    }
}

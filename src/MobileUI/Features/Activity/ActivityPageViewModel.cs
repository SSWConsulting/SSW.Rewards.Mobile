using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SSW.Rewards.ApiClient.Services;
using SSW.Rewards.Enums;
using SSW.Rewards.Mobile.Controls;
using SSW.Rewards.Shared.DTOs.ActivityFeed;
using SSW.Rewards.Shared.DTOs.Users;
using SSW.Rewards.Shared.Utils;
using IUserService = SSW.Rewards.Mobile.Services.IUserService;

namespace SSW.Rewards.Mobile.ViewModels;

public enum ActivityPageSegments
{
    All,
    Friends
}

public partial class ActivityPageViewModel : BaseViewModel
{
    private readonly IActivityFeedService _activityService;
    private readonly IServiceProvider _serviceProvider;

    private ActivityPageSegments CurrentSegment { get; set; }
    
    public ObservableRangeCollection<ActivityFeedItemDto> Feed { get; set; } = [];

    public List<Segment> Segments { get; set; } =
    [
        new() { Name = "All", Value = ActivityPageSegments.All },
        new() { Name = "Friends", Value = ActivityPageSegments.Friends }
    ];

    [ObservableProperty]
    private Segment? _selectedSegment;
    
    [ObservableProperty]
    private bool _isRefreshing;

    private bool _loaded;

    private const int Take = 50;
    private int _skip;
    private bool _limitReached;
    
    private int _myUserId;

    public ActivityPageViewModel(IActivityFeedService activityService, IUserService userService, IServiceProvider serviceProvider)
    {
        _activityService = activityService;
        _serviceProvider = serviceProvider;

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
        var feed = await GetFeedData();
        
        Feed.ReplaceRange(feed);
        _loaded = true;
    }
    
    private async Task<List<ActivityFeedItemDto>> GetFeedData()
    {
        List<ActivityFeedItemDto> feed = [];
        
        try
        {
            feed = (CurrentSegment == ActivityPageSegments.Friends
                ? await _activityService.GetFriendsActivities(Take, _skip, CancellationToken.None)
                : await _activityService.GetAllActivities(Take, _skip, CancellationToken.None)).Feed.Select(x =>
            {
                x.UserAvatar = string.IsNullOrWhiteSpace(x.UserAvatar)
                    ? "v2sophie"
                    : x.UserAvatar;
                x.AchievementMessage = GetMessage(x.Achievement);
                x.TimeElapsed = DateTimeHelpers.GetTimeElapsed(x.AwardedAt);
                x.UserTitle = RegexHelpers.WebsiteRegex().Replace(x.UserTitle, string.Empty);
                return x;
            }).ToList();
        }
        catch (Exception e)
        {
            if (! await ExceptionHandler.HandleApiException(e))
            {
                await Shell.Current.DisplayAlert("Oops...", "There seems to be a problem loading the activity feed. Please try again soon.", "OK");
            }
        }

        return feed;
    }
    
    [RelayCommand]
    private async Task LoadMore()
    {
        if (_limitReached)
            return;

        _skip += Take;
        var feed = await GetFeedData();
        
        if (feed.Count == 0)
        {
            _limitReached = true;
            return;
        }
        
        Feed.AddRange(feed);
    }

    [RelayCommand]
    private async Task FilterBySegment()
    {
        if (!_loaded || SelectedSegment == null || CurrentSegment == (ActivityPageSegments)SelectedSegment.Value)
        {
            return;
        }
        
        CurrentSegment = (ActivityPageSegments)SelectedSegment.Value;
        _limitReached = false;
        _skip = 0;
        
        await LoadFeed();
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
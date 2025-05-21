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

public partial class ActivityPageViewModel(IActivityFeedService activityService, IUserService userService, IServiceProvider provider) : BaseViewModel
{
    public ActivityPageSegments CurrentSegment { get; set; }
    
    public ObservableRangeCollection<ActivityFeedItemDto> Feed { get; set; } = [];

    [ObservableProperty]
    private List<Segment> _segments = [];
    
    [ObservableProperty]
    private Segment _selectedSegment;
    
    [ObservableProperty]
    private bool _isRefreshing;

    private bool _loaded;

    private const int Take = 50;
    private int _skip;
    private bool _limitReached;
    
    private int _myUserId;

    public async Task Initialise()
    {
        if (_loaded)
            return;
        
        if (Segments.Count == 0)
        {
            Segments =
            [
                new Segment { Name = "All", Value = ActivityPageSegments.All },
                new Segment { Name = "Friends", Value = ActivityPageSegments.Friends }
            ];
        }

        userService.MyUserIdObservable().Subscribe(myUserId => _myUserId = myUserId);

        await LoadFeed();
    }
    
    private static string GetMessage(UserAchievementDto achievement)
    {
        string name = achievement.AchievementName;
        string action = string.Empty;
        string scored = $"just scored {achievement.AchievementValue}pts for";

        switch (achievement.AchievementType)
        {
            case AchievementType.Attended:
                action = "checked into";
                break;

            case AchievementType.Completed:
                action = $"{scored} completing";
                break;

            case AchievementType.Linked:
                action = $"{scored} linking";
                name = name.Split(' ').Last();
                break;

            case AchievementType.Scanned:
                action = $"{scored} scanning";
                break;
        }

        action = char.ToUpper(action[0]) + action[1..];
        return $"{action} {name}";
    }

    private async Task LoadFeed(bool isRefreshing = false)
    {
        if (!isRefreshing)
        {
            Feed.Clear();
        }
        
        IsBusy = true;
        _skip = 0;
        var feed = await GetFeedData();
        
        if (isRefreshing)
        {
            Feed.Clear();
        }

        Feed.AddRange(feed);
        IsBusy = false;
        _loaded = true;
    }
    
    private async Task<List<ActivityFeedItemDto>> GetFeedData()
    {
        List<ActivityFeedItemDto> feed = [];
        
        try
        {
            feed = (CurrentSegment == ActivityPageSegments.Friends
                ? await activityService.GetFriendsActivities(Take, _skip, CancellationToken.None)
                : await activityService.GetAllActivities(Take, _skip, CancellationToken.None)).Feed.Select(x =>
            {
                x.UserAvatar = string.IsNullOrWhiteSpace(x.UserAvatar)
                    ? "v2sophie"
                    : x.UserAvatar;
                x.AchievementMessage = GetMessage(x.Achievement);
                x.TimeElapsed = DateTimeHelpers.GetTimeElapsed(x.AwardedAt);
                x.UserTitle = RegexHelpers.TitleRegex().Replace(x.UserTitle, string.Empty);
                return x;
            }).ToList();
        }
        catch (Exception e)
        {
            if (! await ExceptionHandler.HandleApiException(e))
            {
                await App.Current.MainPage.DisplayAlert("Oops...", "There seems to be a problem loading the activity feed. Please try again soon.", "OK");
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
        if (!_loaded || CurrentSegment == (ActivityPageSegments)SelectedSegment.Value)
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
        await LoadFeed(true);
        IsRefreshing = false;
    }
    
    [RelayCommand]
    private async Task ActivityTapped(ActivityFeedItemDto item)
    {
        if (_myUserId == item.UserId)
        {
            var page = ActivatorUtilities.CreateInstance<MyProfilePage>(provider);
            await Shell.Current.Navigation.PushAsync(page);
        }
        else
        {
            var page = ActivatorUtilities.CreateInstance<OthersProfilePage>(provider, item.UserId);
            await Shell.Current.Navigation.PushAsync(page);
        }
    }
    
    [RelayCommand]
    private async Task ClosePage()
    {
        await App.Current.MainPage.Navigation.PopModalAsync();
    }
}
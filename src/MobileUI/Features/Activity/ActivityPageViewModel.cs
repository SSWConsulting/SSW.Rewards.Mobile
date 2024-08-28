using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SSW.Rewards.ApiClient.Services;
using SSW.Rewards.Enums;
using SSW.Rewards.Mobile.Controls;
using SSW.Rewards.Shared.DTOs.ActivityFeed;
using SSW.Rewards.Shared.DTOs.Users;
using IUserService = SSW.Rewards.Mobile.Services.IUserService;

namespace SSW.Rewards.Mobile.ViewModels;

public enum ActivityPageSegments
{
    All,
    Friends
}

public partial class ActivityPageViewModel(IActivityFeedService activityService, IUserService userService) : BaseViewModel
{
    public ActivityPageSegments CurrentSegment { get; set; }

    [ObservableProperty]
    private ObservableCollection<ActivityFeedItemDto> _feed = [];

    [ObservableProperty]
    private List<Segment> _segments = [];
    
    [ObservableProperty]
    private Segment _selectedSegment;
    
    [ObservableProperty]
    private bool _isRefreshing;

    private bool _loaded;

    private List<ActivityFeedItemDto> _allFeed = [];
    
    private List<ActivityFeedItemDto> _friendsFeed = [];
    private bool _friendsLoaded;

    private const int _take = 50;
    private int _skip;
    private bool _limitReached;
    
    private int _myUserId;

    public async Task Initialise()
    {
        if (_loaded)
            return;
        
        IsBusy = true;
        if (Segments.Count == 0)
        {
            Segments =
            [
                new Segment { Name = "All", Value = ActivityPageSegments.All },
                new Segment { Name = "Friends", Value = ActivityPageSegments.Friends }
            ];
        }

        userService.MyUserIdObservable().Subscribe(myUserId => _myUserId = myUserId);

        await RefreshFeed();
        
        IsBusy = false;
        _loaded = true;
    }
    
    private string GetMessage(UserAchievementDto achievement)
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

        action = char.ToUpper(action[0]) + action.Substring(1);
        return $"{action} {name}";
    }

    private async Task RefreshFeed()
    {
        var feed = await GetFeedData();
        _skip = 0;

        if (CurrentSegment == ActivityPageSegments.Friends)
        {
            _friendsFeed = feed;
            _friendsLoaded = true;
        }
        else
        {
            _allFeed = feed;
        }

        LoadFeed();
    }

    private void LoadFeed()
    {
        Feed.Clear();
        
        if (CurrentSegment == ActivityPageSegments.Friends)
        {
            foreach (var f in _friendsFeed)
            {
                Feed.Add(f);
            }
        }
        else
        {
            foreach (var f in _allFeed)
            {
                Feed.Add(f);
            }
        }
    }
    
    private async Task<List<ActivityFeedItemDto>> GetFeedData()
    {
        List<ActivityFeedItemDto> feed = [];
        
        try
        {
            feed = (CurrentSegment == ActivityPageSegments.Friends
                ? await activityService.GetFriendsActivities(_take, _skip, CancellationToken.None)
                : await activityService.GetAllActivities(_take, _skip, CancellationToken.None)).Feed.Select(x =>
            {
                x.UserAvatar = string.IsNullOrWhiteSpace(x.UserAvatar)
                    ? "v2sophie"
                    : x.UserAvatar;
                x.AchievementMessage = GetMessage(x.Achievement);
                x.TimeElapsed = DateTimeHelpers.GetTimeElapsed(x.AwardedAt);
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

        _skip += _take;
        IsBusy = true;
        var feed = await GetFeedData();
        
        if (feed.Count == 0)
        {
            _limitReached = true;
            IsBusy = false;
            return;
        }
        
        foreach (var f in feed)
        {
            Feed.Add(f);
        }

        IsBusy = false;
    }

    [RelayCommand]
    private async Task FilterBySegment()
    {
        CurrentSegment = (ActivityPageSegments)SelectedSegment.Value;
        _limitReached = false;
        _skip = 0;
        
        if (CurrentSegment == ActivityPageSegments.Friends && !_friendsLoaded)
        {
            await RefreshFeed();
            return;
        }
        
        LoadFeed();
    }
    
    [RelayCommand]
    private async Task Refresh()
    {
        await RefreshFeed();
        IsRefreshing = false;
    }
    
    [RelayCommand]
    private async Task ActivityTapped(ActivityFeedItemDto item)
    {
        if (_myUserId == item.UserId)
            await Shell.Current.Navigation.PushModalAsync<MyProfilePage>();
        else
            await Shell.Current.Navigation.PushModalAsync<OthersProfilePage>(item.UserId);
    }
}
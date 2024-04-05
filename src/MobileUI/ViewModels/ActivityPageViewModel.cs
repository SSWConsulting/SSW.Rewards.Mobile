using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Maui.Core.Extensions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using SSW.Rewards.Enums;
using SSW.Rewards.Mobile.Controls;
using SSW.Rewards.Shared.DTOs.ActivityFeed;
using SSW.Rewards.Shared.DTOs.Users;

namespace SSW.Rewards.Mobile.ViewModels;

public enum ActivityPageSegments
{
    All,
    Friends
}

public partial class ActivityPageViewModel(IActivityService activityService) : BaseViewModel
{
    public ActivityPageSegments CurrentSegment { get; set; }

    [ObservableProperty]
    private ObservableCollection<ActivityFeedViewModel> _feed = [];

    [ObservableProperty]
    private List<Segment> _segments = [];
    
    [ObservableProperty]
    private Segment _selectedSegment;
    
    [ObservableProperty]
    private bool _isRefreshing;

    private bool _loaded;

    private IEnumerable<ActivityFeedViewModel> _allFeed = [];
    
    private IEnumerable<ActivityFeedViewModel> _friendsFeed = [];
    private bool _friendsLoaded;

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
                action = $"{scored} following";
                name = name.Replace("follow", "");
                name = name.Replace("Follow", "").Trim();
                break;

            case AchievementType.Scanned:
                action = $"{scored} scanning";
                break;
        }

        action = char.ToUpper(action[0]) + action.Substring(1);
        return $"{action} {name}";
    }
    
    private static string GetTimeElapsed(DateTime occurredAt)
    {
        return (DateTime.Now - occurredAt) switch
        {
            { TotalMinutes: < 5 } ts => "Just now",
            { TotalHours: < 1 } ts => $"{ts.Minutes}m ago",
            { TotalDays: < 1 } ts => $"{ts.Hours}h ago",
            { TotalDays: < 31 } ts => $"{ts.Days}d ago",
            _ => occurredAt.ToString("dd MMMM yyyy"),
        };
    }

    private async Task RefreshFeed()
    {
        var feed = (CurrentSegment == ActivityPageSegments.Friends
            ? await activityService.GetFriendsFeed()
            : await activityService.GetActivityFeed())?.Select(x =>
        {
            x.UserAvatar = string.IsNullOrWhiteSpace(x.UserAvatar)
                ? "v2sophie"
                : x.UserAvatar;
            x.AchievementMessage = GetMessage(x.Achievement);
            x.TimeElapsed = GetTimeElapsed(x.AwardedAt);
            return x;
        });

        if (feed is null)
            return;

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

    [RelayCommand]
    private async Task FilterBySegment()
    {
        CurrentSegment = (ActivityPageSegments)SelectedSegment.Value;
        
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
}
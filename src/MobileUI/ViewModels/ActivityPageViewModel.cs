using System.Collections.ObjectModel;
using CommunityToolkit.Maui.Core.Extensions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SSW.Rewards.ApiClient.Services;
using SSW.Rewards.Enums;
using SSW.Rewards.Mobile.Controls;
using SSW.Rewards.Shared.DTOs.ActivityFeed;

namespace SSW.Rewards.Mobile.ViewModels;

public partial class ActivityPageViewModel : BaseViewModel
{
    public ActivityFeedFilter CurrentSegment { get; set; }
    
    [ObservableProperty]
    private List<ActivityFeedViewModel> _activity;
    [ObservableProperty]
    private ObservableCollection<ActivityFeedViewModel> _searchResults;
    [ObservableProperty] 
    private List<Segment> _segments;
    [ObservableProperty]
    private Segment _selectedSegment;
    [ObservableProperty]
    private bool _isRefreshing;

    private readonly IActivityService _activityService;

    public ActivityPageViewModel(IActivityService activityService)
    {
        _activityService = activityService;
    }
    
    public async void Initialise()
    {
        IsBusy = true;
        
        if (Segments is null || Segments.Count() == 0)
        {
            Segments = new List<Segment>
            {
                new() { Name = "Friends", Value = ActivityFeedFilter.Friends },
                new() { Name = "All", Value = ActivityFeedFilter.All }
            };
        }
        
        if(Activity is null || Activity.Count() == 0)
        {
            CurrentSegment = ActivityFeedFilter.Friends;
            await LoadActivities();
        }
        
        IsBusy = false;
    }

    [RelayCommand]
    private async Task FilterBySegment()
    {
        CurrentSegment = (ActivityFeedFilter)SelectedSegment.Value;
        
        if (Activity is null || Activity.Count() == 0)
        {
            await GetActivity();
        }
        
        switch (CurrentSegment)
        {
            case ActivityFeedFilter.All:
            case ActivityFeedFilter.Friends:
            default:
                SearchResults = Activity.ToObservableCollection();
                break;
        }
    }
    
    [RelayCommand]
    private async Task Refresh()
    {
        await LoadActivities();
        IsRefreshing = false;
    }
    
    private async Task GetActivity()
    {
        var activity = await _activityService.GetActivityFeed(new CancellationToken());
        Activity = activity.ToList();
    }
    
    private async Task LoadActivities()
    {
        await GetActivity();
        await FilterBySegment();
    }
}
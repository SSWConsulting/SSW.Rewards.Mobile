using System.Collections.ObjectModel;
using CommunityToolkit.Maui.Core.Extensions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SSW.Rewards.Mobile.Controls;
using SSW.Rewards.Shared.DTOs.Users;

namespace SSW.Rewards.Mobile.ViewModels;

public enum NetworkPageSegments
{
    Friends,
    ToMeet,
    SSW,
    Other
}

public partial class NetworkPageViewModel : BaseViewModel
{
    public NetworkPageSegments CurrentSegment { get; set; }

    [ObservableProperty]
    private List<NetworkProfileDto> _profiles;
    [ObservableProperty] 
    private List<Segment> _segments;
    [ObservableProperty]
    private Segment _selectedSegment;
    [ObservableProperty]
    private ObservableCollection<NetworkProfileDto> _searchResults;
    [ObservableProperty]
    private bool _isRefreshing;

    private IDevService _devService;
    
    public NetworkPageViewModel(IDevService devService)
    {
        _devService = devService;
    }
    
    public async Task Initialise()
    {
        IsBusy = true;
        if (Segments is null || Segments.Count() == 0)
        {
            Segments = new List<Segment>
            {
                new() { Name = "Friends", Value = NetworkPageSegments.Friends },
                new() { Name = "To Meet", Value = NetworkPageSegments.ToMeet },
                new() { Name = "All", Value = NetworkPageSegments.SSW }
            };
        }
        
        if(Profiles is null || Profiles.Count() == 0)
        {
            CurrentSegment = NetworkPageSegments.Friends;
            await LoadNetwork();
        }

        IsBusy = false;
    }

    private async Task GetProfiles()
    {
        var profiles = await _devService.GetProfilesAsync();
        Profiles = profiles.ToList();
    }

    [RelayCommand]
    private async Task FilterBySegment()
    {
        CurrentSegment = (NetworkPageSegments)SelectedSegment.Value;

        if (Profiles is null || Profiles.Count() == 0)
        {
            await GetProfiles();
        }
        
        switch (CurrentSegment)
        {
            case NetworkPageSegments.Friends:
                SearchResults = Profiles.Where(x => x.Scanned).ToObservableCollection();
                break;
            case NetworkPageSegments.ToMeet:
                SearchResults = Profiles.Where(x => !x.Scanned).ToObservableCollection();
                break;
            case NetworkPageSegments.SSW:
            case NetworkPageSegments.Other:
            default:
                SearchResults = Profiles.ToObservableCollection();
                break;
        }
    }
    
    [RelayCommand]
    private async Task UserTapped(NetworkProfileDto leader)
    { 
        await Shell.Current.Navigation.PushModalAsync<OthersProfilePage>(leader.UserId);
    }
    
    [RelayCommand]
    private async Task RefreshNetwork()
    {
        await LoadNetwork();
        IsRefreshing = false;
    }

    private async Task LoadNetwork()
    {
        var profiles = await _devService.GetProfilesAsync();
        Profiles = profiles.ToList();

        await FilterBySegment();
    }
}
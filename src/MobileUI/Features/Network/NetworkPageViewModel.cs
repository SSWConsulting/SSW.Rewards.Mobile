using System.Collections.ObjectModel;
using CommunityToolkit.Maui.Core.Extensions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SSW.Rewards.Mobile.Controls;
using SSW.Rewards.Shared.DTOs.Users;

namespace SSW.Rewards.Mobile.ViewModels;

public enum NetworkPageSegments
{
    Following,
    Followers,
    ToMeet,
}

public partial class NetworkPageViewModel : BaseViewModel
{
    public NetworkPageSegments CurrentSegment { get; set; }
    public ObservableRangeCollection<NetworkProfileDto> SearchResults { get; set; } = [];
    
    [ObservableProperty] 
    private List<Segment> _segments;
    [ObservableProperty]
    private Segment _selectedSegment;
    [ObservableProperty]
    private bool _isRefreshing;

    private List<NetworkProfileDto> _profiles = [];
    private readonly IDevService _devService;
    
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
                new() { Name = "Following", Value = NetworkPageSegments.Following },
                new() { Name = "Followers", Value = NetworkPageSegments.Followers },
                new() { Name = "To Meet", Value = NetworkPageSegments.ToMeet }
            };
        }
        
        if(_profiles.Count == 0)
        {
            CurrentSegment = NetworkPageSegments.Following;
            await LoadNetwork();
        }

        IsBusy = false;
    }

    private async Task GetProfiles()
    {
        var profiles = await _devService.GetProfilesAsync();
        _profiles = profiles.ToList();
    }

    [RelayCommand]
    private async Task FilterBySegment()
    {
        CurrentSegment = (NetworkPageSegments)SelectedSegment.Value;

        if (_profiles.Count == 0)
        {
            await GetProfiles();
        }
        
        switch (CurrentSegment)
        {
            case NetworkPageSegments.Following:
                SearchResults.ReplaceRange(_profiles.Where(x => x.Scanned));
                break;
            case NetworkPageSegments.Followers:
                SearchResults.ReplaceRange(_profiles.Where(x => x.ScannedMe));
                break;
            case NetworkPageSegments.ToMeet:
            default:
                SearchResults.ReplaceRange(Profiles.Where(x => x.IsStaff && !x.Scanned));
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
        _profiles = profiles.ToList();

        await FilterBySegment();
    }
}
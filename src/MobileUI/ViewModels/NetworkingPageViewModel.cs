using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SSW.Rewards.Mobile.Controls;
using SSW.Rewards.Shared.DTOs.Users;

namespace SSW.Rewards.Mobile.ViewModels;

public enum NetworkingPageSegments
{
    Friends,
    ToMeet,
    SSW,
    Other
}

public partial class NetworkingPageViewModel : BaseViewModel
{
    public NetworkingPageSegments CurrentSegment { get; set; }

    [ObservableProperty]
    private List<NetworkingProfileDto> _profiles;
    [ObservableProperty] 
    private List<Segment> _segments;
    [ObservableProperty]
    private Segment _selectedSegment;
    [ObservableProperty]
    private ObservableCollection<NetworkingProfileDto> _searchResults = new ();

    private IDevService _devService;
    
    public NetworkingPageViewModel(IDevService devService)
    {
        _devService = devService;
    }
    
    public async Task Initialise()
    {
        if (Segments is null || Segments.Count() == 0)
        {
            Segments = new List<Segment>
            {
                new() { Name = "Friends", Value = NetworkingPageSegments.Friends },
                new() { Name = "To Meet", Value = NetworkingPageSegments.ToMeet },
                new() { Name = "SSW", Value = NetworkingPageSegments.SSW }
                // new() { Name = "This Year", Value = NetworkingPageSegments.Other },
            };
        }
        
        if(Profiles is null || Profiles.Count() == 0)
        {
            var profiles = await _devService.GetProfilesAsync();
            Profiles = profiles.ToList();
            CurrentSegment = NetworkingPageSegments.Friends;
            SearchResults = new ObservableCollection<NetworkingProfileDto>(Profiles.Where(x => x.Scanned));
        }
    }

    [RelayCommand]
    private async Task FilterBySegment()
    {
        CurrentSegment = (NetworkingPageSegments)SelectedSegment.Value;

        if (Profiles is null || Profiles.Count() == 0)
        {
            var profiles = await _devService.GetProfilesAsync();
            Profiles = profiles.ToList();
        }
        
        switch (CurrentSegment)
        {
            case NetworkingPageSegments.Friends:
                SearchResults = new ObservableCollection<NetworkingProfileDto>(Profiles.Where(x => x.Scanned));
                break;
            case NetworkingPageSegments.ToMeet:
                SearchResults = new ObservableCollection<NetworkingProfileDto>(Profiles.Where(x => !x.Scanned));
                break;
            case NetworkingPageSegments.SSW:
            default:
                SearchResults = new ObservableCollection<NetworkingProfileDto>(Profiles);
                break;
        }
    }
    
    // TODO: Implement Navigation to OthersProfilePage with NetworkingProfileDto
    [RelayCommand]
    private async Task UserTapped(NetworkingProfileDto  leader)
    { 
        await Shell.Current.Navigation.PushModalAsync<OthersProfilePage>(leader);
    }
}
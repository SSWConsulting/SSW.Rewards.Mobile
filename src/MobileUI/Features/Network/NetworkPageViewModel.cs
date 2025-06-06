using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SSW.Rewards.Mobile.Common;
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
    //public ObservableRangeCollection<NetworkProfileDto> SearchResults { get; set; } = [];
    public AdvancedObservableCollection<NetworkProfileDto> AdvancedSearchResults { get; set; } = new();

    [ObservableProperty] 
    private List<Segment> _segments;
    [ObservableProperty]
    private Segment _selectedSegment;
    [ObservableProperty]
    private bool _isRefreshing;

    //private List<NetworkProfileDto> _profiles = [];
    private readonly IFileCacheService _fileCacheService;
    private readonly IDevService _devService;
    private readonly IServiceProvider _provider;
    
    public NetworkPageViewModel(IFileCacheService fileCacheService, IDevService devService, IServiceProvider provider)
    {
        _fileCacheService = fileCacheService;
        _devService = devService;
        _provider = provider;
    }
    
    public async Task Initialise()
    {
        IsBusy = true;
        if (Segments is null || Segments.Count == 0)
        {
            Segments =
            [
                new() { Name = "Scanned", Value = NetworkPageSegments.Following },
                new() { Name = "Scanned me", Value = NetworkPageSegments.Followers },
                new() { Name = "Valuable", Value = NetworkPageSegments.ToMeet }
            ];

            AdvancedSearchResults.InitializeInitialCaching(_fileCacheService, "NetworkProfilesCache");

            // Always use cache first because all tabs have same endpoint but different local filtering logic.
            AdvancedSearchResults.OnUseCache += () => true;

            // Tabs have different filtering logic instead of different endpoints or parameters.
            AdvancedSearchResults.OnFilterItem += x =>
                CurrentSegment switch
                {
                    NetworkPageSegments.Following => x.Scanned,
                    NetworkPageSegments.Followers => x.ScannedMe,
                    NetworkPageSegments.ToMeet => x.IsStaff && !x.Scanned,
                    _ => false
                };

            // Disable refreshing when done.
            AdvancedSearchResults.OnDataReceived += (_, _) => IsRefreshing = false;

            // This is to reduce flickering when loading data.
            AdvancedSearchResults.OnCompareItems += NetworkProfileDto.AreIndentical;
        }

        await RefreshNetwork();

        //if (_profiles.Count == 0)
        //{
        //    CurrentSegment = NetworkPageSegments.Following;
        //    await LoadNetwork();
        //}

        IsBusy = false;
    }

    [RelayCommand]
    private async Task FilterBySegment()
    {
        CurrentSegment = (NetworkPageSegments)SelectedSegment.Value;

        await RefreshNetwork();

        //if (_profiles.Count == 0)
        //{
        //    await GetProfiles();
        //}
        
        //switch (CurrentSegment)
        //{
        //    case NetworkPageSegments.Following:
        //        SearchResults.ReplaceRange(_profiles.Where(x => x.Scanned));
        //        break;
        //    case NetworkPageSegments.Followers:
        //        SearchResults.ReplaceRange(_profiles.Where(x => x.ScannedMe));
        //        break;
        //    case NetworkPageSegments.ToMeet:
        //    default:
        //        SearchResults.ReplaceRange(_profiles.Where(x => x.IsStaff && !x.Scanned).OrderByDescending(x => x.Value));
        //        break;
        //}
    }
    
    [RelayCommand]
    private async Task UserTapped(NetworkProfileDto leader)
    {
        var page = ActivatorUtilities.CreateInstance<OthersProfilePage>(_provider, leader.UserId);
        await Shell.Current.Navigation.PushAsync(page);
    }
    
    [RelayCommand]
    private async Task RefreshNetwork()
    {
        await AdvancedSearchResults.LoadAsync(LoadData, true);

        //await LoadNetwork();
        //IsRefreshing = false;
    }

    //private async Task LoadNetwork()
    //{
    //    var profiles = await _devService.GetProfilesAsync();
    //    _profiles = profiles.ToList();

    //    await FilterBySegment();
    //}

    private async Task<List<NetworkProfileDto>> LoadData(CancellationToken ct)
    {
        IEnumerable<NetworkProfileDto> profiles = await _devService.GetProfilesAsync();
        return profiles.ToList();
    }
}
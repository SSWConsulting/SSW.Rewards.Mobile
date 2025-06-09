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
    public AdvancedObservableCollection<NetworkProfileDto> AdvancedSearchResults { get; set; } = new();

    [ObservableProperty] 
    private List<Segment> _segments;
    [ObservableProperty]
    private Segment _selectedSegment;
    [ObservableProperty]
    private bool _isRefreshing;

    private readonly IFileCacheService _fileCacheService;
    private readonly IDevService _devService;
    private readonly IServiceProvider _provider;

    private bool _pageLoaded;
    
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

            // Always use cache first because all tabs have same endpoint but different local filtering logic.
            AdvancedSearchResults.InitializeInitialCaching(_fileCacheService, "NetworkProfilesCache", () => true);

            // Tabs have different filtering logic instead of different endpoints or parameters.
            AdvancedSearchResults.FilterItem = x =>
                CurrentSegment switch
                {
                    NetworkPageSegments.Following => x.Scanned,
                    NetworkPageSegments.Followers => x.ScannedMe,
                    NetworkPageSegments.ToMeet => x.IsStaff && !x.Scanned,
                    _ => false
                };

            // Disable refreshing when done.
            AdvancedSearchResults.OnCollectionUpdated += (_, _) => IsBusy = IsRefreshing = false;

            // This is to reduce flickering when loading data.
            AdvancedSearchResults.CompareItems = NetworkProfileDto.IsEqual;

            // Handle errors silently, e.g., log them or show a message.
            AdvancedSearchResults.OnError += ex =>
                {
                    IsRefreshing = IsBusy = false;
                    return true;
                };
        }

        _pageLoaded = true;

        await RefreshNetwork();
    }

    [RelayCommand]
    private void FilterBySegment()
    {
        CurrentSegment = (NetworkPageSegments)SelectedSegment.Value;

        // This may trigger before the page is ready.
        if (_pageLoaded)
        {
            AdvancedSearchResults.RefreshCollectionWithOfflineFilter();
        }
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
    }

    private async Task<List<NetworkProfileDto>> LoadData(CancellationToken ct)
    {
        IEnumerable<NetworkProfileDto> profiles = await _devService.GetProfilesAsync();
        return profiles.ToList();
    }
}
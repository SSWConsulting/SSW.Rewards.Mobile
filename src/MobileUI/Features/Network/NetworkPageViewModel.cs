using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SSW.Rewards.Mobile.Common;
using SSW.Rewards.Mobile.Controls;
using SSW.Rewards.Shared.DTOs.Users;

namespace SSW.Rewards.Mobile.ViewModels;

public enum NetworkPageSegments
{
    Following,
    Followers
}

public partial class NetworkPageViewModel : BaseViewModel
{
    private const string NetworkSubject = "your network";

    private readonly IServiceProvider _provider;
    private readonly IApiErrorPresenter _errorPresenter;

    public NetworkPageSegments CurrentSegment { get; set; }

    /// <summary>One list from one endpoint; the segments only filter it locally.</summary>
    public PagedListSource<NetworkProfileDto> Profiles { get; }

    public List<Segment> Segments { get; } =
    [
        new() { Name = "Scanned", Value = NetworkPageSegments.Following },
        new() { Name = "Scanned me", Value = NetworkPageSegments.Followers },
    ];

    [ObservableProperty]
    private Segment? _selectedSegment;

    [ObservableProperty]
    private bool _isRefreshing;

    public NetworkPageViewModel(IFileCacheService fileCacheService, IDevService devService, IServiceProvider provider,
        IApiErrorPresenter errorPresenter)
    {
        _provider = provider;
        _errorPresenter = errorPresenter;

        Profiles = new PagedListSource<NetworkProfileDto>(new()
        {
            FetchPage = async (_, _, _) => (await devService.GetProfilesAsync()).ToList(),
            PageSize = PagedListSourceOptions<NetworkProfileDto>.SinglePage,
            Filter = profile => CurrentSegment switch
            {
                NetworkPageSegments.Following => profile.Scanned,
                NetworkPageSegments.Followers => profile.ScannedMe,
                _ => false
            },
            AreSame = NetworkProfileDto.IsEqual,
            Cache = fileCacheService,
            CacheKey = "NetworkProfilesCache",
        });

        // Cached profiles land before the network: drop the spinner then.
        Profiles.Items.CollectionChanged += (_, _) => IsBusy &= !Profiles.HasContent;
    }

    public async Task Initialise()
    {
        IsBusy = !Profiles.HasContent;
        try
        {
            await LoadAsync();
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task LoadAsync()
    {
        var result = await Profiles.RefreshAsync();

        // Quiet behind cached profiles, except a 401, which still goes to re-login.
        await _errorPresenter.HandleLoadAsync(result, userRequestedNewData: false, NetworkSubject);
    }

    [RelayCommand]
    private void FilterBySegment()
    {
        if (SelectedSegment == null)
            return;

        CurrentSegment = (NetworkPageSegments)SelectedSegment.Value;
        Profiles.ApplyFilter();
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
        try
        {
            await LoadAsync();
        }
        finally
        {
            IsRefreshing = false;
        }
    }
}

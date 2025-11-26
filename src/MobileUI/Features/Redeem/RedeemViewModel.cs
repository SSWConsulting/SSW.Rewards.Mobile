using System.Reactive.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mopups.Services;
using SSW.Rewards.ApiClient.Services;
using SSW.Rewards.Mobile.PopupPages;
using SSW.Rewards.Mobile.Common;
using IRewardService = SSW.Rewards.Mobile.Services.IRewardService;
using IUserService = SSW.Rewards.Mobile.Services.IUserService;
using System.Reactive.Subjects;

namespace SSW.Rewards.Mobile.ViewModels;

public partial class RedeemViewModel : BaseViewModel
{
    private readonly IRewardService _rewardService;
    private readonly IUserService _userService;
    private readonly IAddressService _addressService;
    private readonly IFirebaseAnalyticsService _firebaseAnalyticsService;
    private readonly IFileCacheService _fileCacheService;
    private readonly IDispatcherTimer _timer;
    private readonly Subject<string> _searchSubject = new();

    private const int AutoScrollInterval = 6;
    private const int DebounceInterval = 300;
    private const string CacheKey = "RewardsList";

    public AdvancedObservableCollection<Reward> Rewards { get; } = new();
    public ObservableRangeCollection<Reward> CarouselRewards { get; set; } = [];

    [ObservableProperty]
    private int _credits;

    [ObservableProperty]
    private bool _isRefreshing;

    [ObservableProperty]
    private int _carouselPosition;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private bool _isSearching;

    public RedeemViewModel(IRewardService rewardService, IUserService userService, IAddressService addressService,
        IFirebaseAnalyticsService firebaseAnalyticsService, IFileCacheService fileCacheService)
    {
        Title = "Rewards";
        _rewardService = rewardService;
        _userService = userService;
        _addressService = addressService;
        _firebaseAnalyticsService = firebaseAnalyticsService;
        _fileCacheService = fileCacheService;

        _userService.MyBalanceObservable().Subscribe(OnBalanceChanged);
        _userService.MyUserIdObservable().DistinctUntilChanged().Subscribe(OnUserChanged);

        _timer = Application.Current.Dispatcher.CreateTimer();
        _timer.Interval = TimeSpan.FromSeconds(AutoScrollInterval);

        // Set up reactive search with debouncing
        _searchSubject
            .DistinctUntilChanged()
            .Throttle(TimeSpan.FromMilliseconds(DebounceInterval))
            .ObserveOn(SynchronizationContext.Current)
            .Subscribe(_ => Rewards.RefreshCollectionWithOfflineFilter());
    }

    partial void OnSearchTextChanged(string value)
    {
        _searchSubject.OnNext(value);
    }

    public void OnDisappearing()
    {
        _timer.Stop();
        _timer.Tick -= OnScrollTick;
        Rewards.OnCollectionUpdated -= OnRewardsUpdated;
        Rewards.OnError -= OnRewardsError;
    }

    private void OnUserChanged(int userId)
    {
        Rewards.Reset();
        CarouselRewards.Clear();
    }

    private void OnBalanceChanged(int balance)
    {
        Credits = balance;
        UpdateRewardsAffordability();
    }

    private void UpdateRewardsAffordability()
    {
        foreach (var reward in Rewards.Collection)
        {
            reward.CanAfford = reward.Cost <= Credits;
        }

        foreach (var reward in CarouselRewards)
        {
            reward.CanAfford = reward.Cost <= Credits;
        }
    }

    public async Task Initialise()
    {
        Rewards.InitializeInitialCaching(_fileCacheService, CacheKey, () => true);
        Rewards.FilterItem = FilterReward;
        Rewards.CompareItems = Reward.IsEqual;
        Rewards.OnCollectionUpdated += OnRewardsUpdated;
        Rewards.OnError += OnRewardsError;

        if (!Rewards.IsLoaded)
        {
            await LoadData();
        }

        BeginAutoScroll();
    }

    private async Task LoadData()
    {
        if (!Rewards.IsLoaded)
            IsBusy = true;

        _timer.Stop();
        CarouselPosition = 0;

        await Rewards.LoadAsync(async ct => await FetchRewardsData(ct), reload: true);
    }

    private async Task<List<Reward>> FetchRewardsData(CancellationToken ct)
    {
        var rewards = await _rewardService.GetRewards();
        var pendingRedemptions = (await _userService.GetPendingRedemptionsAsync()).ToList();

        var rewardsList = new List<Reward>();

        foreach (var reward in rewards.Where(reward => !reward.IsHidden))
        {
            var pendingRedemption = pendingRedemptions.FirstOrDefault(x => x.RewardId == reward.Id);
            reward.CanAfford = reward.Cost <= Credits;

            if (pendingRedemption != null)
            {
                reward.IsPendingRedemption = true;
                reward.PendingRedemptionCode = pendingRedemption.Code;
            }

            rewardsList.Add(reward);
        }

        return rewardsList;
    }

    private void OnRewardsUpdated(List<Reward> rewards, bool isFromCache)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            IsBusy = false;
            IsRefreshing = false;
            _timer.Start();

            CarouselRewards.ReplaceRange(Rewards.Collection.Where(r => r.IsCarousel));
        });
    }

    private bool OnRewardsError(Exception ex)
    {
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            if (!Rewards.IsLoaded)
            {
                string userMessage;

                if (ex is HttpRequestException)
                {
                    userMessage = "Unable to load rewards due to a network issue. Please check your internet connection and try again.";
                }
                else
                {
                    userMessage = "An unexpected error occurred while loading rewards. Please try again later.";
                }

                await Shell.Current.DisplayAlert("Oops...", userMessage, "OK");
            }

            IsBusy = false;
            IsRefreshing = false;
            _timer.Start();
        });
        return true;
    }

    private bool FilterReward(Reward reward)
    {
        if (reward == null) return false;

        IsSearching = !string.IsNullOrWhiteSpace(SearchText);

        if (string.IsNullOrWhiteSpace(SearchText))
            return true;

        var searchTerms = SearchText.ToLowerInvariant().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var name = reward.Name?.ToLowerInvariant() ?? string.Empty;
        var description = reward.Description?.ToLowerInvariant() ?? string.Empty;

        return searchTerms.All(term => name.Contains(term) || description.Contains(term));
    }

    private void BeginAutoScroll()
    {
        _timer.Tick += OnScrollTick;
        _timer.Start();
    }

    private void OnScrollTick(object sender, object args)
    {
        MainThread.BeginInvokeOnMainThread(Scroll);
    }

    private void Scroll()
    {
        var count = CarouselRewards.Count;

        if (count > 0)
            CarouselPosition = (CarouselPosition + 1) % count;
    }

    [RelayCommand]
    private void CarouselScrolled()
    {
        // Reset timer when scrolling
        _timer.Stop();
        _timer.Start();
    }

    [RelayCommand]
    private async Task RefreshRewards()
    {
        await LoadData();
    }

    [RelayCommand]
    private async Task RedeemReward(int id)
    {
        if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
        {
            await Shell.Current.DisplayAlert("Device Offline", "You must be online to redeem a reward.", "OK");
            return;
        }

        var reward = Rewards.Collection.FirstOrDefault(r => r.Id == id);
        if (reward != null)
        {
            var popup = new RedeemRewardPage(
                _firebaseAnalyticsService,
                new RedeemRewardViewModel(_userService, _rewardService, _addressService, _firebaseAnalyticsService),
                reward);
            EventHandler<object> handler = null;
            handler = async (_, __) =>
            {
                popup.CallbackEvent -= handler;
                await LoadData();
                await _userService.UpdateMyDetailsAsync();
            };
            popup.CallbackEvent += handler;
            await MopupService.Instance.PushAsync(popup);
        }
    }
}
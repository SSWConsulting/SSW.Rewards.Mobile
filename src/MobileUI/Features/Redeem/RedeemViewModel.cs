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
    private readonly IDispatcherTimer _timer;
    private readonly Subject<string> _searchSubject = new();
    private readonly IAlertService _alertService;
    private readonly IApiErrorPresenter _errorPresenter;

    private const int AutoScrollInterval = 6;
    private const int DebounceInterval = 300;
    private const string RewardsSubject = "rewards";

    public PagedListSource<Reward> Rewards { get; }
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
        IFirebaseAnalyticsService firebaseAnalyticsService, IFileCacheService fileCacheService, IAlertService alertService,
        IApiErrorPresenter errorPresenter)
    {
        Title = "Rewards";
        _rewardService = rewardService;
        _userService = userService;
        _addressService = addressService;
        _firebaseAnalyticsService = firebaseAnalyticsService;
        _alertService = alertService;
        _errorPresenter = errorPresenter;

        Rewards = new PagedListSource<Reward>(new()
        {
            FetchPage = (_, _, _) => FetchRewardsData(),
            PageSize = PagedListSourceOptions<Reward>.SinglePage,
            PrepareItem = reward => reward.CanAfford = reward.Cost <= Credits,
            Filter = FilterReward,
            AreSame = Reward.IsEqual,
            Cache = fileCacheService,
            CacheKey = "RewardsList",
        });

        // Cached rewards land before the network: show the carousel and drop the spinner then.
        Rewards.Items.CollectionChanged += (_, _) => OnRewardsShown();

        _userService.MyBalanceObservable().Subscribe(OnBalanceChanged);
        _userService.MyUserIdObservable().DistinctUntilChanged().Subscribe(OnUserChanged);

        _timer = Application.Current.Dispatcher.CreateTimer();
        _timer.Interval = TimeSpan.FromSeconds(AutoScrollInterval);

        // Set up reactive search with debouncing
        _searchSubject
            .DistinctUntilChanged()
                .Throttle(TimeSpan.FromMilliseconds(DebounceInterval))
                .ObserveOn(SynchronizationContext.Current)
                .Subscribe(_ =>
                {
                    IsSearching = !string.IsNullOrWhiteSpace(SearchText);
                    Rewards.ApplyFilter();
                });
    }

    partial void OnSearchTextChanged(string value)
    {
        _searchSubject.OnNext(value);
    }

    public void OnDisappearing()
    {
        _timer.Stop();
        _timer.Tick -= OnScrollTick;
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
        foreach (var reward in Rewards.AllItems)
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
        if (!Rewards.HasContent)
        {
            await LoadData();
        }

        BeginAutoScroll();
    }

    private async Task LoadData()
    {
        IsBusy = !Rewards.HasContent;
        _timer.Stop();
        CarouselPosition = 0;

        try
        {
            // The latest credit balance decides affordability. Fetched alongside the rewards, so it
            // doesn't hold back the cached ones (offline it only fails after the request does).
            var updateDetails = _userService.UpdateMyDetailsAsync();
            var result = await Rewards.RefreshAsync();
            await updateDetails;
            UpdateRewardsAffordability();

            // Quiet behind cached rewards, except a 401, which still goes to re-login.
            await _errorPresenter.HandleLoadAsync(result, userRequestedNewData: false, RewardsSubject);
        }
        finally
        {
            IsBusy = false;
            IsRefreshing = false;
            _timer.Start();
        }
    }

    private void OnRewardsShown()
    {
        if (Rewards.HasContent)
        {
            IsBusy = false;
        }

        var carousel = Rewards.AllItems.Where(r => r.IsCarousel).ToList();
        if (!carousel.SequenceEqual(CarouselRewards))
        {
            CarouselRewards.ReplaceRange(carousel);
        }
    }

    private async Task<List<Reward>> FetchRewardsData()
    {
        var rewards = await _rewardService.GetRewards();
        var pendingRedemptions = (await _userService.GetPendingRedemptionsAsync()).ToList();

        var rewardsList = new List<Reward>();

        foreach (var reward in rewards.Where(reward => !reward.IsHidden))
        {
            var pendingRedemption = pendingRedemptions.FirstOrDefault(x => x.RewardId == reward.Id);
            if (pendingRedemption != null)
            {
                reward.IsPendingRedemption = true;
                reward.PendingRedemptionCode = pendingRedemption.Code;
            }

            rewardsList.Add(reward);
        }

        return rewardsList;
    }

    private bool FilterReward(Reward reward)
    {
        if (reward == null) return false;

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
        if (!await _errorPresenter.EnsureOnlineAsync("view or redeem a reward"))
        {
            return;
        }

        var reward = Rewards.Items.FirstOrDefault(r => r.Id == id);
        if (reward != null)
        {
            var popup = new RedeemRewardPage(
                _firebaseAnalyticsService,
                new RedeemRewardViewModel(_userService, _rewardService, _addressService, _firebaseAnalyticsService, _alertService),
                reward,
                Credits);
            EventHandler<object> handler = null;
            handler = async (_, __) =>
            {
                popup.CallbackEvent -= handler;
                await LoadData();
            };
            popup.CallbackEvent += handler;
            await MopupService.Instance.PushAsync(popup);
        }
    }
}

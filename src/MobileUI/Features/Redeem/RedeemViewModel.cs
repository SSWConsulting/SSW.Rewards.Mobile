using System.Collections.ObjectModel;
using System.Reactive.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mopups.Services;
using SSW.Rewards.ApiClient.Services;
using SSW.Rewards.Mobile.PopupPages;
using IRewardService = SSW.Rewards.Mobile.Services.IRewardService;
using IUserService = SSW.Rewards.Mobile.Services.IUserService;

namespace SSW.Rewards.Mobile.ViewModels;

public partial class RedeemViewModel : BaseViewModel
{
    private readonly IRewardService _rewardService;
    private readonly IUserService _userService;
    private readonly IAddressService _addressService;
    private readonly IFirebaseAnalyticsService _firebaseAnalyticsService;
    private bool _isLoaded;
    private IDispatcherTimer _timer;
    
    private const int AutoScrollInterval = 6;

    public ObservableRangeCollection<Reward> Rewards { get; set; } = [];
    public ObservableRangeCollection<Reward> CarouselRewards { get; set; } = [];

    [ObservableProperty]
    private int _credits;
    
    [ObservableProperty]
    private bool _isRefreshing;
    
    [ObservableProperty]
    private int _carouselPosition;

    public RedeemViewModel(IRewardService rewardService, IUserService userService, IAddressService addressService, IFirebaseAnalyticsService firebaseAnalyticsService)
    {
        Title = "Rewards";
        _rewardService = rewardService;
        _userService = userService;
        _addressService = addressService;
        _firebaseAnalyticsService = firebaseAnalyticsService;
        _userService.MyBalanceObservable().Subscribe(balance => Credits = balance);
        _userService.MyUserIdObservable().DistinctUntilChanged().Subscribe(OnUserChanged);
        
        _timer = Application.Current.Dispatcher.CreateTimer();
        _timer.Interval = TimeSpan.FromSeconds(AutoScrollInterval);
    }

    public void OnDisappearing()
    {
        _timer.Stop();
        _timer.Tick -= OnScrollTick;
    }

    private void OnUserChanged(int userId)
    {
        Rewards.Clear();
        CarouselRewards.Clear();
        _isLoaded = false;
    }

    public async Task Initialise()
    {
        if (!_isLoaded)
        {
            await LoadData();
        }

        BeginAutoScroll();
    }

    private async Task LoadData()
    {
        if (!_isLoaded)
            IsBusy = true;
        
        _timer.Stop();

        var allRewards = await _rewardService.GetRewards();
        var rewards = allRewards.ToList();
        var pendingRedemptions = (await _userService.GetPendingRedemptionsAsync()).ToList();

        CarouselPosition = 0;
        
        var rewardsList = new List<Reward>();
        var carouselRewardsList = new List<Reward>();
        
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

            if (reward.IsCarousel)
            {
                carouselRewardsList.Add(reward);
            }
        }

        Rewards.ReplaceRange(rewardsList);
        CarouselRewards.ReplaceRange(carouselRewardsList);

        IsBusy = false;
        _isLoaded = true;
        _timer.Start();
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
        IsRefreshing = false;
    }

    [RelayCommand]
    public async Task RedeemReward(int id)
    {
        var reward = Rewards.FirstOrDefault(r => r.Id == id);
        if (reward != null)
        {
            Application.Current.Resources.TryGetValue("Background", out var statusBarColor);
            var popup = new RedeemRewardPage(
                _firebaseAnalyticsService,
                new RedeemRewardViewModel(_userService, _rewardService, _addressService, _firebaseAnalyticsService),
                reward,
                statusBarColor as Color);
            popup.CallbackEvent += async (_, _) =>
            {
                await LoadData();
                await _userService.UpdateMyDetailsAsync();
            };
            await MopupService.Instance.PushAsync(popup);
        }
    }
    
    [RelayCommand]
    private async Task ClosePage()
    {
        await App.Current.MainPage.Navigation.PopModalAsync();
    }
}
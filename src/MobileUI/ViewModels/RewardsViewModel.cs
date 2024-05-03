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

public partial class RewardsViewModel : BaseViewModel
{
    private readonly IRewardService _rewardService;
    private readonly IUserService _userService;
    private readonly IAddressService _addressService;
    private bool _isLoaded;

    public ObservableCollection<Reward> Rewards { get; set; } = new ();
    public ObservableCollection<Reward> CarouselRewards { get; set; } = new ();

    [ObservableProperty]
    private int _credits;

    public RewardsViewModel(IRewardService rewardService, IUserService userService, IAddressService addressService)
    {
        Title = "Rewards";
        _rewardService = rewardService;
        _userService = userService;
        _addressService = addressService;
        _userService.MyBalanceObservable().Subscribe(balance => Credits = balance);
        _userService.MyUserIdObservable().DistinctUntilChanged().Subscribe(OnUserChanged);
    }

    private void OnUserChanged(int userId)
    {
        Rewards.Clear();
        CarouselRewards.Clear();
        _isLoaded = false;
    }

    public async Task Initialise()
    {
        if (_isLoaded)
        {
            return;
        }

        await LoadData();
    }

    private async Task LoadData()
    {
        IsBusy = true;

        Rewards.Clear();
        CarouselRewards.Clear();

        var rewardList = await _rewardService.GetRewards();
        var pendingRedemptions = (await _userService.GetPendingRedemptionsAsync()).ToList();

        foreach (var reward in rewardList.Where(reward => !reward.IsHidden))
        {
            var pendingRedemption = pendingRedemptions.FirstOrDefault(x => x.RewardId == reward.Id);
            reward.CanAfford = reward.Cost <= Credits;

            if (pendingRedemption != null)
            {
                reward.IsPendingRedemption = true;
                reward.PendingRedemptionCode = pendingRedemption.Code;
            }

            Rewards.Add(reward);

            if (reward.IsCarousel)
            {
                CarouselRewards.Add(reward);
            }
        }

        IsBusy = false;
        _isLoaded = true;
    }

    [RelayCommand]
    public async Task RedeemReward(int id)
    {
        var reward = Rewards.FirstOrDefault(r => r.Id == id);
        if (reward != null)
        {
            Application.Current.Resources.TryGetValue("Background", out var statusBarColor);
            var popup = new RedeemRewardPage(new RedeemRewardViewModel(_userService, _rewardService, _addressService), reward);
            popup.CallbackEvent += async (_, _) =>
            {
                await LoadData();
                await _userService.UpdateMyDetailsAsync();
            };
            await MopupService.Instance.PushAsync(popup);
        }
    }
}
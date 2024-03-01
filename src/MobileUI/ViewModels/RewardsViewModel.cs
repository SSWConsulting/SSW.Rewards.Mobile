using SSW.Rewards.PopupPages;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mopups.Services;
using SSW.Rewards.Mobile.PopupPages;

namespace SSW.Rewards.Mobile.ViewModels;

public partial class RewardsViewModel : BaseViewModel
{
    private readonly IRewardService _rewardService;
    private readonly IUserService _userService;
    private bool _isLoaded;

    public ObservableCollection<Reward> Rewards { get; set; } = new ();
    public ObservableCollection<Reward> CarouselRewards { get; set; } = new ();
        
    [ObservableProperty]
    private int _credits;

    public RewardsViewModel(IRewardService rewardService, IUserService userService)
    {
        Title = "Rewards";
        _rewardService = rewardService;
        _userService = userService;
    }

    public async Task Initialise()
    {
        if (_isLoaded)
        {
            return;
        }
            
        IsBusy = true;
        var rewardList = await _rewardService.GetRewards();

        rewardList.ForEach(reward =>
        {
            reward.CanAfford = reward.Cost <= _userService.MyBalance;
            Rewards.Add(reward);

            if (reward.IsCarousel)
            {
                CarouselRewards.Add(reward);
            }
        });
            
        Credits = _userService.MyBalance;

        IsBusy = false;
        _isLoaded = true;
    }

    [RelayCommand]
    public async Task RedeemReward(int id)
    {
        var reward = Rewards.FirstOrDefault(r => r.Id == id);
        if (reward != null)
        {
            var popup = new RedeemReward(new RedeemRewardViewModel(_userService, _rewardService), reward);
            await MopupService.Instance.PushAsync(popup);
        }
    }
}
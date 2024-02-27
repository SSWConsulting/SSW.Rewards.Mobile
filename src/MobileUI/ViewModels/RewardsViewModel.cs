using SSW.Rewards.PopupPages;
using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using Mopups.Services;

namespace SSW.Rewards.Mobile.ViewModels
{
    public partial class RewardsViewModel : BaseViewModel
    {
        private readonly IRewardService _rewardService;
        private readonly IUserService _userService;

        public ICommand RewardCardTappedCommand { get; set; }
        public ICommand MoreTapped { get; set; }

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

            RewardCardTappedCommand = new Command<Reward>(async (reward) =>
            {
                await OpenRewardDetails(reward);
            });

            MoreTapped = new Command<Reward>(async (reward) =>
            {
                await OpenRewardDetails(reward);
            });

            IsBusy = false;
        }

        public async Task OpenRewardDetails(Reward reward)
        {
            await MopupService.Instance.PushAsync(new RewardDetailsPage(reward));
        }
    }
}
using SSW.Rewards.Models;
using SSW.Rewards.PopupPages;
using SSW.Rewards.Services;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace SSW.Rewards.ViewModels
{
    public class RewardsViewModel : BaseViewModel
    {
        private readonly IRewardService _rewardService;

        public ICommand RewardCardTappedCommand { get; set; }
        public ICommand MoreTapped { get; set; }

        public ObservableCollection<Reward> Rewards { get; set; }

        public bool NoRewards { get; set; } = true;

        public RewardsViewModel(IRewardService rewardService)
        {
            Title = "Rewards";
            _rewardService = rewardService;
            Rewards = new ObservableCollection<Reward>();
            _ = Initialise();
        }

        private async Task Initialise()
        {
            var rewardList = await _rewardService.GetRewards();
            rewardList.ForEach(reward =>
            {
                Rewards.Add(reward);
            });

            if (Rewards.Count > 0)
            {
                NoRewards = false;
                RaisePropertyChanged("NoRewards");
            }

            RewardCardTappedCommand = new Command<Reward>(async (reward) =>
            {
                await OpenRewardDetails(reward);
            });

            MoreTapped = new Command<Reward>(async (reward) =>
            {
                await OpenRewardDetails(reward);
            });
        }

        public async Task OpenRewardDetails(Reward reward)
        {
            await Rg.Plugins.Popup.Services.PopupNavigation.Instance.PushAsync(new RewardDetailsPage(reward));
        }
    }
}
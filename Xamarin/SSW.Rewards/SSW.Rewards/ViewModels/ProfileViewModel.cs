using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Threading.Tasks;
using SSW.Rewards.Models;
using SSW.Rewards.Services;
using Xamarin.Forms;

namespace SSW.Rewards.ViewModels
{
    public class ProfileViewModel : BaseViewModel
    {
        private IUserService _userService;

        public string ProfilePic { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Points { get; set; }

        private int userId { get; set; }

        public bool IsLoading { get; set; }

        public ObservableCollection<Reward> Rewards { get; set; }
        public ObservableCollection<Achievement> CompletedAchievements { get; set; }
        public ObservableCollection<Achievement> OutstandingAchievements { get; set; }

        public ProfileViewModel(IUserService userService)
        {
            IsLoading = true;
            RaisePropertyChanged("IsLoading");
            _userService = userService;
            _ = Initialise(true);
        }

        public ProfileViewModel(LeaderSummaryViewModel vm)
        {
            IsLoading = true;
            RaisePropertyChanged("IsLoading");
            ProfilePic = vm.ProfilePic;
            Name = vm.Name;
            Email = vm.Title;
            userId = vm.Id;
            Points = String.Format("{0:n0}", vm.BaseScore);
            _ = Initialise(false);
        }

        private async Task Initialise(bool me)
        {
            IEnumerable<Reward> rewardList = new List<Reward>();
            IEnumerable<Achievement> achievementList = new List<Achievement>();
            MessagingCenter.Subscribe<object>(this, "ProfilePicChanged", async (obj) => { await Refresh(); });

            if (me)
            {
                //initialise me
                ProfilePic = _userService.MyProfilePic;
                Name =  _userService.MyName;
                Email = _userService.MyEmail;
                Points = String.Format("{0:n0}", _userService.MyPoints);
                rewardList = await _userService.GetRewardsAsync();
                achievementList = await _userService.GetAchievementsAsync();
            }
            else
            {
                //initialise other
                _userService = Resolver.Resolve<IUserService>();
                rewardList = await _userService.GetRewardsAsync(userId);
                achievementList = await _userService.GetAchievementsAsync(userId);
            }

            Rewards = new ObservableCollection<Reward>();
            CompletedAchievements = new ObservableCollection<Achievement>();
            OutstandingAchievements = new ObservableCollection<Achievement>();

            foreach(Reward reward in rewardList)
            {
                var profileReward = new Reward();

                if (reward.Awarded)
                {
                    profileReward.Name = "🏆 WON: " + reward.Name;
                }
                else
                {
                    profileReward.Name = reward.Name;
                }

                Rewards.Add(profileReward);
            }

            foreach(Achievement achievement in achievementList)
            {
                if(achievement.Complete && achievement.Value > 0)
                {
                    CompletedAchievements.Add(achievement);
                }
                else if(achievement.Value > 0)
                {
                    OutstandingAchievements.Add(achievement);
                }
            }

            IsLoading = false;

            RaisePropertyChanged("IsLoading", "Rewards", "CompletedAchievements", "OutstandingAchievements");
        }

        private async Task Refresh()
        {
            ProfilePic = _userService.MyProfilePic;
            RaisePropertyChanged(nameof(ProfilePic));
        }
    }
}

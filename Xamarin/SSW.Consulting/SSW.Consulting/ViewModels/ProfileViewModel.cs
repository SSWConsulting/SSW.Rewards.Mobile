using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Threading.Tasks;
using SSW.Consulting.Models;
using SSW.Consulting.Services;

namespace SSW.Consulting.ViewModels
{
    public class ProfileViewModel : BaseViewModel
    {
        private IUserService _userService;

        public string ProfilePic { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Points { get; set; }

        private int userId { get; set; }

        public ObservableCollection<Reward> Rewards { get; set; }
        public ObservableCollection<Achievement> CompletedAchievements { get; set; }
        public ObservableCollection<Achievement> OutstandingAchievements { get; set; }

        public ProfileViewModel(IUserService userService)
        {
            _userService = userService;
            _ = Initialise(true);
        }

        public ProfileViewModel(LeaderSummaryViewModel vm)
        {
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

            if(me)
            {
                //initialise me
                ProfilePic = await _userService.GetMyProfilePicAsync();
                Name = await _userService.GetMyNameAsync();
                Email = await _userService.GetMyEmailAsync();
                Points = String.Format("{0:n0}",await _userService.GetMyPointsAsync());
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

            RaisePropertyChanged("Rewards", "CompletedAchievements", "OutstandingAchievements");
        }
    }
}

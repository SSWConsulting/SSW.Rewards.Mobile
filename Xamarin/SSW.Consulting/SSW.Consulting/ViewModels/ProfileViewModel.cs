using System;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Threading.Tasks;
using SSW.Consulting.Models;
using SSW.Consulting.Services;

namespace SSW.Consulting.ViewModels
{
    public class ProfileViewModel
    {

        private IAchievementService _achievementService;
        private IRewardService _rewardService;
        private IUserService _userService;
        private HttpClient _httpClient;

        public string ProfilePic { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Points { get; set; }

        public ObservableCollection<Reward> Rewards { get; set; }
        public ObservableCollection<Achievement> Achievements { get; set; }

        public ProfileViewModel(IAchievementService achievementService, IRewardService rewardService, IUserService userService)
        {
            _achievementService = achievementService;
            _rewardService = rewardService;
            _userService = userService;
        }

        public ProfileViewModel(LeaderSummaryViewModel vm)
        {
            ProfilePic = vm.ProfilePic;
            Name = vm.Name;
            Email = vm.Title;
            Points = String.Format("{0:n0}", vm.BaseScore);
            _ = Initialise(vm.Id);
        }

        private async Task Initialise(int id)
        {
            if(id == await _userService.GetMyUserIdAsync())
            {
                //initialise me
                ProfilePic = await _userService.GetMyProfilePicAsync();
                Name = await _userService.GetMyNameAsync();
                Email = await _userService.GetMyEmailAsync();
                Points = await _userService.GetMyPointsAsync();
            }
            else
            {
                //initialise other
            }
        }
    }
}

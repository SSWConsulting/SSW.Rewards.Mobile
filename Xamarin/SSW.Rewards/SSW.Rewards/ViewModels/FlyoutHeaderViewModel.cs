using System;
using SSW.Rewards.Services;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace SSW.Rewards.ViewModels
{
    public class FlyoutHeaderViewModel : BaseViewModel
    {
        private IUserService _userService;

        public string ProfilePic{ get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string VersionInfo { get; set; }

        public FlyoutHeaderViewModel(IUserService userService)
        {
            _userService = userService;
            _ = Initialise();
        }

        private async Task Initialise()
        {
            ProfilePic = await _userService.GetMyProfilePicAsync();
            Name = await _userService.GetMyNameAsync();
            Email = await _userService.GetMyEmailAsync();
            VersionInfo = string.Format("Version {0}", AppInfo.VersionString);
        }
    }
}

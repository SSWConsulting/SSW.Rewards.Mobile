using System;
using SSW.Consulting.Services;
using Xamarin.Essentials;

namespace SSW.Consulting.ViewModels
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
            Initialise();
        }

        private async void Initialise()
        {
            ProfilePic = await _userService.GetMyProfilePicAsync();
            Name = await _userService.GetMyNameAsync();
            Email = await _userService.GetMyEmailAsync();
            VersionInfo = string.Format("Version {0}", AppInfo.VersionString);
        }
    }
}

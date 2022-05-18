using System;
using SSW.Rewards.Services;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Rg.Plugins.Popup.Services;
using SSW.Rewards.PopupPages;
using System.Windows.Input;
using Xamarin.Forms;

namespace SSW.Rewards.ViewModels
{
    public class FlyoutHeaderViewModel : BaseViewModel
    {
        private IUserService _userService;

        public string ProfilePic{ get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string VersionInfo { get; set; }
        public ICommand OnProfiePicTapped { get; set; }

        public FlyoutHeaderViewModel(IUserService userService)
        {
            _userService = userService;
            Initialise();
        }

        private void Initialise()
        {
            try
            {
                ProfilePic = _userService.MyProfilePic;
                Name = _userService.MyName;
                Email = _userService.MyEmail;
                VersionInfo = string.Format("Version {0}", AppInfo.VersionString);
                MessagingCenter.Subscribe<string>(this, "ProfilePicChanged", (obj) => { Refresh(obj); });
                OnProfiePicTapped = new Command(async () => await ShowCameraPageAsync());
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR in flyout header");
                Console.WriteLine(ex.Message);
            }
        }

        private async Task ShowCameraPageAsync()
        {
            await PopupNavigation.Instance.PushAsync(new CameraPage());
        }

        private void Refresh(string newPicUri)
        {
            ProfilePic = newPicUri;
            RaisePropertyChanged("ProfilePic");
        }
    }
}

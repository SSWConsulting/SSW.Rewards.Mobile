using SSW.Rewards.Services;
using Xamarin.Forms;

namespace SSW.Rewards.ViewModels
{
    public class FlyoutHeaderViewModel : BaseViewModel
    {
        private IUserService _userService;

        public string ProfilePic{ get; set; }
        public string Name { get; set; }
        public string Email { get; set; }

        public bool Staff { get; set; }

        public FlyoutHeaderViewModel(IUserService userService)
        {
            _userService = userService;
            Initialise();
        }

        private void Initialise()
        {
            ProfilePic = _userService.MyProfilePic;
            Name = _userService.MyName;
            Email = _userService.MyEmail;
            Staff = !string.IsNullOrWhiteSpace(_userService.MyQrCode);
            MessagingCenter.Subscribe<string>(this, "ProfilePicChanged", (obj) => { Refresh(obj); });
        }

        private void Refresh(string newPicUri)
        {
            ProfilePic = newPicUri;
            RaisePropertyChanged("ProfilePic");
        }
    }
}

namespace SSW.Rewards.Mobile.ViewModels;

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

        // Tech Debt - https://github.com/SSWConsulting/SSW.Rewards.Mobile/issues/405
        MessagingCenter.Subscribe<FlyoutHeaderViewModel, string>(this, UserService.UserDetailsUpdatedMessage, (vm, str) => Refresh(str));
    }

    private void Refresh(string pic)
    {
        ProfilePic = pic;
        OnPropertyChanged(nameof(ProfilePic));
    }
}

using Mopups.Services;
using SSW.Rewards.PopupPages;

namespace SSW.Rewards.Mobile;


public partial class AppShell : Shell
{
    private IUserService _userService { get; set; }

    bool _isStaff = false;
    bool _showJoinMenu = false;

    private string _name;
    private string _email;
    private string _profilePic;

    public AppShell(IUserService userService)
    {
        BindingContext = this;

        InitializeComponent();
        _userService = userService;
        VersionLabel.Text = string.Format("Version {0}", AppInfo.VersionString);
        Routing.RegisterRoute("quiz/details", typeof(QuizDetailsPage));

        // Tech Debt - https://github.com/SSWConsulting/SSW.Rewards.Mobile/issues/405
        MessagingCenter.Subscribe<AppShell>(this, UserService.UserDetailsUpdatedMessage, (sh) => Refresh());
    }

    public bool IsStaff
    {
        get => _isStaff;
        set
        {
            _isStaff = value;
            OnPropertyChanged();
        }
    }

    public bool ShowJoinMenuItem
    {
        get => _showJoinMenu;
        set
        {
            _showJoinMenu = value;
            OnPropertyChanged();
        }
    }

    public string Name
    {
        get => _name;
        set
        {
            if (value == _name)
                return;

            _name = value;
            OnPropertyChanged();
        }
    }

    public string Email
    {
        get => _email;
        set
        {
            if (value == _email)
                return;

            _email = value;
            OnPropertyChanged();
        }
    }

    public string ProfilePic
    {
        get => _profilePic;
        set
        {
            if (value == _profilePic)
                return;

            _profilePic = value;
            OnPropertyChanged();
        }
    }

    private void Refresh()
    {
        ProfilePic = _userService.MyProfilePic;
        Name = _userService.MyName;
        Email = _userService.MyEmail;

        IsStaff = _userService.IsStaff;
        ShowJoinMenuItem = !_isStaff;
    }


    public async void Handle_LogOutClicked(object sender, EventArgs e)
    {
        var sure = await App.Current.MainPage.DisplayAlert("Logout", "Are you sure you want to log out of SSW Rewards?", "Yes", "No");

        if (sure)
        {
            _userService.SignOut();
            await Navigation.PushModalAsync<LoginPage>();
        }
    }

    public void Handle_EventsClicked(object sender, EventArgs e)
    {
        var popup = new Events();
        MopupService.Instance.PushAsync(popup);
    }

    public void Handle_JoinClicked(object sender, EventArgs e)
    {
        var popup = new JoinUs();
        MopupService.Instance.PushAsync(popup);
    }

    public void Handle_AboutClicked(object sender, EventArgs e)
    {
        var popup = new AboutSSW();
        MopupService.Instance.PushAsync(popup);
    }

    public void Handle_HowToPlayClicked(object sender, EventArgs e)
    {
        Navigation.PushModalAsync<OnBoarding>();
    }

    private void Handle_QRClicked(object sender, EventArgs e)
    {
        var qrCode = _userService.MyQrCode;

        if (!string.IsNullOrWhiteSpace(qrCode))
        {
            var popup = new MyQRPage(qrCode);
            MopupService.Instance.PushAsync(popup);
        }
    }

    protected override bool OnBackButtonPressed()
    {
        if (Application.Current.MainPage.GetType() == typeof(AppShell) && Shell.Current.Navigation.NavigationStack.Where(x => x != null).Any())
        {
            return base.OnBackButtonPressed();
        }
        else
        {
            System.Diagnostics.Process.GetCurrentProcess().CloseMainWindow();
            return true;
        }
    }
}
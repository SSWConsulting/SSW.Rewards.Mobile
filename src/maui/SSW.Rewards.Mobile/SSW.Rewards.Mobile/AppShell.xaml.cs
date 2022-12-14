using CommunityToolkit.Maui.Views;
using Mopups.Services;
using SSW.Rewards.PopupPages;

namespace SSW.Rewards.Mobile;


public partial class AppShell : Shell
{
    private IUserService _userService { get; set; }

    bool _showQRMenu;
    bool _showJoinMenu;

    public AppShell(IUserService userService, bool isStaff)
    {
        InitializeComponent();
        _userService = userService;
        ShowQRCodeMenuItem = isStaff;
        ShowJoinMenuItem = !isStaff;
        BindingContext = this;
        VersionLabel.Text = string.Format("Version {0}", AppInfo.VersionString);
        Routing.RegisterRoute("quiz/details", typeof(QuizDetailsPage));
    }

    public bool ShowQRCodeMenuItem
    {
        get => _showQRMenu;
        set
        {
            _showQRMenu = value;
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
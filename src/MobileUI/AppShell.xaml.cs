using System.Diagnostics;
using Mopups.Services;
using SSW.Rewards.PopupPages;

namespace SSW.Rewards.Mobile;


public partial class AppShell : Shell
{
    private readonly IUserService _userService;
    private readonly IAuthenticationService _authService;

    public AppShell(IUserService userService, IAuthenticationService authService, bool isStaff)
    {
        IsStaff = isStaff;

        BindingContext = this;
        InitializeComponent();
        _userService = userService;
        _authService = authService;
        VersionLabel.Text = $"Version {AppInfo.VersionString}";
        Routing.RegisterRoute("earn/details", typeof(EarnDetailsPage));
    }

    private bool _isStaff;
    public bool IsStaff
    {
        get => _isStaff;
        set
        {
            _isStaff = value;
            OnPropertyChanged();
        }
    }

    public async void Handle_LogOutClicked(object sender, EventArgs e)
    {
        var sure = await App.Current.MainPage.DisplayAlert("Logout", "Are you sure you want to log out of SSW Rewards?", "Yes", "No");

        if (sure)
        {
            _authService.SignOut();
            await Navigation.PushModalAsync<LoginPage>();
        }
    }    
    
    public async void Handle_SettingsClicked(object sender, EventArgs e)
    {
        //TODO: Perform SettingsClickedAction
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

    public async void Handle_DeleteClicked(object sender, EventArgs e)
    {
        // TODO: remove the DisplayAlert and use the Mopup instead
        // Currently blocked by this issue: https://github.com/LuckyDucko/Mopups/issues/66
        // Note this is related to an underlying .NET MAUI change, which has been
        // fixed, see: https://github.com/dotnet/maui/pull/16983.
        // Until this is merged into a build we can use, we will
        // need to use OS dialogs instead. We can also make this
        // method sync again once we have the fix.
        // var popup = new DeleteProfilePage(_userService);
        // MopupService.Instance.PushAsync(popup);

        // Remove all remaining code in this method after the fix is available

        var sure = await App.Current.MainPage.DisplayAlert("Delete Profile", "If you no longer want an SSW or SSW Rewards account, you can submit a request to SSW to delete your profile and all associated data. Are you sure you want to delete your profile and all associated data?", "Yes", "No");

        if (sure)
        {
            var page = new BusyPage();
            await MopupService.Instance.PushAsync(page);
            var requestSubmitted = await _userService.DeleteProfileAsync();
            await MopupService.Instance.PopAllAsync();

            if (requestSubmitted)
            {
                await App.Current.MainPage.DisplayAlert("Request Submitted", "Your request has been received and you will be contacted within 5 business days. You will now be logged out.", "OK");
                await Navigation.PushModalAsync<LoginPage>();
                await MopupService.Instance.PopAllAsync();
            }
            else
            {
                await App.Current.MainPage.DisplayAlert("Error", "There was an error submitting your request. Please try again later.", "OK");
            }
        }
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

        Process.GetCurrentProcess().CloseMainWindow();
        return true;
    }
}

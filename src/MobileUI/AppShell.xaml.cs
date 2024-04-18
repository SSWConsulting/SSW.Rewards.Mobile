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
        Routing.RegisterRoute("scan", typeof(ScanPage));
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
    
    public async void Handle_SettingsClicked(object sender, EventArgs e)
    {
        await Current.Navigation.PushModalAsync<SettingsPage>();
    }
    
    public async void Handle_LogOutClicked(object sender, EventArgs e)
    {
        var sure = await App.Current.MainPage.DisplayAlert("Logout", "Are you sure you want to log out of SSW Rewards?", "Yes", "No");

        if (sure)
        {
            await _authService.SignOut();
            await Navigation.PushModalAsync<LoginPage>();
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

using System.Diagnostics;
using Mopups.Services;
using SSW.Rewards.Mobile.PopupPages;

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

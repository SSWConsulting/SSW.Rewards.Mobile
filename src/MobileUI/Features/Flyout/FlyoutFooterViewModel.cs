using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mopups.Services;
using SSW.Rewards.Mobile.PopupPages;

namespace SSW.Rewards.Mobile.ViewModels;

public partial class FlyoutFooterViewModel : ObservableObject
{
    private readonly IUserService _userService;
    private readonly IAuthenticationService _authService;
    private readonly IFirebaseAnalyticsService _firebaseAnalyticsService;
    private readonly IPermissionsService _permissionsService;

    [ObservableProperty]
    private bool _isStaff;

    [ObservableProperty]
    private string _versionNumber;

    public FlyoutFooterViewModel(IUserService userService, IAuthenticationService authService, IFirebaseAnalyticsService firebaseAnalyticsService, IPermissionsService permissionsService)
    {
        _userService = userService;
        _authService = authService;
        _firebaseAnalyticsService = firebaseAnalyticsService;
        _permissionsService = permissionsService;
        
        VersionNumber = $"Version {AppInfo.VersionString}";
        
        userService.MyQrCodeObservable().Subscribe(myQrCode => IsStaff = !string.IsNullOrWhiteSpace(myQrCode));
    }
    
    [RelayCommand]
    private async Task MyProfileTapped()
    {
        await AppShell.Current.GoToAsync($"myprofile");
    }
    
    [RelayCommand]
    private async Task MySettingsTapped()
    {
        await AppShell.Current.GoToAsync($"settings");
    }
    
    [RelayCommand]
    private async Task LogOutTapped()
    {
        var sure = await App.Current.MainPage.DisplayAlert("Logout", "Are you sure you want to log out of SSW Rewards?", "Yes", "No");
        
        if (sure)
        {
            await _authService.SignOut();
            await App.Current.MainPage.Navigation.PushModalAsync<LoginPage>();
        }
    }
}

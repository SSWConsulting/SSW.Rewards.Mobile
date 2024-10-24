using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace SSW.Rewards.Mobile.ViewModels;

public partial class FlyoutFooterViewModel : ObservableObject
{
    private readonly IAuthenticationService _authService;
    private readonly IAlertService _alertService;

    [ObservableProperty]
    private bool _isStaff;

    [ObservableProperty]
    private string _versionNumber;

    public FlyoutFooterViewModel(IUserService userService, IAuthenticationService authService, IFirebaseAnalyticsService firebaseAnalyticsService, IPermissionsService permissionsService, IAlertService alertService)
    {
        _authService = authService;
        _alertService = alertService;
        
        VersionNumber = $"Version {AppInfo.VersionString}";
        
        userService.MyQrCodeObservable().Subscribe(myQrCode => IsStaff = !string.IsNullOrWhiteSpace(myQrCode));
    }
    
    [RelayCommand]
    private async Task MyProfileTapped()
    {
        await App.Current.MainPage.Navigation.PushModalAsync<MyProfilePage>();
    }
    
    [RelayCommand]
    private async Task MySettingsTapped()
    {
        await App.Current.MainPage.Navigation.PushModalAsync<SettingsPage>();
    }
    
    [RelayCommand]
    private async Task LogOutTapped()
    {
        var sure = await _alertService.DisplayAlert("Logout", "Are you sure you want to log out of SSW Rewards?", "Yes", "No");
        
        if (sure)
        {
            await _authService.SignOut();
            await App.Current.MainPage.Navigation.PushModalAsync<LoginPage>();
        }
    }
}

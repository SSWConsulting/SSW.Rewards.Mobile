using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace SSW.Rewards.Mobile.ViewModels;

public partial class FlyoutFooterViewModel : ObservableObject
{
    private readonly IAuthenticationService _authService;
    private readonly IServiceProvider _provider;
    private readonly IAlertService _alertService;

    [ObservableProperty]
    private bool _isStaff;

    [ObservableProperty]
    private string _versionNumber;

    public FlyoutFooterViewModel(IUserService userService, IAuthenticationService authService, IServiceProvider provider, IAlertService alertService)
    {
        _authService = authService;
        _provider = provider;
        _alertService = alertService;
        VersionNumber = $"Version {AppInfo.VersionString}";
        userService.MyQrCodeObservable().Subscribe(myQrCode => IsStaff = !string.IsNullOrWhiteSpace(myQrCode));
    }

    [RelayCommand]
    private async Task MyProfileTapped()
    {
        Shell.Current.FlyoutIsPresented = false;
        var page = ActivatorUtilities.CreateInstance<MyProfilePage>(_provider);
        await Shell.Current.Navigation.PushAsync(page);
    }

    [RelayCommand]
    private async Task MySettingsTapped()
    {
        Shell.Current.FlyoutIsPresented = false;
        var page = ActivatorUtilities.CreateInstance<SettingsPage>(_provider);
        await Shell.Current.Navigation.PushAsync(page);
    }

    [RelayCommand]
    private async Task LogOutTapped()
    {
        var sure = await _alertService.DisplayConfirmationAsync("Logout", "Are you sure you want to log out of SSW Rewards?", "Yes", "No");
        if (sure)
        {
            await _authService.SignOut();
            App.NavigateToLoginPage();
        }
    }
}

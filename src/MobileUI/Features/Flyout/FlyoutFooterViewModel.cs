using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mopups.Services;
using SSW.Rewards.Mobile.PopupPages;

namespace SSW.Rewards.Mobile.ViewModels;

public partial class FlyoutFooterViewModel : ObservableObject
{
    private readonly IUserService _userService;
    private readonly IAuthenticationService _authService;

    [ObservableProperty]
    private bool _isStaff;

    [ObservableProperty]
    private string _versionNumber;

    public FlyoutFooterViewModel(IUserService userService, IAuthenticationService authService)
    {
        _userService = userService;
        _authService = authService;
        
        VersionNumber = $"Version {AppInfo.VersionString}";
        
        userService.MyQrCodeObservable().Subscribe(myQrCode => IsStaff = !string.IsNullOrWhiteSpace(myQrCode));
    }
    
    [RelayCommand]
    private async Task MyQrCodeTapped()
    {
        if (!IsStaff)
            return;
        
        Application.Current.Resources.TryGetValue("Background", out var statusBarColor);
        var popup = new QrCodePage(new QrCodeViewModel(_userService), statusBarColor as Color);
        await MopupService.Instance.PushAsync(popup);
    }
    
    [RelayCommand]
    private async Task MySettingsTapped()
    {
        await App.Current.MainPage.Navigation.PushModalAsync<SettingsPage>();
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

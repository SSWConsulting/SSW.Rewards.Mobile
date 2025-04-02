using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SSW.Rewards.Mobile.Messages;

namespace SSW.Rewards.Mobile.ViewModels;

public partial class TopBarViewModel : ObservableObject
{
    private readonly IPermissionsService _permissionsService;
    
    private readonly IUserService _userService;

    private readonly IScannerService _scannerService;

    [ObservableProperty]
    private string _profilePic;

    public TopBarViewModel(IPermissionsService permissionsService, IUserService userService, IScannerService scannerService)
    {
        _permissionsService = permissionsService;
        _userService = userService;
        _scannerService = scannerService;

        userService.MyProfilePicObservable().Subscribe(myProfilePage => ProfilePic = myProfilePage);
    }


    [RelayCommand]
    private void ToggleFlyout()
    {
        Shell.Current.FlyoutIsPresented = !Shell.Current.FlyoutIsPresented;
    }

    [RelayCommand]
    private async Task OpenActivityPage()
    {
        await Shell.Current.GoToAsync("activity");
    }

    [RelayCommand]
    private static void GoBack()
    {
        Shell.Current.SendBackButtonPressed();
    }
}

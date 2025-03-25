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

    [ObservableProperty]
    private bool _showAvatar = true;

    [ObservableProperty]
    private bool _showBack;

    [ObservableProperty]
    private bool _showRedeem = true;
    
    [ObservableProperty]
    private string _title = string.Empty;

    public TopBarViewModel(IPermissionsService permissionsService, IUserService userService, IScannerService scannerService)
    {
        _permissionsService = permissionsService;
        _userService = userService;
        _scannerService = scannerService;
        WeakReferenceMessenger.Default.Register<TopBarAvatarMessage>(this, (_, m) =>
        {
            switch (m.Value)
            {
                case AvatarOptions.Back:
                    SetBackButton();
                    break;
                case AvatarOptions.Original:
                default:
                    SetDefaultAvatar();
                    break;
            }
        });
        
        WeakReferenceMessenger.Default.Register<TopBarTitleMessage>(this, (_, m) =>
        {
            Title = m.Value;
        });

        userService.MyProfilePicObservable().Subscribe(myProfilePage => ProfilePic = myProfilePage);
    }


    [RelayCommand]
    private void ToggleFlyout()
    {
        Shell.Current.FlyoutIsPresented = !Shell.Current.FlyoutIsPresented;
    }

    [RelayCommand]
    private async Task OpenRedeemPage()
    {
        await App.Current.MainPage.Navigation.PushModalAsync<RedeemPage>();
    }

    [RelayCommand]
    private async Task GoBack()
    {
        SetDefaultAvatar();
        await Shell.Current.GoToAsync("..");
    }

    private void SetDefaultAvatar()
    {
        ShowBack = false;
        ShowAvatar = true;
        ShowRedeem = true;
    }

    private void SetBackButton()
    {
        ShowBack = true;
        ShowAvatar = false;
        ShowRedeem = false;
    }
}

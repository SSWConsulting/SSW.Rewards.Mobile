using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SSW.Rewards.Mobile.Messages;

namespace SSW.Rewards.Mobile.ViewModels;

public partial class TopBarViewModel : ObservableObject
{
    private readonly IPermissionsService _permissionsService;

    [ObservableProperty]
    private string _profilePic;

    [ObservableProperty]
    private bool _showAvatar = true;

    [ObservableProperty]
    private bool _showBack;

    [ObservableProperty]
    private bool _showDone;

    [ObservableProperty]
    private bool _showScanner = true;

    public TopBarViewModel(IPermissionsService permissionsService, IUserService userService)
    {
        _permissionsService = permissionsService;
        WeakReferenceMessenger.Default.Register<TopBarAvatarMessage>(this, (r, m) =>
        {
            switch (m.Value)
            {
                case AvatarOptions.Done:
                    SetDoneButton();
                    break;
                case AvatarOptions.Back:
                    SetBackButton();
                    break;
                case AvatarOptions.Original:
                default:
                    SetDefaultAvatar();
                    break;
            }
        });

        userService.MyProfilePicObservable().Subscribe(myProfilePage => ProfilePic = myProfilePage);
    }


    [RelayCommand]
    private void ToggleFlyout()
    {
        Shell.Current.FlyoutIsPresented = !Shell.Current.FlyoutIsPresented;
    }

    [RelayCommand]
    private async Task OpenScanner()
    {
        var granted = await _permissionsService.CheckAndRequestPermission<Permissions.Camera>();
        if (granted)
        {
            await App.Current.MainPage.Navigation.PushModalAsync<ScanPage>();
        }
    }

    [RelayCommand]
    private async Task GoBack()
    {
        SetDefaultAvatar();
        await Shell.Current.GoToAsync("..");
    }

    private void SetDefaultAvatar()
    {
        ShowDone = false;
        ShowBack = false;
        ShowAvatar = true;
        ShowScanner = true;
    }

    private void SetBackButton()
    {
        ShowDone = false;
        ShowBack = true;
        ShowAvatar = false;
        ShowScanner = false;
    }

    private void SetDoneButton()
    {
        ShowDone = true;
        ShowBack = false;
        ShowAvatar = false;
        ShowScanner = true;
    }
}

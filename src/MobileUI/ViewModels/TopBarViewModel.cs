using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SSW.Rewards.Mobile.Messages;

namespace SSW.Rewards.Mobile.ViewModels;

public partial class TopBarViewModel : ObservableObject
{
    [ObservableProperty]
    string profilePic;

    [ObservableProperty]
    bool showAvatar = true;

    [ObservableProperty]
    bool showBack = false;

    [ObservableProperty]
    bool showDone = false;

    public TopBarViewModel()
    {
        WeakReferenceMessenger.Default.Register<ProfilePicUpdatedMessage>(this, (r, m) =>
        {
            ProfilePic = m.ProfilePic;
        });

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
                    SetDefaultAvatar();
                    break;
            }
        });

        ProfilePic = AppShell.ProfilePic;
    }


    [RelayCommand]
    private void ToggleFlyout()
    {
        Shell.Current.FlyoutIsPresented = !Shell.Current.FlyoutIsPresented;
    }

    [RelayCommand]
    private async Task OpenScanner()
    {
        await Shell.Current.GoToAsync("/scan");
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
    }

    private void SetBackButton()
    {
        ShowDone = false;
        ShowBack = true;
        ShowAvatar = false;
    }

    private void SetDoneButton()
    {
        ShowDone = true;
        ShowBack = false;
        ShowAvatar = false;
    }
}

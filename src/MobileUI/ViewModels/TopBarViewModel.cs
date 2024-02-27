using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SSW.Rewards.Mobile.Messages;

namespace SSW.Rewards.Mobile.ViewModels;

public partial class TopBarViewModel : ObservableObject
{
    [ObservableProperty]
    string profilePic;

    public TopBarViewModel()
    {
        WeakReferenceMessenger.Default.Register<ProfilePicUpdatedMessage>(this, (r, m) =>
        {
            ProfilePic = m.ProfilePic;
        });

        ProfilePic = AppShell.ProfilePic;
    }


    [RelayCommand]
    private void ToggleFlyout()
    {
        Shell.Current.FlyoutIsPresented = !Shell.Current.FlyoutIsPresented;
    }

    [RelayCommand]
    private void OpenScanner()
    {

    }
}

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace SSW.Rewards.Mobile.ViewModels;

public partial class TopBarViewModel : ObservableObject
{
    private readonly IServiceProvider _provider;

    [ObservableProperty]
    private string _profilePic;

    /// <summary>Drives the offline banner in the shared page template.</summary>
    public ConnectivityMonitor Connectivity { get; }

    public TopBarViewModel(IServiceProvider provider, IUserService userService, ConnectivityMonitor connectivity)
    {
        _provider = provider;
        Connectivity = connectivity;

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
        var page = ActivatorUtilities.CreateInstance<ActivityPage>(_provider);
        await Shell.Current.Navigation.PushAsync(page);
    }

    [RelayCommand]
    private static void GoBack()
    {
        Shell.Current.SendBackButtonPressed();
    }
}

using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Mopups.Services;
using SSW.Rewards.Mobile.Messages;
using SSW.Rewards.Mobile.PopupPages;
using SSW.Rewards.PopupPages;

namespace SSW.Rewards.Mobile.ViewModels;

public partial class SettingsViewModel : BaseViewModel
{
    private readonly IUserService _userService;
    private readonly ISnackbarService _snackbarService;

    public SettingsViewModel(IUserService userService, ISnackbarService snackbarService)
    {
        _userService = userService;
        _snackbarService = snackbarService;
        Title = "Settings";
    }

    public static void Initialise()
    {
        WeakReferenceMessenger.Default.Send(new TopBarAvatarMessage(AvatarOptions.Back));
    }

    [RelayCommand]
    private static async Task IntroClicked()
    {
        Application.Current.Resources.TryGetValue("Background", out var statusBarColor);
        var page = new OnBoardingPage(false, statusBarColor as Color);
        await MopupService.Instance.PushAsync(page);
    }

    [RelayCommand]
    private static async Task ProfileClicked()
    {
        await Shell.Current.Navigation.PushModalAsync<MyProfilePage>();
    }

    [RelayCommand]
    private async Task AddLinkedIn()
    {
        Application.Current.Resources.TryGetValue("Background", out var statusBarColor);
        var page = new AddLinkedInPage(_userService, _snackbarService, statusBarColor as Color);
        await MopupService.Instance.PushAsync(page);
    }

    [RelayCommand]
    private async Task DeleteClicked()
    {
        // TODO: remove the DisplayAlert and use the Mopup instead
        // Currently blocked by this issue: https://github.com/LuckyDucko/Mopups/issues/66
        // Note this is related to an underlying .NET MAUI change, which has been
        // fixed, see: https://github.com/dotnet/maui/pull/16983.
        // Until this is merged into a build we can use, we will
        // need to use OS dialogs instead. We can also make this
        // method sync again once we have the fix.
        // var popup = new DeleteProfilePage(_userService);
        // MopupService.Instance.PushAsync(popup);

        // Remove all remaining code in this method after the fix is available

        var sure = await App.Current.MainPage.DisplayAlert("Delete Profile", "If you no longer want an SSW or SSW Rewards account, you can submit a request to SSW to delete your profile and all associated data. Are you sure you want to delete your profile and all associated data?", "Yes", "No");

        if (sure)
        {
            var page = new BusyPage();
            await MopupService.Instance.PushAsync(page);
            var requestSubmitted = await _userService.DeleteProfileAsync();
            await MopupService.Instance.PopAllAsync();

            if (requestSubmitted)
            {
                await App.Current.MainPage.DisplayAlert("Request Submitted", "Your request has been received and you will be contacted within 5 business days. You will now be logged out.", "OK");
                await Navigation.PushModalAsync<LoginPage>();
                await MopupService.Instance.PopAllAsync();
            }
            else
            {
                await App.Current.MainPage.DisplayAlert("Error", "There was an error submitting your request. Please try again later.", "OK");
            }
        }
    }

    [RelayCommand]
    private static async Task AboutClicked()
    {
        Application.Current.Resources.TryGetValue("Background", out var statusBarColor);
        var popup = new AboutSswPage(statusBarColor as Color);
        await MopupService.Instance.PushAsync(popup);
    }
}
using CommunityToolkit.Mvvm.Input;

namespace SSW.Rewards.Mobile.ViewModels.ProfileViewModels;

public partial class OthersProfileViewModel(
    IRewardService rewardsService,
    IUserService userService,
    IDevService devService,
    IPermissionsService permissionsService,
    ISnackbarService snackbarService)
    : ProfileViewModelBase(false, rewardsService, userService, devService, permissionsService, snackbarService)
{
    public async Task Initialise()
    {
        await _initialise();
    }

    public void SetUser(int userId)
    {
        UserId = userId;
    }

    [RelayCommand]
    private async Task EmailUser()
    {
        if (Email.Default.IsComposeSupported)
        {
            var message = new EmailMessage
            {
                BodyFormat = EmailBodyFormat.PlainText,
                To = [UserEmail]
            };

            await Email.Default.ComposeAsync(message);
        }
    }
}

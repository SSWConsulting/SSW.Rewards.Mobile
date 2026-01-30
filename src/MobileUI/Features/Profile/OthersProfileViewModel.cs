using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace SSW.Rewards.Mobile.ViewModels.ProfileViewModels;

public partial class OthersProfileViewModel(
    IUserService userService,
    IDevService devService,
    IServiceProvider provider,
    IFileCacheService fileCacheService,
    IAlertService alertService,
    ILogger<ProfileViewModelBase> logger)
    : ProfileViewModelBase(false, userService, devService, provider, fileCacheService, alertService, logger)
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

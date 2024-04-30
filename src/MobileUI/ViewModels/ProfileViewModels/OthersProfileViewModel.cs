using CommunityToolkit.Mvvm.Input;
using SSW.Rewards.Shared.DTOs.Users;

namespace SSW.Rewards.Mobile.ViewModels.ProfileViewModels;

public partial class OthersProfileViewModel(
    IRewardService rewardsService,
    IUserService userService,
    IDevService devService,
    IPermissionsService permissionsService,
    ISnackbarService snackbarService)
    : ProfileViewModelBase(rewardsService, userService, devService, permissionsService, snackbarService)
{
    public async Task Initialise()
    {
        IsMe = false;
        await _initialise();
    }

    public void SetUser(LeaderViewModel vm)
    {
        ProfilePic = vm.ProfilePic;
        Name = vm.Name;
        UserEmail = vm.Email;
        userId = vm.UserId;
        Points = vm.TotalPoints;
        Rank = vm.Rank;
        IsStaff = vm.IsStaff;

        ShowBalance = false;
    }

    public void SetUser(NetworkProfileDto vm)
    {
        ProfilePic = vm.ProfilePicture;
        Name = vm.Name;
        UserEmail = vm.Email;
        userId = vm.UserId;
        Points = vm.TotalPoints;
        Rank = vm.Rank;
        IsStaff = !vm.IsExternal;

        ShowBalance = false;
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

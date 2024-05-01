using CommunityToolkit.Mvvm.Input;
using SSW.Rewards.Shared.DTOs.ActivityFeed;
using SSW.Rewards.Shared.DTOs.Users;

namespace SSW.Rewards.Mobile.ViewModels.ProfileViewModels;

public partial class OthersProfileViewModel(
    IRewardService rewardsService,
    IUserService userService,
    IDevService devService,
    IPermissionsService permissionsService)
    : ProfileViewModelBase(rewardsService, userService, devService, permissionsService)
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
        IsStaff = vm.IsStaff;

        ShowBalance = false;
    }

    public void SetUser(NetworkProfileDto vm)
    {
        ProfilePic = vm.ProfilePicture;
        Name = vm.Name;
        UserEmail = vm.Email;
        userId = vm.UserId;
        IsStaff = !vm.IsExternal;

        ShowBalance = false;
    }
    
    public void SetUser(ActivityFeedItemDto vm)
    {
        ProfilePic = vm.UserAvatar;
        Name = vm.UserName;
        userId = vm.UserId;
        // IsStaff = !vm.IsExternal;

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

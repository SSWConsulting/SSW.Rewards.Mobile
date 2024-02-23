using System.Net.Mail;
using System.Windows.Input;

namespace SSW.Rewards.Mobile.ViewModels.ProfileViewModels;

public class OthersProfileViewModel(
    IRewardService rewardsService,
    IUserService userService,
    ISnackbarService snackbarService,
    IDevService devService)
    : ProfileViewModelBase(rewardsService, userService, snackbarService, devService)
{
    public ICommand EmailUserCommand => new Command(async () => await EmailUser());

    public async Task Initialise()
    {
        IsMe = false;
        
        await _initialise();
    }

    public void SetLeader(LeaderViewModel vm)
    {
        ProfilePic = vm.ProfilePic;
        Name = vm.Name;
        UserEmail = vm.Email;
        userId = vm.UserId;
        Points = vm.TotalPoints;
        Balance = vm.Balance;
        Rank = vm.Rank;
        IsStaff = vm.IsStaff;
        
        ShowBalance = false;
        ShowPopButton = true;
    }
    
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

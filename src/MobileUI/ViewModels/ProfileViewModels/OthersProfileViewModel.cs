using System.Net.Mail;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;

namespace SSW.Rewards.Mobile.ViewModels.ProfileViewModels;

public partial class OthersProfileViewModel(
    IRewardService rewardsService,
    IUserService userService,
    ISnackbarService snackbarService,
    IDevService devService)
    : ProfileViewModelBase(rewardsService, userService, snackbarService, devService)
{
    public async Task Initialise()
    {
        IsMe = false;
        
        await _initialise();
    }

    public bool ShowCloseButton { get; set; } = true;

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
    }

    [RelayCommand]
    private async Task ClosePage()
    {
        await Navigation.PopModalAsync();
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

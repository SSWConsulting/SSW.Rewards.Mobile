using System.Collections.ObjectModel;
using System.Net.Mail;

namespace SSW.Rewards.Mobile.ViewModels.ProfileViewModels;

public class OthersProfileViewModel : ProfileViewModelBase
{
    public OthersProfileViewModel(IRewardService rewardsService, IUserService userService, ISnackbarService snackbarService)
        : base(rewardsService, userService, snackbarService)
    {
    }

    public async Task Initialise()
    {
        if (DeviceInfo.Platform == DevicePlatform.iOS)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                ProfileSections = new ObservableCollection<ProfileCarouselViewModel>();
                OnPropertyChanged(nameof(ProfileSections));
            });
        }

        IsMe = false;
        
        await _initialise();
    }

    public void SetLeader(LeaderViewModel vm)
    {
        ProfilePic = vm.ProfilePic;
        Name = vm.Name;
        userId = vm.UserId;
        Points = vm.TotalPoints;
        Balance = vm.Balance;
        Rank = vm.Rank;
        IsStaff = false;
        
        var emailAddress = new MailAddress(vm.Email);

        if (emailAddress.Host == "ssw.com.au")
        {
            IsStaff = true;
        }
        
        ShowBalance = false;
        ShowPopButton = true;
    }
}

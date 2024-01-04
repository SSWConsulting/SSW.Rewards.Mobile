using System.Collections.ObjectModel;

namespace SSW.Rewards.Mobile.ViewModels.ProfileViewModels;

public class OtherProfileViewModel : ProfileViewModelBase
{
    public OtherProfileViewModel(IRewardService rewardsService, IUserService userService, ISnackbarService snackbarService) : base(rewardsService, userService, snackbarService)
    {
    }

    public async Task Initialise()
    {
        if (DeviceInfo.Platform == DevicePlatform.iOS)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                ProfileSections = new ObservableCollection<ProfileCarouselViewModel>();
            });
        }

        _isMe = false;

        await _initialise();
    }

    public void SetLeader(LeaderViewModel vm)
    {
        ProfilePic = vm.ProfilePic;
        Name = vm.Name;
        userId = vm.UserId;
        Points = vm.TotalPoints;
        Balance = vm.Balance;
        ShowBalance = false;
        ShowPopButton = true;
    }
}

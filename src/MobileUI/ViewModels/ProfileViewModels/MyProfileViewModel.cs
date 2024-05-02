
namespace SSW.Rewards.Mobile.ViewModels.ProfileViewModels;

public class MyProfileViewModel(
    IRewardService rewardsService,
    IUserService userService,
    IDevService devService,
    IPermissionsService permissionsService,
    ISnackbarService snackbarService)
    : ProfileViewModelBase(true, rewardsService, userService, devService, permissionsService, snackbarService)
{
    private readonly IUserService _userService = userService;

    public async Task Initialise()
    {
        _userService.MyUserIdObservable().Subscribe(myUserId => HandleUserIdChange(myUserId));
        _userService.MyNameObservable().Subscribe(myName => Name = myName);
        _userService.MyEmailObservable().Subscribe(myEmail => UserEmail = myEmail);
        _userService.MyProfilePicObservable().Subscribe(myProfilePicture => ProfilePic = myProfilePicture);
        _userService.MyPointsObservable().Subscribe(myPoints => Points = myPoints);
        _userService.MyBalanceObservable().Subscribe(myBalance => Balance = myBalance);
        _userService.IsStaffObservable().Subscribe(isStaff => IsStaff = isStaff);
        _userService.MyAllTimeRankObservable().Subscribe(myRank => Rank = myRank);

        await _initialise();
    }

    private async void HandleUserIdChange(int myUserId)
    {
        userId = myUserId;
        await LoadProfileSections();
    }
}

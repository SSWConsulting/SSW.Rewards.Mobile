using Microsoft.Extensions.Logging;

namespace SSW.Rewards.Mobile.ViewModels.ProfileViewModels;

public class MyProfileViewModel(
    IUserService userService,
    IDevService devService,
    IServiceProvider provider,
    IFileCacheService fileCacheService,
    ILogger<ProfileViewModelBase> logger)
    : ProfileViewModelBase(true, userService, devService, provider, fileCacheService, logger)
{
    private readonly IUserService _userService = userService;

    public async Task Initialise()
    {
        _userService.MyUserIdObservable().Subscribe(HandleUserIdChange);
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
        UserId = myUserId;
        await LoadProfileSections();
    }
}

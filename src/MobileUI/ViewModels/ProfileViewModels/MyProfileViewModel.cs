
namespace SSW.Rewards.Mobile.ViewModels.ProfileViewModels;

public class MyProfileViewModel(
    IRewardService rewardsService,
    IUserService userService,
    ILeaderService leaderService,
    IDevService devService,
    IPermissionsService permissionsService)
    : ProfileViewModelBase(rewardsService, userService, devService, permissionsService)
{
    private readonly IUserService _userService = userService;

    public async Task Initialise()
    {
        IsMe = true;

        _userService.MyUserIdObservable().Subscribe(myUserId => HandleUserIdChange(myUserId));
        _userService.MyNameObservable().Subscribe(myName => Name = myName);
        _userService.MyEmailObservable().Subscribe(myEmail => UserEmail = myEmail);
        _userService.MyProfilePicObservable().Subscribe(myProfilePicture => ProfilePic = myProfilePicture);
        _userService.MyPointsObservable().Subscribe(myPoints => Points = myPoints);
        _userService.MyBalanceObservable().Subscribe(myBalance => Balance = myBalance);
        _userService.MyQrCodeObservable().Subscribe(myQrCode => IsStaff = !string.IsNullOrWhiteSpace(myQrCode));
        Rank = await LoadRank();

        await _initialise();
    }

    private async void HandleUserIdChange(int myUserId)
    {
        userId = myUserId;
        await LoadProfileSections();
    }

    private async Task<int> LoadRank()
    {
        var summaries = await leaderService.GetLeadersAsync(false);
        var myId = userId;
        return summaries.FirstOrDefault(x => x.UserId == myId)?.Rank ?? 0;
    }
}

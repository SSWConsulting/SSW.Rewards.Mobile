using System.Reactive.Linq;
using CommunityToolkit.Mvvm.Messaging;
using SSW.Rewards.Mobile.Messages;

namespace SSW.Rewards.Mobile.ViewModels.ProfileViewModels;

public class MyProfileViewModel(
    IRewardService rewardsService,
    IUserService userService,
    ILeaderService leaderService,
    IDevService devService,
    IPermissionsService permissionsService)
    : ProfileViewModelBase(rewardsService, userService, devService, permissionsService),
        IRecipient<PointsAwardedMessage>
{
    private readonly IUserService _userService = userService;

    public async void Receive(PointsAwardedMessage message)
    {
        await OnPointsAwarded();
    }

    public async Task Initialise()
    {
        IsMe = true;

        _userService.MyName.AsObservable().Subscribe(myName => Name = myName);
        _userService.MyProfilePic.AsObservable().Subscribe(myProfilePicture => ProfilePic = myProfilePicture);
        _userService.MyPoints.AsObservable().Subscribe(myPoints => Points = myPoints);
        _userService.MyBalance.AsObservable().Subscribe(myBalance => Balance = myBalance);
        userId = _userService.MyUserId;
        Rank = await LoadRank();
        IsStaff = _userService.IsStaff;

        await _initialise();
    }

    private async Task OnPointsAwarded()
    {
        await _userService.UpdateMyDetailsAsync();
        await LoadProfileSections();
    }

    private async Task<int> LoadRank()
    {
        var summaries = await leaderService.GetLeadersAsync(false);
        var myId = _userService.MyUserId;
        return summaries.FirstOrDefault(x => x.UserId == myId)?.Rank ?? 0;
    }
}

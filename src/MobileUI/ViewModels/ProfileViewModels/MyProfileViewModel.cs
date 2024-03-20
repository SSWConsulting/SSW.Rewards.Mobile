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
        IRecipient<ProfilePicUpdatedMessage>,
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

        var profilePic = _userService.MyProfilePic;

        //initialise me
        ProfilePic = profilePic;
        Name = _userService.MyName;
        Points = _userService.MyPoints;
        Balance = _userService.MyBalance;
        userId = _userService.MyUserId;
        Rank = await LoadRank();
        IsStaff = _userService.IsStaff;
        UserEmail = _userService.MyEmail;

        await _initialise();
    }

    private async Task OnPointsAwarded()
    {
        await _userService.UpdateMyDetailsAsync();
        await UpdatePoints();
        await LoadProfileSections();
    }

    private Task UpdatePoints()
    {
        Points = _userService.MyPoints;
        Balance = _userService.MyBalance;

        return Task.CompletedTask;
    }

    private async Task<int> LoadRank()
    {
        var summaries = await leaderService.GetLeadersAsync(false);
        var myId = _userService.MyUserId;
        return summaries.FirstOrDefault(x => x.UserId == myId)?.Rank ?? 0;
    }
}

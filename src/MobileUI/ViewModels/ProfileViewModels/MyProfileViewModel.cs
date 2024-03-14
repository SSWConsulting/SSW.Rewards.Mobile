using CommunityToolkit.Mvvm.Messaging;
using SSW.Rewards.Mobile.Messages;
using System.Collections.ObjectModel;

namespace SSW.Rewards.Mobile.ViewModels.ProfileViewModels;

public class MyProfileViewModel(
    IRewardService rewardsService,
    IUserService userService,
    ISnackbarService snackbarService,
    ILeaderService leaderService,
    IDevService devService,
    IPermissionsService permissionsService)
    : ProfileViewModelBase(rewardsService, userService, snackbarService, devService, permissionsService),
        IRecipient<ProfilePicUpdatedMessage>,
        IRecipient<PointsAwardedMessage>,
        IRecipient<SocialUsernameAddedMessage>
{
    public async void Receive(PointsAwardedMessage message)
    {
        await OnPointsAwarded();
    }

    public void Receive(SocialUsernameAddedMessage message)
    {
        AddSocialMediaId(message);
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

    private void AddSocialMediaId(SocialUsernameAddedMessage message)
    {
        // We will remove this. In v3 we're doing social media integration at the identity level.
        return;

        //WeakReferenceMessenger.Default.Unregister<SocialUsernameAddedMessage>(this);

        //IsBusy = true;

        //var result = await _userService.SaveSocialMediaId(message.Value.Achievement.Id, message.Value.Username);

        //var options = new SnackbarOptions
        //{
        //    ActionCompleted = false,
        //    Points = message.Value.Achievement.Value,
        //    GlyphIsBrand = message.Value.Achievement.IconIsBranded,
        //    Glyph = (string)typeof(Icon).GetField(message.Value.Achievement.AchievementIcon.ToString()).GetValue(null)
        //};

        //if (result)
        //{
        //    options.ActionCompleted = true;
        //    message.Value.Achievement.Complete = true;
        //}

        //options.Message = $"{GetMessage(message.Value.Achievement)}";

        //await _snackbarService.ShowSnackbar(options);

        //if (result)
        //{
        //    WeakReferenceMessenger.Default.Send(new PointsAwardedMessage());
        //}

        //IsBusy = false;
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

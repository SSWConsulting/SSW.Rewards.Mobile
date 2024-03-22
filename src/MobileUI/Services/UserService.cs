using System.Reactive.Subjects;
using CommunityToolkit.Mvvm.Messaging;
using SSW.Rewards.Mobile.Messages;
using SSW.Rewards.Shared.DTOs.Users;
using IApiUserService = SSW.Rewards.ApiClient.Services.IUserService;

namespace SSW.Rewards.Mobile.Services;

public class UserService : IUserService, IDisposable
{
    private IApiUserService _userClient { get; }

    private readonly IAuthenticationService _authService;
    private bool _loggedIn = false;

    public bool HasCachedAccount { get => Preferences.Get(nameof(HasCachedAccount), false); }

    public UserService(IApiUserService userService, IAuthenticationService authService)
    {
        _userClient = userService;
        _authService = authService;

        MyEmail = new BehaviorSubject<string>(string.Empty);
        MyPoints = new BehaviorSubject<int>(0);
        MyBalance = new BehaviorSubject<int>(0);

        _authService.DetailsUpdated += UpdateMyDetailsAsync;
    }

    public bool IsLoggedIn { get => _loggedIn; }

    public async Task<bool> DeleteProfileAsync()
    {
        try
        {
            await _userClient.DeleteMyProfile();

            _authService.SignOut();

            return true;
        }
        catch
        {
            return false;
        }

    }

    public async Task<UserProfileDto> GetUserAsync(int userId)
    {
        return await _userClient.GetUser(userId);
    }

    public int MyUserId { get => Preferences.Get(nameof(MyUserId), 0); }
    public BehaviorSubject<string> MyEmail { get; }
    public string MyName { get => Preferences.Get(nameof(MyName), string.Empty); }
    public BehaviorSubject<int> MyPoints { get; }
    public BehaviorSubject<int> MyBalance { get; }
    public string MyQrCode { get => Preferences.Get(nameof(MyQrCode), string.Empty); }

    public string MyProfilePic
    {
        get
        {
            var pic = Preferences.Get(nameof(MyProfilePic), string.Empty);
            if (!string.IsNullOrWhiteSpace(pic))
                return pic;

            return "v2sophie";
        }
    }

    public bool IsStaff { get => !string.IsNullOrWhiteSpace(MyQrCode); }

    public async Task<string> UploadImageAsync(Stream image, string fileName)
    {
        var response = await _userClient.UploadProfilePic(image, fileName);

        Preferences.Set(nameof(MyProfilePic), response.PicUrl);

        if (response.AchievementAwarded)
        {
            await UpdateMyDetailsAsync();
        }

        WeakReferenceMessenger.Default.Send(new ProfilePicUpdatedMessage
        {
            ProfilePic = MyProfilePic
        });
        return response.PicUrl;
    }

    private void UpdateMyDetailsAsync(object sender, DetailsUpdatedEventArgs args)
    {
        MainThread.BeginInvokeOnMainThread(async () => await UpdateMyDetailsAsync());
    }

    public async Task UpdateMyDetailsAsync()
    {
        var user = await _userClient.GetCurrentUser();

        Preferences.Set(nameof(MyName), user.FullName);
        Preferences.Set(nameof(MyUserId), user.Id);
        Preferences.Set(nameof(MyProfilePic), user.ProfilePic);
        Preferences.Set(nameof(MyQrCode), user.QRCode);

        MyEmail.OnNext(user.Email);
        MyPoints.OnNext(user.Points);
        MyBalance.OnNext(user.Balance);

        WeakReferenceMessenger.Default.Send(new UserDetailsUpdatedMessage(new UserContext
        {
            ProfilePic  = MyProfilePic,
            Name        = MyName,
            IsStaff     = IsStaff
        }));
    }

    public async Task<IEnumerable<Achievement>> GetAchievementsAsync()
    {
        return await GetAchievementsForUserAsync(MyUserId);
    }

    public async Task<IEnumerable<Achievement>> GetAchievementsAsync(int userId)
    {
        return await GetAchievementsForUserAsync(userId);
    }

    public async Task<IEnumerable<Achievement>> GetProfileAchievementsAsync()
    {
        return await GetProfileAchievementsAsync(MyUserId);
    }

    public async Task<IEnumerable<Achievement>> GetProfileAchievementsAsync(int userId)
    {
        List<Achievement> achievements = new List<Achievement>();

        var achievementsList = await _userClient.GetProfileAchievements(userId);

        foreach (var achievement in achievementsList.UserAchievements)
        {
            achievements.Add(new Achievement
            {
                Complete = achievement.Complete,
                Name = achievement.AchievementName,
                Value = achievement.AchievementValue,
                Type = achievement.AchievementType,
                AwardedAt = achievement.AwardedAt,
                AchievementIcon = achievement.AchievementIcon,
                IconIsBranded = achievement.AchievementIconIsBranded,
                Id = achievement.AchievementId
            });
        }

        return achievements;
    }

    private async Task<IEnumerable<Achievement>> GetAchievementsForUserAsync(int userId)
    {
        List<Achievement> achievements = new List<Achievement>();

        var achievementsList = await _userClient.GetUserAchievements(userId);

        foreach (var achievement in achievementsList.UserAchievements)
        {
            achievements.Add(new Achievement
            {
                Complete = achievement.Complete,
                Name = achievement.AchievementName,
                Value = achievement.AchievementValue,
                Type = achievement.AchievementType,
                AwardedAt = achievement.AwardedAt
            });
        }

        return achievements;
    }

    public async Task<IEnumerable<Reward>> GetRewardsAsync()
    {
        return await GetRewardsForUserAsync(MyUserId);
    }

    public async Task<IEnumerable<Reward>> GetRewardsAsync(int userId)
    {
        return await GetRewardsForUserAsync(userId);
    }

    private async Task<IEnumerable<Reward>> GetRewardsForUserAsync(int userId)
    {
        List<Reward> rewards = new List<Reward>();

        var rewardsList = await _userClient.GetUserRewards(userId);

        foreach (var userReward in rewardsList.UserRewards)
        {
            rewards.Add(new Reward
            {
                Awarded = userReward.Awarded,
                Name = userReward.RewardName,
                Cost = userReward.RewardCost,
                AwardedAt = userReward.AwardedAt
            });
        }

        return rewards;
    }

    public Task<ImageSource> GetProfilePicAsync(string url)
    {
        throw new NotImplementedException();
    }

    public Task<ImageSource> GetAvatarAsync(string url)
    {
        throw new NotImplementedException();
    }

    public void Dispose()
    {
        //_authService.DetailsUpdated -= UpdateMyDetailsAsync;
    }
}

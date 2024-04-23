using System.Reactive.Linq;
using System.Reactive.Subjects;
using SSW.Rewards.Shared.DTOs.Users;
using IApiUserService = SSW.Rewards.ApiClient.Services.IUserService;

namespace SSW.Rewards.Mobile.Services;

public class UserService : IUserService
{
    private readonly IApiUserService _userClient;
    private readonly IAuthenticationService _authService;

    private readonly BehaviorSubject<int> _myUserId = new(0);
    private readonly BehaviorSubject<string> _myName = new(string.Empty);
    private readonly BehaviorSubject<string> _myEmail = new(string.Empty);
    private readonly BehaviorSubject<string> _myProfilePic = new(string.Empty);
    private readonly BehaviorSubject<int> _myPoints = new(0);
    private readonly BehaviorSubject<int> _myBalance = new(0);
    private readonly BehaviorSubject<string> _myQrCode = new(string.Empty);
    private readonly BehaviorSubject<int> _myAllTimeRank = new(Int32.MaxValue);

    public UserService(IApiUserService userService, IAuthenticationService authService)
    {
        _userClient = userService;
        _authService = authService;
        _authService.DetailsUpdated += UpdateMyDetailsAsync;
    }

    public IObservable<int> MyUserIdObservable() => _myUserId.AsObservable();
    public IObservable<string> MyNameObservable() => _myName.AsObservable();
    public IObservable<string> MyEmailObservable() => _myEmail.AsObservable();
    public IObservable<string> MyProfilePicObservable() => _myProfilePic.AsObservable();
    public IObservable<int> MyPointsObservable() => _myPoints.AsObservable();
    public IObservable<int> MyBalanceObservable() => _myBalance.AsObservable();
    public IObservable<string> MyQrCodeObservable() => _myQrCode.AsObservable();
    public IObservable<int> MyAllTimeRankObservable() => _myAllTimeRank.AsObservable();

    public async Task<UserProfileDto> GetUserAsync(int userId)
    {
        return await _userClient.GetUser(userId);
    }

    public async Task<string> UploadImageAsync(Stream image, string fileName)
    {
        var response = await _userClient.UploadProfilePic(image, fileName);
        await UpdateMyDetailsAsync();

        return response.PicUrl;
    }

    private void UpdateMyDetailsAsync(object sender, EventArgs args)
    {
        MainThread.BeginInvokeOnMainThread(async () => await UpdateMyDetailsAsync());
    }

    public async Task UpdateMyDetailsAsync()
    {
        var user = await _userClient.GetCurrentUser();
        _myUserId.OnNext(user.Id);
        _myName.OnNext(user.FullName);
        _myEmail.OnNext(user.Email);
        _myProfilePic.OnNext(user.ProfilePic ?? "v2sophie");
        _myPoints.OnNext(user.Points);
        _myBalance.OnNext(user.Balance);
        _myQrCode.OnNext(user.QRCode);
    }

    public void UpdateMyAllTimeRank(int newRank)
    {
        _myAllTimeRank.OnNext(newRank);
    }

    public async Task<IEnumerable<Achievement>> GetAchievementsAsync()
    {
        return await GetAchievementsForUserAsync(_myUserId.Value);
    }

    public async Task<IEnumerable<Achievement>> GetAchievementsAsync(int userId)
    {
        return await GetAchievementsForUserAsync(userId);
    }

    public async Task<IEnumerable<Achievement>> GetProfileAchievementsAsync()
    {
        return await GetProfileAchievementsAsync(_myUserId.Value);
    }

    public async Task<IEnumerable<Achievement>> GetProfileAchievementsAsync(int userId)
    {
        List<Achievement> achievements = [];
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
        List<Achievement> achievements = [];
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
        return await GetRewardsForUserAsync(_myUserId.Value);
    }

    public async Task<IEnumerable<Reward>> GetRewardsAsync(int userId)
    {
        return await GetRewardsForUserAsync(userId);
    }

    private async Task<IEnumerable<Reward>> GetRewardsForUserAsync(int userId)
    {
        List<Reward> rewards = [];
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

    public async Task<bool?> SaveSocialMediaId(int achievementId, string socialMediaUserId)
    {
        try
        {
            var achieved = await _userClient.UpsertUserSocialMediaIdAsync(achievementId, socialMediaUserId);
            return achieved != 0;
        }
        catch
        {
            return null;
        }
    }

    public async Task<bool> DeleteProfileAsync()
    {
        try
        {
            await _userClient.DeleteMyProfile();
            await _authService.SignOut();
            return true;
        }
        catch
        {
            return false;
        }
    }
}

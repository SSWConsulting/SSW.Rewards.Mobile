using System.Reactive.Linq;
using System.Reactive.Subjects;
using Microsoft.Extensions.Logging;
using SSW.Rewards.Shared.DTOs.Users;
using IApiUserService = SSW.Rewards.ApiClient.Services.IUserService;

namespace SSW.Rewards.Mobile.Services;

public interface IUserService
{
    IObservable<int> MyUserIdObservable();
    IObservable<string> MyNameObservable();
    IObservable<string> MyEmailObservable();
    IObservable<string> MyProfilePicObservable();
    IObservable<int> MyPointsObservable();
    IObservable<int> MyBalanceObservable();
    IObservable<string> MyQrCodeObservable();
    IObservable<int> MyAllTimeRankObservable();
    IObservable<bool> IsStaffObservable();
    IObservable<bool> IsConnectedObservable();

    IObservable<string> LinkedInProfileObservable();
    IObservable<string> GitHubProfileObservable();
    IObservable<string> TwitterProfileObservable();
    IObservable<string> CompanyUrlObservable();

    // auth methods

    // user details
    Task UpdateMyDetailsAsync();
    Task<IEnumerable<Achievement>> GetAchievementsAsync();
    Task<IEnumerable<Achievement>> GetAchievementsAsync(int userId);
    Task<IEnumerable<Achievement>> GetProfileAchievementsAsync();
    Task<IEnumerable<Achievement>> GetProfileAchievementsAsync(int userId);
    Task<IEnumerable<Reward>> GetRewardsAsync();
    Task<IEnumerable<Reward>> GetRewardsAsync(int userId);
    Task<IEnumerable<UserPendingRedemptionDto>> GetPendingRedemptionsAsync();
    Task<IEnumerable<UserPendingRedemptionDto>> GetPendingRedemptionsAsync(int userId);
    Task<string> UploadImageAsync(Stream image, string fileName);
    Task<UserProfileDto> GetUserAsync(int userId);
    Task<bool?> SaveSocialMedia(int socialMediaPlatformId, string socialMediaUserProfile);
    Task LoadSocialMedia(int userId);
    Task<List<UserSocialMediaIdDto>> GetSocialMedia(int userId);
    void ClearSocialMedia();
    Task<bool> DeleteProfileAsync();
}

public class UserService : IUserService
{
    private const string UserIdKey = "CachedUserId";
    private const string UserNameKey = "CachedUserName";
    private const string UserEmailKey = "CachedUserEmail";
    private const string UserProfilePicKey = "CachedUserProfilePic";
    private const string UserPointsKey = "CachedUserPoints";
    private const string UserBalanceKey = "CachedUserBalance";
    private const string UserQrCodeKey = "CachedUserQrCode";
    private const string UserRankKey = "CachedUserRank";
    private const string UserIsStaffKey = "CachedUserIsStaff";

    private readonly IApiUserService _userClient;
    private readonly IAuthenticationService _authService;
    private readonly ILogger<UserService> _logger;

    private readonly BehaviorSubject<int> _myUserId = new(0);
    private readonly BehaviorSubject<string> _myName = new(string.Empty);
    private readonly BehaviorSubject<string> _myEmail = new(string.Empty);
    private readonly BehaviorSubject<string> _myProfilePic = new(string.Empty);
    private readonly BehaviorSubject<int> _myPoints = new(0);
    private readonly BehaviorSubject<int> _myBalance = new(0);
    private readonly BehaviorSubject<string> _myQrCode = new(string.Empty);
    private readonly BehaviorSubject<int> _myAllTimeRank = new(0);
    private readonly BehaviorSubject<bool> _isStaff = new(false);
    private readonly BehaviorSubject<bool> _isConnected = new(true);

    /// <summary>
    /// Stores my profile as well as other users
    /// </summary>
    /// <returns></returns>
    private readonly BehaviorSubject<string> _linkedInProfile = new(string.Empty);
    private readonly BehaviorSubject<string> _gitHubProfile = new(string.Empty);
    private readonly BehaviorSubject<string> _twitterProfile = new(string.Empty);
    private readonly BehaviorSubject<string> _companyUrl = new(string.Empty);

    public UserService(IApiUserService userService, IAuthenticationService authService, ILogger<UserService> logger)
    {
        _userClient = userService;
        _authService = authService;
        _logger = logger;
        _authService.DetailsUpdated += UpdateMyDetailsAsync;

        // Load cached profile data on startup
        LoadCachedProfileData();

        // Monitor connectivity
        Connectivity.ConnectivityChanged += OnConnectivityChanged;
        UpdateConnectivityState();
    }

    public IObservable<int> MyUserIdObservable() => _myUserId.AsObservable();
    public IObservable<string> MyNameObservable() => _myName.AsObservable();
    public IObservable<string> MyEmailObservable() => _myEmail.AsObservable();
    public IObservable<string> MyProfilePicObservable() => _myProfilePic.AsObservable();
    public IObservable<int> MyPointsObservable() => _myPoints.AsObservable();
    public IObservable<int> MyBalanceObservable() => _myBalance.AsObservable();
    public IObservable<string> MyQrCodeObservable() => _myQrCode.AsObservable();
    public IObservable<int> MyAllTimeRankObservable() => _myAllTimeRank.AsObservable();
    public IObservable<bool> IsStaffObservable() => _isStaff.AsObservable();
    public IObservable<bool> IsConnectedObservable() => _isConnected.AsObservable();

    public IObservable<string> LinkedInProfileObservable() => _linkedInProfile.AsObservable();
    public IObservable<string> GitHubProfileObservable() => _gitHubProfile.AsObservable();
    public IObservable<string> TwitterProfileObservable() => _twitterProfile.AsObservable();
    public IObservable<string> CompanyUrlObservable() => _companyUrl.AsObservable();

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
        try
        {
            var user = await _userClient.GetCurrentUser();
            _myUserId.OnNext(user.Id);
            _myName.OnNext(user.FullName);
            _myEmail.OnNext(user.Email);
            _myProfilePic.OnNext(user.ProfilePic ?? "v2sophie");
            _myPoints.OnNext(user.Points);
            _myBalance.OnNext(user.Balance);
            _myQrCode.OnNext(user.QRCode);
            _isStaff.OnNext(user.IsStaff);
            _myAllTimeRank.OnNext(user.Rank);

            // Persist data for offline access
            SaveProfileDataToCache(user);

            // Update connectivity state on successful API call
            UpdateConnectivityState();
        }
        catch (HttpRequestException ex)
        {
            // Network connectivity issues - silently fail and keep existing data
            _logger.LogWarning(ex, "Network error while updating current user details");
            UpdateConnectivityState();
        }
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

        rewards.AddRange(rewardsList.UserRewards.Select(userReward => new Reward
        {
            Awarded = userReward.Awarded,
            Name = userReward.RewardName,
            Cost = userReward.RewardCost,
            AwardedAt = userReward.AwardedAt
        }));

        return rewards;
    }

    public async Task<IEnumerable<UserPendingRedemptionDto>> GetPendingRedemptionsAsync()
    {
        return await GetPendingRedemptionsForUserAsync(_myUserId.Value);
    }

    public async Task<IEnumerable<UserPendingRedemptionDto>> GetPendingRedemptionsAsync(int userId)
    {
        return await GetPendingRedemptionsForUserAsync(userId);
    }

    private async Task<IEnumerable<UserPendingRedemptionDto>> GetPendingRedemptionsForUserAsync(int userId)
    {
        List<UserPendingRedemptionDto> redemptions = [];
        var redemptionsList = await _userClient.GetUserPendingRedemptions(userId);

        foreach (var userRedemption in redemptionsList.PendingRedemptions)
        {
            redemptions.Add(new UserPendingRedemptionDto
            {
                RewardId = userRedemption.RewardId,
                ClaimedAt = userRedemption.ClaimedAt,
                Code = userRedemption.Code
            });
        }

        return redemptions;
    }

    public async Task<bool?> SaveSocialMedia(int socialMediaPlatformId, string socialMediaUserProfile)
    {
        try
        {
            var achieved = await _userClient.UpsertUserSocialMediaIdAsync(socialMediaPlatformId, socialMediaUserProfile);
            UpdateSocialMediaProfile(socialMediaPlatformId, socialMediaUserProfile);
            return achieved != 0;
        }
        catch
        {
            return null;
        }
    }

    public async Task LoadSocialMedia(int userId)
    {
        try
        {
            var socialMedia = await _userClient.GetSocialMedia(userId);

            if (socialMedia == null || socialMedia.SocialMedia.Count == 0)
            {
                return;
            }

            foreach (var social in socialMedia.SocialMedia)
            {
                UpdateSocialMediaProfile(social.SocialMediaPlatformId, social.SocialMediaUserId);
            }
        }
        catch (Exception ex)
        {
            // ignored
        }
    }

    public async Task<List<UserSocialMediaIdDto>> GetSocialMedia(int userId)
    {
        try
        {
            var socialMedia = await _userClient.GetSocialMedia(userId);

            return socialMedia?.SocialMedia ?? [];
        }
        catch (Exception)
        {
            return [];
        }
    }

    public void ClearSocialMedia()
    {
        _linkedInProfile.OnNext(string.Empty);
        _gitHubProfile.OnNext(string.Empty);
        _twitterProfile.OnNext(string.Empty);
        _companyUrl.OnNext(string.Empty);
    }

    public void UpdateSocialMediaProfile(int socialMediaPlatformId, string socialMediaUserProfile)
    {
        switch (socialMediaPlatformId)
        {
            case Constants.SocialMediaPlatformIds.LinkedIn:
                _linkedInProfile.OnNext(socialMediaUserProfile);
                break;
            case Constants.SocialMediaPlatformIds.GitHub:
                _gitHubProfile.OnNext(socialMediaUserProfile);
                break;
            case Constants.SocialMediaPlatformIds.Twitter:
                _twitterProfile.OnNext(socialMediaUserProfile);
                break;
            case Constants.SocialMediaPlatformIds.Company:
                _companyUrl.OnNext(socialMediaUserProfile);
                break;
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

    private void LoadCachedProfileData()
    {
        try
        {
            var userId = Preferences.Get(UserIdKey, 0);
            if (userId == 0) return; // No cached data

            _myUserId.OnNext(userId);
            _myName.OnNext(Preferences.Get(UserNameKey, string.Empty));
            _myEmail.OnNext(Preferences.Get(UserEmailKey, string.Empty));
            _myProfilePic.OnNext(Preferences.Get(UserProfilePicKey, "v2sophie"));
            _myPoints.OnNext(Preferences.Get(UserPointsKey, 0));
            _myBalance.OnNext(Preferences.Get(UserBalanceKey, 0));
            _myQrCode.OnNext(Preferences.Get(UserQrCodeKey, string.Empty));
            _myAllTimeRank.OnNext(Preferences.Get(UserRankKey, 0));
            _isStaff.OnNext(Preferences.Get(UserIsStaffKey, false));

            _logger.LogInformation("Loaded cached profile data for user {UserId}", userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load cached profile data");
        }
    }

    private void SaveProfileDataToCache(CurrentUserDto user)
    {
        try
        {
            Preferences.Set(UserIdKey, user.Id);
            Preferences.Set(UserNameKey, user.FullName ?? string.Empty);
            Preferences.Set(UserEmailKey, user.Email ?? string.Empty);
            Preferences.Set(UserProfilePicKey, user.ProfilePic ?? "v2sophie");
            Preferences.Set(UserPointsKey, user.Points);
            Preferences.Set(UserBalanceKey, user.Balance);
            Preferences.Set(UserQrCodeKey, user.QRCode ?? string.Empty);
            Preferences.Set(UserRankKey, user.Rank);
            Preferences.Set(UserIsStaffKey, user.IsStaff);

            _logger.LogInformation("Saved profile data to cache for user {UserId}", user.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save profile data to cache");
        }
    }

    private void UpdateConnectivityState()
    {
        var networkAccess = Connectivity.Current.NetworkAccess;
        var isConnected = networkAccess == NetworkAccess.Internet;
        _isConnected.OnNext(isConnected);
    }

    private void OnConnectivityChanged(object sender, ConnectivityChangedEventArgs e)
    {
        UpdateConnectivityState();
    }
}

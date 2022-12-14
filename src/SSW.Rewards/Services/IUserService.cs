using SSW.Rewards.Models;

namespace SSW.Rewards.Services;

public interface IUserService
{
    // values
    int MyUserId { get; }
    string MyName { get; }
    string MyEmail { get; }
    string MyProfilePic { get; }
    int MyPoints { get; }
    int MyBalance { get; }
    string MyQrCode { get; }
    bool IsLoggedIn { get; }
    bool HasCachedAccount { get; }

    // auth methods
    Task<ApiStatus> SignInAsync();
    Task ResetPassword();
    Task<bool> RefreshLoginAsync();
    void SignOut();

    // user details
    Task UpdateMyDetailsAsync();
    Task<IEnumerable<Achievement>> GetAchievementsAsync();
    Task<IEnumerable<Achievement>> GetAchievementsAsync(int userId);
    Task<IEnumerable<Achievement>> GetProfileAchievementsAsync();
    Task<IEnumerable<Achievement>> GetProfileAchievementsAsync(int userId);
    Task<IEnumerable<Reward>> GetRewardsAsync();
    Task<IEnumerable<Reward>> GetRewardsAsync(int userId);
    Task<ImageSource> GetAvatarAsync(string url);
    Task<string> UploadImageAsync(Stream image);

    Task<bool> SaveSocialMediaId(int achievementId, string userId);
}


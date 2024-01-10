namespace SSW.Rewards.Mobile.Services;

public interface IUserService
{
    // TODO: Replace all this with a GetMyProfile method
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
    bool IsStaff { get; }

    // auth methods

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


    Task<bool> DeleteProfileAsync();
}


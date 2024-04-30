using SSW.Rewards.Shared.DTOs.Users;

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

    // auth methods

    // user details
    Task UpdateMyDetailsAsync();
    void UpdateMyAllTimeRank(int newRank);
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
    Task<bool> DeleteProfileAsync();
}


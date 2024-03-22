using System.Reactive.Subjects;
using SSW.Rewards.Shared.DTOs.Users;

namespace SSW.Rewards.Mobile.Services;

public interface IUserService
{
    BehaviorSubject<int> MyUserId { get; }
    BehaviorSubject<string> MyName { get; }
    BehaviorSubject<string> MyEmail { get; }
    BehaviorSubject<string> MyProfilePic { get; }
    BehaviorSubject<int> MyPoints { get; }
    BehaviorSubject<int> MyBalance { get; }
    BehaviorSubject<string> MyQrCode { get; }
    bool HasCachedAccount { get; }

    // auth methods

    // user details
    Task UpdateMyDetailsAsync();
    Task<IEnumerable<Achievement>> GetAchievementsAsync();
    Task<IEnumerable<Achievement>> GetAchievementsAsync(int userId);
    Task<IEnumerable<Achievement>> GetProfileAchievementsAsync();
    Task<IEnumerable<Achievement>> GetProfileAchievementsAsync(int userId);
    Task<IEnumerable<Reward>> GetRewardsAsync();
    Task<IEnumerable<Reward>> GetRewardsAsync(int userId);
    Task<string> UploadImageAsync(Stream image, string fileName);
    Task<UserProfileDto> GetUserAsync(int userId);
    Task<bool> DeleteProfileAsync();
}


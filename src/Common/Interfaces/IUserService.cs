using Shared.DTOs.Users;

namespace Shared.Interfaces;

public interface IUserService
{
    Task DeleteMyProfile(CancellationToken cancellationToken = default);

    Task<ProfilePicResponseDto> UploadProilePic(Stream file, CancellationToken cancellationToken = default);

    Task<CurrentUserDto> GetCurrentUser(CancellationToken cancellationToken = default);

    Task<UserAchievementsViewModel> GetProfileAchievements(int userId, CancellationToken cancellationToken = default);

    Task<UserProfileDto> GetUser(int userId, CancellationToken cancellationToken = default);

    Task<UserAchievementsViewModel> GetUserAchievements(int userId, CancellationToken cancellationToken = default);

    Task<UserRewardsViewModel> GetUserRewards(int userId, CancellationToken cancellationToken = default);
}

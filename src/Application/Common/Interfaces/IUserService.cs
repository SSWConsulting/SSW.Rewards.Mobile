using SSW.Rewards.Shared.DTOs.Users;

namespace SSW.Rewards.Application.Common.Interfaces;
public interface IUserService
{
    Task<IEnumerable<Role>> GetUserRoles(int userId, CancellationToken cancellationToken);

    Task<IEnumerable<Role>> GetCurrentUserRoles(CancellationToken cancellationToken);


    // Add user roles
    Task AddUserRole(User user, Role role, CancellationToken cancellationToken);


    // Remove user roles
    Task RemoveUserRole(int userId, int roleId, CancellationToken cancellationToken);


    // Create user
    Task<User> CreateUser(User user, CancellationToken cancellationToken);


    // Update user
    Task UpdateUser(int UserId, User user, CancellationToken cancellationToken);


    // Get user
    Task<UserProfileDto> GetUser(int userId, CancellationToken cancellationToken);
    Task<int> GetUserId(string email, CancellationToken cancellationToken);
    
    // Get users
    Task<UsersViewModel> GetUsers(CancellationToken cancellationToken);

    // Get user achievements
    Task<UserAchievementsViewModel> GetUserAchievements(int userId, CancellationToken cancellationToken);

    // Get user rewards
    Task<UserRewardsViewModel> GetUserRewards(int userId, CancellationToken cancellationToken);

    // Get current user
    Task<CurrentUserDto> GetCurrentUser(CancellationToken cancellationToken);

    Task<int> GetCurrentUserId(CancellationToken cancellationToken);

    // Get user's QR code
    Task<string> GetStaffQRCode(string emailAddress, CancellationToken cancellationToken);
    
    // Get user's pending redemptions
    Task<UserPendingRedemptionsViewModel> GetUserPendingRedemptions(int userId, CancellationToken cancellationToken);
}

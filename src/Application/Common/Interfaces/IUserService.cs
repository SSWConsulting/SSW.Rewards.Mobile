using SSW.Rewards.Shared.DTOs.Users;

namespace SSW.Rewards.Application.Common.Interfaces;
public interface IUserService
{
    IEnumerable<Role> GetUserRoles(int userId);
    Task<IEnumerable<Role>> GetUserRoles(int userId, CancellationToken cancellationToken);

    IEnumerable<Role> GetCurrentUserRoles();
    Task<IEnumerable<Role>> GetCurrentUserRoles(CancellationToken cancellationToken);


    // Add user roles
    void AddUserRole(User user, Role role);
    Task AddUserRole(User user, Role role, CancellationToken cancellationToken);


    // Remove user roles
    void RemoveUserRole(int userId, int roleId);
    Task RemoveUserRole(int userId, int roleId, CancellationToken cancellationToken);


    // Create user
    int CreateUser(User user);
    Task<int> CreateUser(User user, CancellationToken cancellationToken);


    // Update user
    void UpdateUser(int userId, User user);
    Task UpdateUser(int UserId, User user, CancellationToken cancellationToken);


    // Get user
    UserProfileDto GetUser(int userId);
    Task<UserProfileDto> GetUser(int userId, CancellationToken cancellationToken);
    Task<int> GetUserId(string email, CancellationToken cancellationToken);
    
    // Get users
    UsersViewModel GetUsers();
    Task<UsersViewModel> GetUsers(CancellationToken cancellationToken);

    // Get user achievements
    UserAchievementsViewModel GetUserAchievements(int userId);
    Task<UserAchievementsViewModel> GetUserAchievements(int userId, CancellationToken cancellationToken);

    // Get user rewards
    UserRewardsViewModel GetUserRewards(int userId);
    Task<UserRewardsViewModel> GetUserRewards(int userId, CancellationToken cancellationToken);

    // Get current user
    CurrentUserDto GetCurrentUser();
    Task<CurrentUserDto> GetCurrentUser(CancellationToken cancellationToken);

    // Get user's QR code
    string GetStaffQRCode(string emailAddress);
    Task<string> GetStaffQRCode(string emailAddress, CancellationToken cancellationToken);
    
    // Get user's pending redemptions
    UserPendingRedemptionsViewModel GetUserPendingRedemptions(int userId);
    Task<UserPendingRedemptionsViewModel> GetUserPendingRedemptions(int userId, CancellationToken cancellationToken);
}

using SSW.Rewards.Application.Users.Queries.GetCurrentUser;
using SSW.Rewards.Application.Users.Queries.GetUser;
using SSW.Rewards.Application.Users.Queries.GetUserAchievements;
using SSW.Rewards.Application.Users.Queries.GetUserRewards;
using SSW.Rewards.Domain.Entities;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SSW.Rewards.Application.Users.Common.Interfaces
{
    public interface IUserService
    {
        // Get roles
        IEnumerable<Role> GetUserRoles(int userId);
        Task<IEnumerable<Role>> GetUserRoles(int userId, CancellationToken cancellationToken);


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
        UserViewModel GetUser(int userId);
        Task<UserViewModel> GetUser(int userId, CancellationToken cancellationToken);


        // Get user achievements
        UserAchievementsViewModel GetUserAchievements(int userId);
        Task<UserAchievementsViewModel> GetUserAchievements(int userId, CancellationToken cancellationToken);

        // Get user rewards
        UserRewardsViewModel GetUserRewards(int userId);
        Task<UserRewardsViewModel> GetUserRewards(int userId, CancellationToken cancellationToken);

        // Get current user
        CurrentUserViewModel GetCurrentUser();
        Task<CurrentUserViewModel> GetCurrentUser(CancellationToken cancellationToken);

        // Get user's QR code
        string GetStaffQRCode(string emailAddress);
        Task<string> GetStaffQRCode(string emailAddress, CancellationToken cancellationToken);
    }
}

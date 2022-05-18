using SSW.Rewards.Domain.Entities;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SSW.Rewards.Application.Users.Common.Interfaces
{
    public interface IUserService
    {
        // Get roles
        IEnumerable<Role> GetUserRoles();

        IEnumerable<Role> GetUserRoles(int userId);

        Task<IEnumerable<Role>> GetUserRoles(CancellationToken cancellationToken);

        Task<IEnumerable<Role>> GetUserRoles(int userId, CancellationToken cancellationToken);


        // Add roles
        int AddRole(Role role);

        Task<int> AddRole(Role role, CancellationToken cancellationToken);


        // Add user roles
        void AddUserRole(User user, Role role);

        Task AddUserRole(User user, Role role, CancellationToken cancellationToken);


        // Remove user roles
        void RemoveUserRole(User user, Role role);

        Task RemoveUserRole(User user, Role role, CancellationToken cancellationToken);


        // Create user
        int CreateUser(User user);

        Task<int> CreateUser(User user, CancellationToken cancellationToken);


        // Update user
        void UpdateUser(int userId, User user);

        Task UpdateUser(int UserId, User user, CancellationToken cancellationToken);


        // Get user
    }
}

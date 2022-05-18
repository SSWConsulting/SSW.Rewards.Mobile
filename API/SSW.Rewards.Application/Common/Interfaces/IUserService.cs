using SSW.Rewards.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SSWUser = SSW.Rewards.Domain.Entities.User;

namespace SSW.Rewards.Application.Common.Interfaces
{
    public interface IUserService
    {
        IEnumerable<Role> GetUserRoles();

        IEnumerable<Role> GetUserRoles(int userId);

        Task<IEnumerable<Role>> GetUserRoles(CancellationToken cancellationToken);

        Task<IEnumerable<Role>> GetUserRoles(int userId, CancellationToken cancellationToken);

        Task<int> CreateUser(SSWUser user);
    }
}

using SSW.Rewards.Domain.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace SSW.Rewards.Application.Common.Interfaces
{
    public interface IRolesService
    {
        // Add roles
        int AddRole(Role role);
        Task<int> AddRole(Role role, CancellationToken cancellationToken);
    }
}

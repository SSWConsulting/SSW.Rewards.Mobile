

namespace SSW.Rewards.Application.Common.Interfaces;

public interface IRolesService
{
    // Add roles
    Task<int> AddRole(Role role, CancellationToken cancellationToken);
}

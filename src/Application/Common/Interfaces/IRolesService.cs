using SSW.Rewards.Domain.Entities;

namespace SSW.Rewards.Application.Common.Interfaces;

public interface IRolesService
{
    // Add roles
    int AddRole(Role role);
    Task<int> AddRole(Role role, CancellationToken cancellationToken);
}

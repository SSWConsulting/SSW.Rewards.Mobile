using System.Collections.Generic;
using System.Threading.Tasks;

namespace SSW.Rewards.Services
{
    public interface ILeaderService
    {
        Task<IEnumerable<LeaderboardUserDto>> GetLeadersAsync(bool forceRefresh);
    }
}

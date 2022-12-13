using SSW.Rewards.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SSW.Rewards.Services
{
    public interface IDevService
    {
        Task<IEnumerable<DevProfile>> GetProfilesAsync();
    }
}

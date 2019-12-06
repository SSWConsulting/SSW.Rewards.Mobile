using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SSW.Rewards.Models;

namespace SSW.Rewards.Services
{
    public interface IDevService
    {
        Task<IEnumerable<DevProfile>> GetProfilesAsync();
    }
}

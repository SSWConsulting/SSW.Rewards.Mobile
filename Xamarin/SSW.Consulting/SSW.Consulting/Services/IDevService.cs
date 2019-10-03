using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SSW.Consulting.Models;

namespace SSW.Consulting.Services
{
    public interface IDevService
    {
        Task<IEnumerable<DevProfile>> GetProfilesAsync();
    }
}

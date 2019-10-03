using SSW.Consulting.Models;
using SSW.Consulting.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SSW.Consulting.Services
{
    public interface ILeaderService
    {
        Task<IEnumerable<LeaderSummary>> GetLeadersAsync(bool forceRefresh);
    }
}

using SSW.Rewards.Models;
using SSW.Rewards.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SSW.Rewards.Services
{
    public interface ILeaderService
    {
        Task<IEnumerable<LeaderSummary>> GetLeadersAsync(bool forceRefresh);
    }
}

using SSW.Consulting.Models;
using SSW.Consulting.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SSW.Consulting.Services
{
    public interface IRewardService
    {
        Task<IEnumerable<Reward>> GetAllRewardsAsync();
        Task<IEnumerable<Reward>> GetRewardsForUserAsync(int userId);
        Task<ChallengeResultViewModel> PostRewardAsync(string qrCode);
    }
}

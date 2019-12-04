using SSW.Rewards.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SSW.Rewards.Services
{
    public interface IAchievementService
    {
        Task<PostAchievementResult> PostAchievementAsync(string qrCode);
    }
}

using SSW.Consulting.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SSW.Consulting.Services
{
    public interface IAchievementService
    {
        Task<PostAchievementResult> PostAchievementAsync(string qrCode);
    }
}

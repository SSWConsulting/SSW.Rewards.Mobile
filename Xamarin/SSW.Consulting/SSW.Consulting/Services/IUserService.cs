using SSW.Consulting.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SSW.Consulting.Services
{
    public interface IUserService
    {
        Task<int> GetMyUserIdAsync();
        Task<string> GetMyNameAsync();
        Task<string> GetMyEmailAsync();
        Task<string> GetMyProfilePicAsync();
        Task<int> GetMyPointsAsync();
        Task<string> GetTokenAsync();
        Task<ApiStatus> SignInAsync();
        void SignOut();
        Task<bool> IsLoggedInAsync();
        Task UpdateMyDetailsAsync();
        Task<IEnumerable<MyChallenge>> GetOThersAchievementsAsync(int userId);
    }
}

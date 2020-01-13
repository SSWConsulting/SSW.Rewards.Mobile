using SSW.Rewards.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace SSW.Rewards.Services
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
        Task<IEnumerable<Achievement>> GetAchievementsAsync();
        Task<IEnumerable<Achievement>> GetAchievementsAsync(int userId);
        Task<IEnumerable<Reward>> GetRewardsAsync();
        Task<IEnumerable<Reward>> GetRewardsAsync(int userId);
        Task<ImageSource> GetAvatarAsync(string url);
        Task<string> UploadImageAsync(Stream image);
    }
}

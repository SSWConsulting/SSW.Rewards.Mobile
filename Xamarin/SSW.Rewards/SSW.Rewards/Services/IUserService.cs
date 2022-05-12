using SSW.Rewards.Models;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace SSW.Rewards.Services
{
    public interface IUserService
    {
        // values
        int MyUserId { get; }
        string MyName { get; }
        string MyEmail { get; }
        string MyProfilePic { get; }
        int MyPoints { get; }
        string MyQrCode { get; }
        bool IsLoggedIn { get; }

        // auth methods
        Task<ApiStatus> SignInAsync();
        Task ResetPassword();
        Task RefreshLoginAsync();
        void SignOut();

        // user details
        Task UpdateMyDetailsAsync();
        Task<IEnumerable<Achievement>> GetAchievementsAsync();
        Task<IEnumerable<Achievement>> GetAchievementsAsync(int userId);
        Task<IEnumerable<Reward>> GetRewardsAsync();
        Task<IEnumerable<Reward>> GetRewardsAsync(int userId);
        Task<ImageSource> GetAvatarAsync(string url);
        Task<string> UploadImageAsync(Stream image);        
    }
}

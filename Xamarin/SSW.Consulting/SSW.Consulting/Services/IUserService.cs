using SSW.Consulting.Models;
using System;
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
        Task SignOutAsync();
        Task<bool> IsLoggedInAsync();
        Task UpdateMyDetailsAsync();
    }
}

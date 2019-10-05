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
        Task<bool> SignInAsync();
        Task SignOutAsync();
        Task<bool> IsLoggedInAsync();
    }
}

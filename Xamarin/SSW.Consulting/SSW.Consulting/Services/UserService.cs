using System;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace SSW.Consulting.Services
{
    public class UserService : IUserService
    {
        public UserService()
        {
        }

        public async Task<string> GetMyEmailAsync()
        {
            return await Task.FromResult(Preferences.Get("MyEmail", string.Empty));
        }

        public async Task<string> GetMyNameAsync()
        {
            return await Task.FromResult(Preferences.Get("MyEmail", string.Empty));
        }

        public async Task<string> GetMyPointsAsync()
        {
            return await Task.FromResult(Preferences.Get("MyEmail", string.Empty));
        }

        public async Task<string> GetMyProfilePicAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<int> GetMyUserIdAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<string> GetTokenAsync()
        {
            return await SecureStorage.GetAsync("auth_token");
        }

        public async Task<bool> IsLoggedInAsync()
        {
            return await Task.FromResult(Preferences.Get("LoggedIn", false));
        }

        public async Task SetTokenAsync(string token)
        {
            await SecureStorage.SetAsync("auth_token", token);
        }
    }
}

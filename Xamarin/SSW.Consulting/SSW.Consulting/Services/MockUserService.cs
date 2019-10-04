using System;
using System.Threading.Tasks;
using Microsoft.AppCenter.Auth;

namespace SSW.Consulting.Services
{
    public class MockUserService : IUserService
    {
        public MockUserService()
        {
        }

        public async Task<string> GetMyEmailAsync()
        {
            return await Task.FromResult("mattgoldman@ssw.com.au");
        }

        public async Task<string> GetMyNameAsync()
        {
            return await Task.FromResult("Matt Goldman");
        }

        public async Task<string> GetMyPointsAsync()
        {
            return await Task.FromResult("136");
        }

        public async Task<string> GetMyProfilePicAsync()
        {
            return await Task.FromResult("MattGMain");
        }

        public async Task<int> GetMyUserIdAsync()
        {
            return await Task.FromResult(4);
        }

        public Task<string> GetTokenAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<bool> IsLoggedInAsync()
        {
            return await Task.FromResult(true);
        }

        public Task SetTokenAsync(string token)
        {
            throw new NotImplementedException();
        }

        public Task SignInAsync(UserInformation userInfo)
        {
            throw new NotImplementedException();
        }

        public Task SignOutAsync()
        {
            throw new NotImplementedException();
        }
    }
}

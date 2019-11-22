using SSW.Consulting.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SSW.Consulting.Services
{
    public class MockUserService : IUserService
    {
        public async Task<string> GetMyEmailAsync()
        {
            return await Task.FromResult("mattgoldman@ssw.com.au");
        }

        public async Task<string> GetMyNameAsync()
        {
            return await Task.FromResult("Matt Goldman");
        }

        public async Task<int> GetMyPointsAsync()
        {
            return await Task.FromResult(136);
        }

        public async Task<string> GetMyProfilePicAsync()
        {
            return await Task.FromResult("MattGMain");
        }

        public async Task<int> GetMyUserIdAsync()
        {
            return await Task.FromResult(4);
        }

        public Task<IEnumerable<MyChallenge>> GetOthersAchievementsAsync(int userId)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetTokenAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<bool> IsLoggedInAsync()
        {
            return await Task.FromResult(true);
        }

        public Task<bool> SetMyEmailAsync(string email)
        {
            throw new NotImplementedException();
        }

        public Task<bool> SetMyNameAsync(string name)
        {
            throw new NotImplementedException();
        }

        public Task<bool> SetMyPointsAsync(int points)
        {
            throw new NotImplementedException();
        }

        public Task<bool> SetMyProfilePicAsync(string picUri)
        {
            throw new NotImplementedException();
        }

        public Task<bool> SetMyUserIdAsync(int userId)
        {
            throw new NotImplementedException();
        }

        public Task<bool> SetTokenAsync(string token)
        {
            throw new NotImplementedException();
        }

        public async Task<ApiStatus> SignInAsync()
        {
            return await Task.FromResult(ApiStatus.Success);
        }

        public void SignOut()
        {
            throw new NotImplementedException();
        }

        public Task SignOutAsync()
        {
            throw new NotImplementedException();
        }

        public Task UpdateMyDetailsAsync()
        {
            throw new NotImplementedException();
        }
    }
}

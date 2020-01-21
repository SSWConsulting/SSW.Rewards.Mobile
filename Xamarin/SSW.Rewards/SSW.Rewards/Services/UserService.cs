using System;
using System.Threading.Tasks;
using Microsoft.AppCenter.Auth;
using Xamarin.Essentials;
using System.Linq;
using System.IdentityModel.Tokens.Jwt;
using Xamarin.Forms;
using System.Net.Http;
using System.Net.Http.Headers;
using SSW.Rewards.Models;
using System.Collections.Generic;
using System.IO;

namespace SSW.Rewards.Services
{
    public class UserService : IUserService
    {
        private UserClient _userClient { get; set; }
        private HttpClient _httpClient { get; set; }

        public async Task<string> GetMyEmailAsync()
        {
            return await Task.FromResult(Preferences.Get("MyEmail", string.Empty));
        }

        public async Task<string> GetMyNameAsync()
        {
            return await Task.FromResult(Preferences.Get("MyName", string.Empty));
        }

        public async Task<int> GetMyPointsAsync()
        {
            return await Task.FromResult(Preferences.Get("MyPoints", 0));
        }

        public async Task<string> GetMyProfilePicAsync()
        {
            string profilePic = await Task.FromResult(Preferences.Get("MyProfilePic", string.Empty));
            if (!string.IsNullOrWhiteSpace(profilePic))
                return profilePic;

            return "icon_avatar";
        }

        public async Task<int> GetMyUserIdAsync()
        {
            return await Task.FromResult(Preferences.Get("MyUserId", 0));
        }

        public async Task<string> GetTokenAsync()
        {
            return await SecureStorage.GetAsync("auth_token");
        }

        public async Task<bool> IsLoggedInAsync()
        {
            return await Task.FromResult(Preferences.Get("LoggedIn", false));
        }

        public async Task<string> UploadImageAsync(Stream image)
        {

            if (_userClient == null)
            {
                if (_httpClient == null)
                {
                    string token = await SecureStorage.GetAsync("auth_token");
                    _httpClient = new HttpClient();
                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }

                _userClient = new UserClient(Constants.ApiBaseUrl, _httpClient);
            }


            FileParameter parameter = new FileParameter(image);
            
            string newPicUri = await _userClient.UploadProfilePicAsync(parameter);
            Preferences.Set("MyProfilePic", newPicUri);
            return newPicUri;
        }

        public async Task<ApiStatus> SignInAsync()
        {
            try
            {
                UserInformation userInfo = await Auth.SignInAsync();
                // Sign-in succeeded.
                string accountId = userInfo.AccountId;
                string token = userInfo.AccessToken;
                if (!string.IsNullOrWhiteSpace(accountId) && !string.IsNullOrWhiteSpace(token))
                {
                    await SecureStorage.SetAsync("auth_token", token);

                    var tokenHandler = new JwtSecurityTokenHandler();

                    try
                    {
                        var jwToken = tokenHandler.ReadJwtToken(userInfo.IdToken);

                        var firstName = jwToken.Claims.FirstOrDefault(t => t.Type == "given_name")?.Value;
                        var familyName = jwToken.Claims.FirstOrDefault(t => t.Type == "family_name")?.Value;
                        var jobTitle = jwToken.Claims.FirstOrDefault(t => t.Type == "jobTitle")?.Value;
                        var email = jwToken.Claims.FirstOrDefault(t => t.Type == "emails")?.Value;

                        string fullName = firstName + " " + familyName;

                        if (!string.IsNullOrWhiteSpace(fullName))
                        {
                            Preferences.Set("MyName", fullName);
                        }

                        if (!string.IsNullOrWhiteSpace(jobTitle))
                        {
                            Preferences.Set("JobTitle", jobTitle);
                        }

                        if (!string.IsNullOrWhiteSpace(email))
                        {
                            Preferences.Set("MyEmail", email);
                        }

                        _httpClient = new HttpClient();
                        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                        string baseUrl = Constants.ApiBaseUrl;

                        _userClient = new UserClient(baseUrl, _httpClient);

                        var user = await _userClient.GetAsync();

                        Preferences.Set("MyUserId", user.Id);
                        Preferences.Set("MyProfilePic", user.ProfilePic);

                        if (!string.IsNullOrWhiteSpace(user.Points.ToString()))
                        {
                            Preferences.Set("MyPoints", user.Points);
                        }

                        Preferences.Set("LoggedIn", true);
                        return ApiStatus.Success;
                    }
                    catch(ArgumentException)
                    {
                        //TODO: Handle error decoding JWT
                        return ApiStatus.Error;
                    }
                }
                else
                {
                    return ApiStatus.LoginFailure;
                }
            }

            catch (ApiException e)
            {
                if(e.StatusCode == 404)
                {
                    return ApiStatus.Unavailable;
                }
                else if(e.StatusCode == 401)
                {
                    return ApiStatus.LoginFailure;
                }

                return ApiStatus.Error;
                
            }
        }

        public void SignOut()
        {
            Auth.SignOut();
            SecureStorage.RemoveAll();
            Preferences.Clear();
        }

        public async Task UpdateMyDetailsAsync()
        {
            if(_userClient == null)
            {
                if(_httpClient == null)
                {
                    string token = await SecureStorage.GetAsync("auth_token");
                    _httpClient = new HttpClient();
                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }

                _userClient = new UserClient(Constants.ApiBaseUrl, _httpClient);
            }

            var user = await _userClient.GetAsync();

            if (!string.IsNullOrWhiteSpace(user.FullName))
            {
                Preferences.Set("MyName", user.FullName);
            }
            
            if (!string.IsNullOrWhiteSpace(user.Email))
            {
                Preferences.Set("MyEmail", user.Email);
            }

            if(!string.IsNullOrWhiteSpace(user.Id.ToString()))
            {
                Preferences.Set("MyUserId", user.Id);
            }

            if(!string.IsNullOrWhiteSpace(user.ProfilePic))
            {
                Preferences.Set("MyProfilePic", user.ProfilePic);
            }

            if(!string.IsNullOrWhiteSpace(user.Points.ToString()))
            {
                Preferences.Set("MyPoints", user.Points);
            }
        }

        public async Task<IEnumerable<Achievement>> GetAchievementsAsync()
        {
            return await GetAchievementsForUserAsync(await GetMyUserIdAsync());
        }

        public async Task<IEnumerable<Achievement>> GetAchievementsAsync(int userId)
        {
            return await GetAchievementsForUserAsync(userId);
        }

        private async Task<IEnumerable<Achievement>> GetAchievementsForUserAsync(int userId)
        {
            List<Achievement> achievements = new List<Achievement>();

            if (_userClient == null)
            {
                if (_httpClient == null)
                {
                    string token = await SecureStorage.GetAsync("auth_token");
                    _httpClient = new HttpClient();
                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }

                _userClient = new UserClient(Constants.ApiBaseUrl, _httpClient);
            }

            var achievementsList = await _userClient.AchievementsAsync(userId);

            foreach(UserAchievementViewModel achievement in achievementsList.UserAchievements)
            {
                achievements.Add(new Achievement
                {
                    Complete = achievement.Complete,
                    Name = achievement.AchievementName,
                    Value = achievement.AchievementValue
                });
            }

            return achievements;
        }

        public async Task<IEnumerable<Reward>> GetRewardsAsync()
        {
            return await GetRewardsForUserAsync(await GetMyUserIdAsync());
        }

        public async Task<IEnumerable<Reward>> GetRewardsAsync(int userId)
        {
            return await GetRewardsForUserAsync(userId);
        }

        private async Task<IEnumerable<Reward>> GetRewardsForUserAsync(int userId)
        {
            List<Reward> rewards = new List<Reward>();

            if (_userClient == null)
            {
                if (_httpClient == null)
                {
                    string token = await SecureStorage.GetAsync("auth_token");
                    _httpClient = new HttpClient();
                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }

                _userClient = new UserClient(Constants.ApiBaseUrl, _httpClient);
            }

            var rewardsList = await _userClient.RewardsAsync(userId);

            foreach (UserRewardViewModel userReward in rewardsList.UserRewards)
            {
                rewards.Add(new Reward
                {
                    Awarded = userReward.Awarded,
                    Name = userReward.RewardName,
                    Cost = userReward.RewardCost
                });
            }

            return rewards;
        }

        public Task<ImageSource> GetProfilePicAsync(string url)
        {
            throw new NotImplementedException();
        }

        public Task<ImageSource> GetAvatarAsync(string url)
        {
            throw new NotImplementedException();
        }
    }
}

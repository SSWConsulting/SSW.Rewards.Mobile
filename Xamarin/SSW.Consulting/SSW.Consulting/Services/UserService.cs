using System;
using System.Threading.Tasks;
using Microsoft.AppCenter.Auth;
using Xamarin.Essentials;
using System.Linq;
using System.IdentityModel.Tokens.Jwt;
using Xamarin.Forms;
using System.Net.Http;
using System.Net.Http.Headers;
using SSW.Consulting.Models;
using System.Collections.Generic;

namespace SSW.Consulting.Services
{
    public class UserService : IUserService
    {
        private UserClient _userClient { get; set; }
        private HttpClient _httpClient { get; set; }

        public UserService()
        {
        }

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
                        Preferences.Set("MyProfilePic", user.Picture);

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
            await Task.Run(Auth.SignOut);
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

            if(!string.IsNullOrWhiteSpace(user.Picture))
            {
                Preferences.Set("MyProfilePic", user.Picture);
            }

            if(!string.IsNullOrWhiteSpace(user.Points.ToString()))
            {
                Preferences.Set("MyPoints", user.Points);
            }
        }
        
        public async Task<IEnumerable<MyChallenge>> GetOThersAchievementsAsync(int userId)
        {
            List<MyChallenge> challenges = new List<MyChallenge>();

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

            var SOCA = await _userClient.AchievementsAsync(userId);

            foreach(var achievement in SOCA.UserAchievements)
            {
                challenges.Add(new MyChallenge
                {
                    Completed = achievement.Complete,
                    Points = achievement.AchievementValue,
                    Title = achievement.AchievementName,
                    awardedAt = achievement.AwardedAt,
                    IsBonus = achievement.AchievementValue == 0 ? true : false
                });
            }

            return challenges;
        }
    }
}

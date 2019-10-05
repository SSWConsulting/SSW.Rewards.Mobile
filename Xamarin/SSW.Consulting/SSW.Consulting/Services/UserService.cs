using System;
using System.Threading.Tasks;
using Microsoft.AppCenter.Auth;
using Xamarin.Essentials;
using System.Linq;
using System.IdentityModel.Tokens.Jwt;
using Xamarin.Forms;
using System.Net.Http;
using System.Net.Http.Headers;

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
            return await Task.FromResult(Preferences.Get("MyEmail", string.Empty));
        }

        public async Task<int> GetMyPointsAsync()
        {
            return await Task.FromResult(Preferences.Get("MyPoints", 0));
        }

        public async Task<string> GetMyProfilePicAsync()
        {
            return await Task.FromResult(Preferences.Get("MyProfilePic", string.Empty));
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

        public async Task<bool> SignInAsync()
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
                        //TODO: Don't hard code this URL
                        _userClient = new UserClient("https://sswconsulting-dev.azurewebsites.net", _httpClient);

                        var user = await _userClient.GetAsync();

                        Preferences.Set("MyUserId", user.Id);
                        Preferences.Set("MyProfilePic", user.Picture);

                        Preferences.Set("LoggedIn", true);
                        return true;
                    }
                    catch(ArgumentException)
                    {
                        //TODO: Handle error decoding JWT
                        return false;
                    }
                }
                else
                {
                    //TODO: handle login error
                    return false;
                }
            }

            catch (Exception e)
            {
                // Do something with sign-in failure.
                Console.Write(e);
                return false;
            }
        }

        public async Task SignOutAsync()
        {
            Auth.SignOut();
            SecureStorage.RemoveAll();
            Preferences.Clear();
        }
    }
}

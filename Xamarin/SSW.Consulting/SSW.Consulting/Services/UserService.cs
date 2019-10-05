using System;
using System.Threading.Tasks;
using Microsoft.AppCenter.Auth;
using Xamarin.Essentials;
using System.Linq;
using System.IdentityModel.Tokens.Jwt;
using Xamarin.Forms;

namespace SSW.Consulting.Services
{
    public class UserService : IUserService
    {
        public UserService()
        {
        }

        public async Task<string> GetMyEmailAsync()
        {
            //return await Task.FromResult(Preferences.Get("MyEmail", string.Empty));
            return await Task.FromResult("mattgoldman@ssw.com.au");
        }

        public async Task<string> GetMyNameAsync()
        {
            //return await Task.FromResult(Preferences.Get("MyEmail", string.Empty));
            return await Task.FromResult("Matt Goldman");
        }

        public async Task<string> GetMyPointsAsync()
        {
            //return await Task.FromResult(Preferences.Get("MyEmail", string.Empty));
            return await Task.FromResult("136");
        }

        public async Task<string> GetMyProfilePicAsync()
        {
            //throw new NotImplementedException();
            return await Task.FromResult("MattGMain");
        }

        public async Task<int> GetMyUserIdAsync()
        {
            //throw new NotImplementedException();
            return await Task.FromResult(4);
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

                        var claimValue = jwToken.Claims.FirstOrDefault(t => t.Type == "claimtype e.g. name")?.Value;

                        if(!string.IsNullOrWhiteSpace(claimValue))
                        {
                            Preferences.Set("Claim", claimValue);
                        }

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
            SecureStorage.RemoveAll();// SetAsync("auth_token", string.Empty);
            Preferences.Clear();
        }
    }
}

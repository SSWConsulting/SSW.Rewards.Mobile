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

        public  async Task SignInAsync(UserInformation userInfo)
        {
            try
            {
                // Sign-in succeeded.
                string accountId = userInfo.AccountId;
                if (!string.IsNullOrWhiteSpace(accountId))
                {
                    await SecureStorage.SetAsync("auth_token", accountId);

                    var tokenHandler = new JwtSecurityTokenHandler();

                    try
                    {
                        var jwToken = tokenHandler.ReadJwtToken(userInfo.IdToken);

                        var claimValue = jwToken.Claims.FirstOrDefault(t => t.Type == "claimtype e.g. name")?.Value;

                        if(!string.IsNullOrWhiteSpace(claimValue))
                        {
                            Preferences.Set("Claim", claimValue);
                        }

                    }
                    catch(ArgumentException)
                    {
                        //TODO: Handle error decoding JWT
                    }


                    Preferences.Set("LoggedIn", true);
                }
                else
                {
                    //TODO: handle login error
                }
            }

            catch (Exception e)
            {
                // Do something with sign-in failure.
                Console.Write(e);
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

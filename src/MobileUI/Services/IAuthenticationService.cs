using System.IdentityModel.Tokens.Jwt;
using IdentityModel.Client;
using IdentityModel.OidcClient;
using Microsoft.AppCenter.Crashes;
using IBrowser = IdentityModel.OidcClient.Browser.IBrowser;

namespace SSW.Rewards.Mobile.Services;

public interface IAuthenticationService
{
    Task<ApiStatus> SignInAsync();
    Task<string> GetAccesstoken();
    void SignOut();
}

public class AuthenticationService : IAuthenticationService
{
    private readonly OidcClientOptions _options;

    private bool _loggedIn = false;

    private string RefreshToken;
    
    private string _accessToken;
    
    private DateTimeOffset _tokenExpiry;
    
    private bool HasCachedAccount { get => Preferences.Get(nameof(HasCachedAccount), false); }
    
    public AuthenticationService(IBrowser browser)
    {
        _options = new OidcClientOptions
        {
            Authority = Constants.AuthorityUri,
            ClientId = Constants.ClientId,
            Scope = Constants.Scope,
            RedirectUri = Constants.AuthRedirectUrl,
            Browser = browser,
        };
    }
    
    public async Task<ApiStatus> SignInAsync()
    {
        try
        {
            var oidcClient = new OidcClient(_options);

            var result = await oidcClient.LoginAsync(new LoginRequest());

            if (result.IsError)
            {
                return ApiStatus.Error;
            }

            return await SetLoggedInState(result);
        }
        catch (TaskCanceledException taskEx)
        {
            return ApiStatus.LoginFailure;
        }
        catch (Exception ex)
        {
            return ApiStatus.Error;
        }
    }

    public Task<string> GetAccesstoken()
    {
        throw new NotImplementedException();
    }

    public void SignOut()
    {
        // TODO: remove from auth client
        SecureStorage.RemoveAll();
        Preferences.Clear();
    }

    private async Task<ApiStatus> SetLoggedInState(LoginResult loginResult)
    {
        if (!string.IsNullOrWhiteSpace(loginResult.IdentityToken) && !string.IsNullOrWhiteSpace(loginResult.AccessToken))
        {

            try
            {
                _accessToken = loginResult.AccessToken;
                _tokenExpiry = loginResult.AccessTokenExpiration;
                
                await SettRefreshToken(loginResult.RefreshToken);
                
                Preferences.Set(nameof(HasCachedAccount), true);

                var tokenHandler = new JwtSecurityTokenHandler();

                var jwToken = tokenHandler.ReadJwtToken(loginResult.IdentityToken);

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

                await UpdateMyDetailsAsync();

                _loggedIn = true;
            }
            catch (Exception ex)
            {

                return ApiStatus.Unavailable;
            }

            return ApiStatus.Success;
        }
        else
        {
            return ApiStatus.LoginFailure;
        }
    }

    private async Task SettRefreshToken(string token)
    {
        RefreshToken = token;

        await SecureStorage.SetAsync(nameof(RefreshToken), token);
    }

    public async Task<bool> RefreshLoginAsync()
    {
        RefreshToken = await SecureStorage.GetAsync(nameof(RefreshToken));

        if (!string.IsNullOrWhiteSpace(RefreshToken))
        {
            var oidcClient = new OidcClient(_options);

            var result = await oidcClient.RefreshTokenAsync(RefreshToken);

            if (!result.IsError)
            {
                await SettRefreshToken(result.RefreshToken);
                await SetLoggedInState(result);
                return true;
            }
            else
            {
                Crashes.TrackError(new Exception($"{result.Error}, {result.ErrorDescription}"));

                var fcep = new Parameters
                {
                    { "prompt", "none" }
                };

                var silentRequest = new LoginRequest
                {
                    FrontChannelExtraParameters = fcep
                };

                var silentResult = await oidcClient.LoginAsync(silentRequest);

                if (!silentResult.IsError)
                {
                    string token = silentResult.AccessToken;
                    string idToken = silentResult.IdentityToken;

                    if (!string.IsNullOrWhiteSpace(idToken) && !string.IsNullOrWhiteSpace(token))
                    {

                        try
                        {
                            await SetLoggedInState(silentResult);
                        }
                        catch
                        {

                            return false;
                        }

                        await SettRefreshToken(result.RefreshToken);

                        return true;
                    }
                }

                await SignInAsync();
            }
        }

        return false;
    }
}
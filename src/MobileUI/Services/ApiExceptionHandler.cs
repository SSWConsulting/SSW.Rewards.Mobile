using SSW.Rewards.Mobile.Common;

namespace SSW.Rewards.Mobile.Services;

/// <summary>
/// MAUI-backed <see cref="IApiExceptionHandler"/>: a 401 ends the session and sends the user back to login.
/// Network failures are retried by the API client's resilience pipeline, so they arrive here only once
/// the retries have run out.
/// </summary>
public sealed class ApiExceptionHandler : IApiExceptionHandler
{
    public async Task<bool> TryHandleAsync(Exception exception)
    {
        if (!exception.IsUnauthorized())
        {
            return false;
        }

        // End the rejected session first: its tokens are useless, and its cached data must not be
        // shown to whoever signs in next.
        await App.SignOutAsync();

        await IPlatformApplication.Current.DisplayAlertAsync("Authentication Failure", "Your session has expired. Please log in again.", "OK");
        App.NavigateToLoginPage();
        return true;
    }
}

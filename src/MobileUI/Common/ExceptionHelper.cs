using System.Net;
using SSW.Rewards.Mobile.Common;

namespace SSW.Rewards.Mobile.Helpers;

// Technical debt: Replace this with Polly.
// see: https://github.com/SSWConsulting/SSW.Rewards.Mobile/issues/276
public static class ExceptionHandler
{
    public static async Task<bool> HandleApiException(Exception exception)
    {
        if (exception is HttpRequestException httpRequestException && IsUnauthorized(httpRequestException))
        {
            await NavigateToLoginPage();
            return true;
        }
        else
        {
            return false;
        }
    }

    private static bool IsUnauthorized(HttpRequestException exception)
    {
        // Check if the exception corresponds to a 401 Unauthorized response
        return exception.StatusCode == HttpStatusCode.Unauthorized;
    }

    private static async Task NavigateToLoginPage()
    {
        await IPlatformApplication.Current.DisplayAlertAsync("Authentication Failure", "Your session has expired. Please log in again.", "OK");
        App.NavigateToLoginPage();
    }
}

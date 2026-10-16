using SSW.Rewards.Mobile.Common;

namespace SSW.Rewards.Mobile.Helpers;

// Technical debt: Replace this with Polly.
// see: https://github.com/SSWConsulting/SSW.Rewards.Mobile/issues/276
public static class ExceptionHandler
{
    public static async Task<bool> HandleApiException(Exception exception)
    {
        if (exception.IsUnauthorized())
        {
            await NavigateToLoginPage();
            return true;
        }
        else
        {
            return false;
        }
    }

    private static async Task NavigateToLoginPage()
    {
        await IPlatformApplication.Current.DisplayAlertAsync("Authentication Failure", "Your session has expired. Please log in again.", "OK");
        App.NavigateToLoginPage();
    }
}

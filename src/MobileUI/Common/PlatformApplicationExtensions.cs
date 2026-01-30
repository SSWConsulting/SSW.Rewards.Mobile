namespace SSW.Rewards.Mobile.Common;

public static class PlatformApplicationExtensions
{
    public static async Task DisplayAlertAsync(this IPlatformApplication? app, string title, string message, string cancel)
    {
        var alertService = app?.Services.GetService<IAlertService>();
        if (alertService != null)
        {
            await alertService.DisplayAlertAsync(title, message, cancel);
        }
    }
}

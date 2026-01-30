namespace SSW.Rewards.Mobile.Services;

public interface IAlertService
{
    Task DisplayAlertAsync(string title, string message, string cancel);
    Task<bool> DisplayConfirmationAsync(string title, string message, string accept, string cancel);
}

public class AlertService : IAlertService
{
    public Task DisplayAlertAsync(string title, string message, string cancel)
        => MainThread.InvokeOnMainThreadAsync(() =>
        {
            var windows = Application.Current?.Windows;
            var page = windows is { Count: > 0 } ? windows[0].Page : null;
            return page?.DisplayAlertAsync(title, message, cancel) ?? Task.CompletedTask;
        });

    public Task<bool> DisplayConfirmationAsync(string title, string message, string accept, string cancel)
        => MainThread.InvokeOnMainThreadAsync(() =>
        {
            var windows = Application.Current?.Windows;
            var page = windows is { Count: > 0 } ? windows[0].Page : null;
            return page?.DisplayAlertAsync(title, message, accept, cancel) ?? Task.FromResult(false);
        });
}

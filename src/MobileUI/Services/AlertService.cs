namespace SSW.Rewards.Mobile.Services;

public interface IAlertService
{
    Task ShowAlertAsync(string title, string message, string cancel);
    Task<bool> ShowConfirmationAsync(string title, string message, string accept, string cancel);
}

public class AlertService : IAlertService
{
    public Task ShowAlertAsync(string title, string message, string cancel)
        => MainThread.InvokeOnMainThreadAsync(() =>
        {
            var windows = Application.Current?.Windows;
            var page = (windows != null && windows.Count > 0) ? windows[0].Page : null;
            return page?.DisplayAlertAsync(title, message, cancel) ?? Task.CompletedTask;
        });

    public Task<bool> ShowConfirmationAsync(string title, string message, string accept, string cancel)
        => MainThread.InvokeOnMainThreadAsync(() =>
        {
            var windows = Application.Current?.Windows;
            var page = (windows != null && windows.Count > 0) ? windows[0].Page : null;
            return page?.DisplayAlertAsync(title, message, accept, cancel) ?? Task.FromResult(false);
        });
}

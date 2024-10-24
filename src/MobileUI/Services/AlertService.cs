namespace SSW.Rewards.Mobile.Services;

public interface IAlertService
{
    Task DisplayAlert(string title, string message, string cancel);
    Task<bool> DisplayAlert(string title, string message, string accept, string cancel);
}

public class AlertService : IAlertService
{
    public async Task DisplayAlert(string title, string message, string cancel)
    {
        await App.Current.MainPage.DisplayAlert(title, message, cancel);
    }
    
    public async Task<bool> DisplayAlert(string title, string message, string accept, string cancel)
    {
        return await App.Current.MainPage.DisplayAlert(title, message, accept, cancel);
    }
}

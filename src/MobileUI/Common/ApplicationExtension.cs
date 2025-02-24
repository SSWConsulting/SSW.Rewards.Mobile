namespace SSW.Rewards.Mobile.Common;
public static class ApplicationExtension
{
    public static async Task InitializeMainPage(this Application application)
    {
        if (!(application.MainPage is AppShell))
        {
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                application.MainPage = new AppShell();
            });
        }

        await Shell.Current.GoToAsync("//main");
    }
}

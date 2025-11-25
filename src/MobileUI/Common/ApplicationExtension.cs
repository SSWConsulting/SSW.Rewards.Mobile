namespace SSW.Rewards.Mobile.Common;

public static class ApplicationExtension
{
    public static void InitializeMainPage(this Application application)
    {
        MainThread.BeginInvokeOnMainThread(() => application.Windows[0].Page = new AppShell());
    }
}

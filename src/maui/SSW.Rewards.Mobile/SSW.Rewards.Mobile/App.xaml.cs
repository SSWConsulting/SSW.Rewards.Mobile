using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Mopups.Services;

namespace SSW.Rewards.Mobile;

public partial class App : Application
{
    public static object UIParent { get; set; }

    #region TECHNICAL_DEBT
    // TECHNICAL DEBT:  This section is here because of a bug in Xamarin.Forms.
    //                  It is not possible to dynamically change menu items in
    //                  Shell at runtime. Instead, we load a version of Shell
    //                  constructed as required. If this bug has been resolved
    //                  we can do a fair bit of refactoring. 🤮🤮🤮
    private static IServiceProvider _serviceProvider;
    private readonly LoginPageViewModel loginPageViewModel;

    public static void SetScope(IServiceCollection services)
    {
        _serviceProvider = services.BuildServiceProvider();
    }

    public static AppShell ResolveShell(bool isStaff)
    {
        var resolvedShell = ActivatorUtilities.CreateInstance<AppShell>(_serviceProvider, isStaff);

        return resolvedShell;
    }
    #endregion

    public App(IUserService userService)
    {
        InitializeComponent();

        AppCenter.Start("android=" + Constants.AppCenterAndroidId + ";" +
            "ios=e33283b1-7326-447d-baae-e783ece0789b",
            typeof(Analytics), typeof(Crashes));

        MainPage = new AppShell(userService);
    }

    protected override async void OnStart()
    {
        //await UpdateAccessTokenAsync();
        await CheckApiCompatibilityAsync();

        // HACK - Resource dictotionary isn't available here :(
        // See discussion: https://github.com/dotnet/maui/discussions/5263
        //MainPage = new LoginPage(loginPageViewModel);

        await App.Current.MainPage.Navigation.PushModalAsync<LoginPage>();
    }

    protected override void OnSleep()
    {
        // Handle when your app sleeps
    }

    protected override void OnResume()
    {
        // Handle when your app resumes
    }

    private async Task CheckApiCompatibilityAsync()
    {
        try
        {
            ApiInfo info = new ApiInfo(Constants.ApiBaseUrl);

            bool compatible = await info.IsApiCompatibleAsync();

            if (!compatible)
            {
                await Application.Current.MainPage.DisplayAlert("Update Required", "Looks like you're using an older version of the app. You can continue, but some features may not function as expected.", "OK");
            }
        }
        catch (Exception ex)
        {
            // TODO: log these instead to AppCenter
            Console.WriteLine("[App] ERROR checking API compat");
            Console.WriteLine($"[App] {ex.Message}");
            Console.WriteLine($"[App {ex.StackTrace}");
        }
    }
}
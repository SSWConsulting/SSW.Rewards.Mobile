using Mopups.Services;

namespace SSW.Rewards.Mobile;

public partial class App : Application
{
    private static IServiceProvider _provider;
    private static IAuthenticationService _authService;
    public static object UIParent { get; set; }

    public App(LoginPage page, IServiceProvider serviceProvider, IAuthenticationService authService)
    {
        _provider = serviceProvider;
        _authService = authService;
        
        InitializeComponent();
        Current.UserAppTheme = AppTheme.Dark;

        MainPage = page;
    }

    protected override async void OnStart()
    {
        //await UpdateAccessTokenAsync();
        await CheckApiCompatibilityAsync();

        // HACK - Resource dictionary isn't available here :(
        // See discussion: https://github.com/dotnet/maui/discussions/5263
        //MainPage = new LoginPage(loginPageViewModel);

        //await App.Current.MainPage.Navigation.PushModalAsync<LoginPage>();
    }

    protected override void OnSleep()
    {
        // Handle when your app sleeps
    }

    protected override void OnResume()
    {
        // Handle when your app resumes
    }
    
    protected override async void OnAppLinkRequestReceived(Uri uri)
    {
        base.OnAppLinkRequestReceived(uri);
        
        if (uri.Scheme != ApiClientConstants.RewardsQRCodeProtocol)
        {
            return;
        }

        var queryDictionary = System.Web.HttpUtility.ParseQueryString(uri.Query);
        var code = queryDictionary.Get(ApiClientConstants.RewardsQRCodeProtocolQueryName);
        
        if (_authService.IsLoggedIn)
        {
            var vm = ActivatorUtilities.CreateInstance<ScanResultViewModel>(_provider);
            var popup = new PopupPages.ScanResult(vm, code);
            await MopupService.Instance.PushAsync(popup);
        }
        else
        {
            ((LoginPage)MainPage)?.QueueCodeScan(code);
        }
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

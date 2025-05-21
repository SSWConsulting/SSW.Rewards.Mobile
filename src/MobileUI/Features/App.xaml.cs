using Mopups.Services;
using Plugin.Firebase.Crashlytics;

namespace SSW.Rewards.Mobile;

public partial class App : Application
{
    private static IServiceProvider _provider;
    private static IAuthenticationService _authService;
    private static IFirstRunService _firstRunService;

    public App(IServiceProvider serviceProvider, IAuthenticationService authService, IFirstRunService firstRunService)
    {
        _provider = serviceProvider;
        _authService = authService;
        _firstRunService = firstRunService;
        
        InitializeComponent();
        Current.UserAppTheme = AppTheme.Dark;
    }
    
    protected override Window CreateWindow(IActivationState? activationState)
    {
        var loginPage = ActivatorUtilities.CreateInstance<LoginPage>(_provider);
        return new Window(loginPage);
    }

    protected override async void OnStart()
    {
        //await UpdateAccessTokenAsync();
        await CheckApiCompatibilityAsync();
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
        try
        {
            base.OnAppLinkRequestReceived(uri);
            
            if (IsAutoLoginRequest(uri))
            {
                await HandleAutoLoginRequest(uri);
                return;
            }

            if (IsRedeemRequest(uri))
            {
                await HandleRedeemRequest(uri);
            }
        }
        catch (Exception ex)
        {
            CrossFirebaseCrashlytics.Current.Log($"Error processing app link: {ex.Message}");
        }
    }

    public static async Task InitialiseMainPage()
    {
        await _firstRunService.InitialiseAfterLogin();
    }

    private async Task CheckApiCompatibilityAsync()
    {
        try
        {
            ApiInfo info = new ApiInfo(Constants.ApiBaseUrl);

            bool compatible = await info.IsApiCompatibleAsync();

            if (!compatible)
            {
                await Shell.Current.DisplayAlert("Update Required", "Looks like you're using an older version of the app. You can continue, but some features may not function as expected.", "OK");
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
    
    private static bool IsAutoLoginRequest(Uri uri) =>
        $"{uri.Scheme}://{uri.Host}" == Constants.AutologinRedirectUrl;

    private static bool IsRedeemRequest(Uri uri) =>
        uri is { Scheme: ApiClientConstants.RewardsQRCodeProtocol, Host: "redeem" } or
            { Host: ApiClientConstants.RewardsWebDomain, AbsolutePath: "/redeem" };

    private static async Task HandleAutoLoginRequest(Uri uri)
    {
        var queryDictionary = System.Web.HttpUtility.ParseQueryString(uri.Query);
        var token = queryDictionary.Get("token");

        if (!string.IsNullOrEmpty(token))
        {
            await _authService.AutologinAsync(token);
        }
    }

    private static async Task HandleRedeemRequest(Uri uri)
    {
        var queryDictionary = System.Web.HttpUtility.ParseQueryString(uri.Query);
        var code = queryDictionary.Get(ApiClientConstants.RewardsQRCodeProtocolQueryName);

        if (string.IsNullOrEmpty(code))
        {
            return;
        }

        if (_authService.IsLoggedIn)
        {
            await ShowScanResultPopup(code);
        }
        else
        {
            _firstRunService.SetPendingScanCode(code);
        }
    }

    private static async Task ShowScanResultPopup(string code)
    {
        var vm = ActivatorUtilities.CreateInstance<ScanResultViewModel>(_provider);
        var popup = new PopupPages.ScanResult(vm, code);
        await MopupService.Instance.PushAsync(popup);
    }

}

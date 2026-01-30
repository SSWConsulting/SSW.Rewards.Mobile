using Microsoft.Extensions.Logging;
using Mopups.Services;
using SSW.Rewards.Mobile.Common;

namespace SSW.Rewards.Mobile;

public partial class App : Application
{
    private static IServiceProvider _serviceProvider;
    private static IAuthenticationService _authService;
    private static IFirstRunService _firstRunService;
    private static ILogger<App> _logger;

    public App(
        LoginPage page,
        IServiceProvider serviceProvider,
        IAuthenticationService authService,
        IFirstRunService firstRunService,
        ILogger<App> logger)
    {
        _serviceProvider = serviceProvider;
        _authService = authService;
        _firstRunService = firstRunService;
        _logger = logger;

        InitializeComponent();
        Current.UserAppTheme = AppTheme.Dark;

        // Log all unhandled exceptions
        MauiExceptions.UnhandledException += OnUnhandledException;
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var loginPage = ActivatorUtilities.CreateInstance<LoginPage>(_serviceProvider);
        return new Window(loginPage);
    }

    protected override async void OnStart()
    {
        try
        {
            await CheckApiCompatibilityAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during app startup");
        }
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
                await HandleAutoLoginRequestAsync(uri);
                return;
            }

            if (IsRedeemRequest(uri))
            {
                await HandleRedeemRequestAsync(uri);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing app link: {Uri}", uri);
        }
    }

    public static async Task InitialiseMainPageAsync()
    {
        await _firstRunService.InitialiseAfterLogin();
    }

    public static void NavigateToLoginPage()
    {
        _authService.NavigateToLoginPage();
    }

    private void OnUnhandledException(object sender, UnhandledExceptionEventArgs args)
    {
        if (args.ExceptionObject is Exception exception)
        {
            _logger.LogError(exception, "Unhandled exception occurred");
        }
    }

    private async Task CheckApiCompatibilityAsync()
    {
        try
        {
            var apiInfo = new ApiInfo(Constants.ApiBaseUrl);
            var isCompatible = await apiInfo.IsApiCompatibleAsync();

            if (!isCompatible)
            {
                await IPlatformApplication.Current.DisplayAlertAsync(
                        "Update Required",
                        "Looks like you're using an older version of the app. You can continue, but some features may not function as expected.",
                        "OK");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking API compatibility");
        }
    }

    private static bool IsAutoLoginRequest(Uri uri) =>
        $"{uri.Scheme}://{uri.Host}" == Constants.AutologinRedirectUrl;

    private static bool IsRedeemRequest(Uri uri) =>
        uri is { Scheme: ApiClientConstants.RewardsQRCodeProtocol, Host: "redeem" } or
        { Host: ApiClientConstants.RewardsWebDomain, AbsolutePath: "/redeem" };

    private async Task HandleAutoLoginRequestAsync(Uri uri)
    {
        var queryDictionary = System.Web.HttpUtility.ParseQueryString(uri.Query);
        var token = queryDictionary.Get("token");

        if (!string.IsNullOrEmpty(token))
        {
            await _authService.AutologinAsync(token);
        }
        else
        {
            _logger.LogWarning("Auto-login request received without token");
        }
    }

    private async Task HandleRedeemRequestAsync(Uri uri)
    {
        var queryDictionary = System.Web.HttpUtility.ParseQueryString(uri.Query);
        var code = queryDictionary.Get(ApiClientConstants.RewardsQRCodeProtocolQueryName);

        if (string.IsNullOrEmpty(code))
        {
            _logger.LogWarning("Redeem request received without code");
            return;
        }

        if (_authService.IsLoggedIn)
        {
            await ShowScanResultPopupAsync(code);
        }
        else
        {
            _firstRunService.SetPendingScanCode(code);
        }
    }

    private async Task ShowScanResultPopupAsync(string code)
    {
        var viewModel = ActivatorUtilities.CreateInstance<ScanResultViewModel>(_serviceProvider);
        var popup = new PopupPages.ScanResult(viewModel, code);
        await MopupService.Instance.PushAsync(popup);
    }
}
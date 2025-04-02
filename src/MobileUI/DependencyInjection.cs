using System.Reflection;
using SSW.Rewards.ApiClient;
using SSW.Rewards.ApiClient.Services;
using SSW.Rewards.Mobile.Controls;
using SSW.Rewards.Mobile.ViewModels.ProfileViewModels;
using SSW.Rewards.Mobile.Config;
using IBrowser = IdentityModel.OidcClient.Browser.IBrowser;
using IQuizService = SSW.Rewards.Mobile.Services.IQuizService;
using IRewardService = SSW.Rewards.Mobile.Services.IRewardService;
using IUserService = SSW.Rewards.Mobile.Services.IUserService;
using QuizService = SSW.Rewards.Mobile.Services.QuizService;
using RewardService = SSW.Rewards.Mobile.Services.RewardService;
using UserService = SSW.Rewards.Mobile.Services.UserService;

namespace SSW.Rewards.Mobile;

public static class DependencyInjection
{
    public static IServiceCollection AddDependencies(this IServiceCollection services)
    {
        var options = new ApiOptions { BaseUrl = Constants.ApiBaseUrl };

        // TODO: move this to a source generator
        // We definitely shouldn't be using reflection at startup in a mobile app!!
        // See: https://github.com/matt-goldman/Maui.Plugins.PageResolver/wiki/2-Using-the-dependency-registration-source-generator

        var excludedTypes = new []
        {
            typeof(OthersProfilePage),
            typeof(OthersProfileViewModel),
            typeof(ProfileViewModelBase),
            typeof(QuizDetailsPage),
            typeof(QuizDetailsViewModel),
            typeof(ScanPage),
            typeof(ScanViewModel)
        };

        var definedTypes = Assembly.GetExecutingAssembly().DefinedTypes
            .Where(e => e.IsSubclassOf(typeof(Page)) || e.IsSubclassOf(typeof(BaseViewModel)) && !excludedTypes.Contains(e));

        foreach (var type in definedTypes)
        {
            services.AddSingleton(type.AsType());
        }

        services.AddTransient<OthersProfilePage>();
        services.AddTransient<OthersProfileViewModel>();
        services.AddTransient<QuizDetailsPage>();
        services.AddTransient<QuizDetailsViewModel>();
        services.AddTransient<ScanPage>();
        services.AddTransient<ScanViewModel>();

        services.AddSingleton<ILeaderService, LeaderService>();
        services.AddSingleton<IUserService, UserService>();
        services.AddSingleton<IDevService, DevService>();
        services.AddSingleton<IScannerService, ScannerService>();
        services.AddSingleton<IRewardService, RewardService>();
        services.AddSingleton<IQuizService, QuizService>();
        services.AddSingleton<IBrowser, AuthBrowser>();
        services.AddTransient<AuthHandler>();
        services.AddSingleton(options);
        services.AddSingleton<ISnackbarService, SnackBarService>();
        services.AddSingleton<IPermissionsService, PermissionsService>();
        services.AddSingleton<IPushNotificationsService, PushNotificationsService>();
        services.AddSingleton<IRewardAdminService, RewardAdminService>();
        services.AddSingleton<IFirebaseAnalyticsService, FirebaseAnalyticsService>();
        services.AddSingleton<IFirstRunService, FirstRunService>();

        services.AddSingleton<FlyoutHeader>();
        services.AddSingleton<FlyoutHeaderViewModel>();
        services.AddSingleton<FlyoutFooter>();
        services.AddSingleton<FlyoutFooterViewModel>();
        services.AddSingleton<TopBarViewModel>();

        services.AddSingleton<IAuthenticationService, AuthenticationService>();

        services.AddApiClientServices<AuthHandler>(options.BaseUrl);

        // Configuration that in the future will be modifiable by the user or WebAPI
        services.AddSingleton(new ScannerConfig());

        return services;
    }
}
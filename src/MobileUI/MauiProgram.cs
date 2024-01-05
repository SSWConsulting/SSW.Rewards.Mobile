using CommunityToolkit.Maui;
using FFImageLoading.Maui;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Microsoft.Extensions.Logging;
using Mopups.Hosting;
using SkiaSharp.Views.Maui.Controls.Hosting;
using SSW.Rewards.Mobile.Controls;
using SSW.Rewards.Mobile.ViewModels.ProfileViewModels;
using SSW.Rewards.Shared;
using System.Reflection;
using ZXing.Net.Maui.Controls;
using IBrowser = IdentityModel.OidcClient.Browser.IBrowser;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace SSW.Rewards.Mobile;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder.UseMauiApp<App>().ConfigureFonts(fonts =>
        {
            fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            fonts.AddFont("FluentSystemIcons-Regular.ttf", "FluentIcons");
            fonts.AddFont("FA6Brands-Regular.otf", "FA6Brands");
            fonts.AddFont("Helvetica-Bold-Font.ttf", "HelveticaBold");
        })
        .UseMauiCommunityToolkit()
        .UseFFImageLoading()
        .ConfigureMopups()
        .UseSkiaSharp()
        .UsePageResolver()
        .UseBarcodeReader();

        AppCenter.Start($"android={Constants.AppCenterAndroidId};" +
                  $"ios={Constants.AppCenterIOSId};",
                  typeof(Analytics), typeof(Crashes));

        var options = new ApiOptions { BaseUrl = Constants.ApiBaseUrl };

        // TODO: move this to a source generator
        // We definitely shouldn't be using reflection at startup in a mobile app!!
        // See: https://github.com/matt-goldman/Maui.Plugins.PageResolver/wiki/2-Using-the-dependency-registration-source-generator

        var excludedTypes = new Type[]
        {
            typeof(OtherProfileViewModel),
            typeof(OthersProfilePage),
            typeof(ProfileViewModelBase),
        };

        var definedTypes = Assembly.GetExecutingAssembly().DefinedTypes
            .Where(e => e.IsSubclassOf(typeof(Page)) || e.IsSubclassOf(typeof(BaseViewModel)) && !excludedTypes.Contains(e));

        foreach (var type in definedTypes)
        {
            builder.Services.AddSingleton(type.AsType());
        }

        builder.Services.AddTransient<OtherProfileViewModel>();
        builder.Services.AddTransient<OthersProfilePage>();

        builder.Services.AddSingleton<ILeaderService, LeaderService>();
        builder.Services.AddSingleton<IUserService, UserService>();
        builder.Services.AddSingleton<IDevService, DevService>();
        builder.Services.AddSingleton<IScannerService, ScannerService>();
        builder.Services.AddSingleton<IRewardService, RewardService>();
        builder.Services.AddSingleton<IQuizService, QuizService>();
        builder.Services.AddSingleton<IBrowser, AuthBrowser>();
        builder.Services.AddSingleton<AuthHandler>();
        builder.Services.AddSingleton(options);
        builder.Services.AddSingleton<ISnackbarService, SnackBarService>();

        builder.Services.AddSingleton<FlyoutHeader>();
        builder.Services.AddSingleton<FlyoutHeaderViewModel>();

        builder.Services.AddApiClientServices<AuthHandler>(options.BaseUrl);

#if DEBUG
        builder.Logging.AddDebug();
#endif

        App.SetScope(builder.Services);

        return builder.Build();
    }
}
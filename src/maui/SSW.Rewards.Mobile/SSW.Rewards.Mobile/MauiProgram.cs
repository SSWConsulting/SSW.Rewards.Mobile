using Microsoft.Extensions.Logging;
using SSW.Rewards.Helpers;
using System.Reflection;
using IBrowser = IdentityModel.OidcClient.Browser.IBrowser;
using CommunityToolkit.Maui;
using Mopups.Hosting;

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
        })
        .UseMauiCommunityToolkit()
        .ConfigureMopups();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        var definedTypes = Assembly.GetExecutingAssembly().DefinedTypes
            .Where(e => e.IsSubclassOf(typeof(Page)) || e.IsSubclassOf(typeof(BaseViewModel)));

        foreach (var type in definedTypes)
        {
            builder.Services.AddSingleton(type.AsType());
        }

        builder.Services.AddSingleton<ILeaderService, LeaderService>();
        builder.Services.AddSingleton<IUserService, UserService>();
        builder.Services.AddSingleton<IDevService, DevService>();
        builder.Services.AddSingleton<IScannerService, ScannerService>();
        builder.Services.AddSingleton<IRewardService, RewardService>();
        builder.Services.AddSingleton<IQuizService, QuizService>();
        builder.Services.AddSingleton<IBrowser, AuthBrowser>();

        return builder.Build();
    }
}
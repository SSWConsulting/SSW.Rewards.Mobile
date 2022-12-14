using SSW.Rewards.Helpers;
//using IBrowser = IdentityModel.OidcClient.Browser.IBrowser;

namespace SSW.Rewards;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>();
        
        var currentAssembly = Assembly.GetExecutingAssembly();

        foreach (var type in currentAssembly.DefinedTypes
            .Where(e =>
            e.IsSubclassOf(typeof(Page)) ||
            e.IsSubclassOf(typeof(BaseViewModel))))
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

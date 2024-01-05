using Microsoft.Extensions.DependencyInjection;
using SSW.Rewards.ApiClient.Services;

namespace SSW.Rewards.ApiClient;

public static class ConfigureServices
{
    /// <summary>
    /// Register the API client services
    /// </summary>
    /// <typeparam name="THandler">The type of your authentication message handler</typeparam>
    /// <param name="baseAddress">The API base address</param>
    /// <returns></returns>
    public static IServiceCollection AddApiClientServices<THandler>(this IServiceCollection services, string baseAddress, bool includeAdminServices = false) where THandler : DelegatingHandler
    {
        services.AddHttpClient(Constants.AuthenticatedClient, client =>
        {
            client.BaseAddress = new Uri(baseAddress);
        })
        .AddHttpMessageHandler<THandler>();

        if (includeAdminServices)
        {
            services.AddSingleton<IAchievementAdminService, AchievementAdminService>();
            services.AddSingleton<IPrizeDrawService, PrizeDrawService>();
            services.AddSingleton<IQuizAdminService, QuizAdminService>();
            services.AddSingleton<IRewardAdminService, RewardAdminService>();
        }

        services.AddSingleton<IAchievementService, AchievementService>();
        services.AddSingleton<ILeaderboardService, LeaderboardService>();
        services.AddSingleton<IQuizService, QuizService>();
        services.AddSingleton<IRewardService, RewardService>();

        return services;
    }
}

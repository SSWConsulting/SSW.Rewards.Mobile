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
            Console.WriteLine($"Configuring API client with base address: {baseAddress}");
            client.BaseAddress = new Uri(baseAddress);
        })
        .AddHttpMessageHandler<THandler>();

        if (includeAdminServices)
        {
            services.AddSingleton<IAchievementAdminService, AchievementAdminService>();
            services.AddSingleton<IPrizeDrawService, PrizeDrawService>();
            services.AddSingleton<IQuizAdminService, QuizAdminService>();
            services.AddSingleton<IRewardAdminService, RewardAdminService>();
            services.AddSingleton<ISkillsAdminService, SkillsAdminService>();
            services.AddSingleton<IStaffAdminService, StaffAdminService>();
            services.AddSingleton<IUserAdminService, UserAdminService>();
        }

        services.AddSingleton<IAchievementService, AchievementService>();
        services.AddSingleton<ILeaderboardService, LeaderboardService>();
        services.AddSingleton<IQuizService, QuizService>();
        services.AddSingleton<IRewardService, RewardService>();
        services.AddSingleton<ISkillsService, SkillsService>();
        services.AddSingleton<IStaffService, StaffService>();
        services.AddSingleton<IUserService, UserService>();
        services.AddSingleton<IAddressService, AddressService>();
        services.AddSingleton<IActivityFeedService, ActivityFeedService>();

        return services;
    }
}

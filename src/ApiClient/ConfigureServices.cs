using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using SSW.Rewards.ApiClient.Resilience;
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

            // The resilience pipeline owns timeouts (see ConfigureResilience). HttpClient's own 100s timeout
            // would race its total timeout and surface as a TaskCanceledException callers don't catch.
            client.Timeout = Timeout.InfiniteTimeSpan;

            var cleintNameHeaderValue = includeAdminServices ? Constants.RewardsAppClientName_AdminUI : Constants.RewardsAppClientName_MobileApp;
            client.DefaultRequestHeaders.Add(Constants.RewardsAppClientNameHeaderKey, cleintNameHeaderValue);
        })
        // Outermost: replays a stored GET once everything inside it (retries included) has failed.
        // Only active when the app registers an IApiResponseCache (the mobile app does, AdminUI doesn't).
        .AddHttpMessageHandler(sp => sp.GetService<IApiResponseCache>() is { } responseCache
            ? new OfflineFallbackHandler(responseCache)
            : new PassThroughHandler())
        // Next, so the pipeline's timeout/circuit failures reach callers as HttpRequestException.
        .AddHttpMessageHandler(() => new ResilienceRejectionHandler())
        .AddHttpMessageHandler<THandler>()
        .AddStandardResilienceHandler()
        .Configure(ConfigureResilience);

        if (includeAdminServices)
        {
            services.AddSingleton<IAchievementAdminService, AchievementAdminService>();
            services.AddSingleton<IPrizeDrawService, PrizeDrawService>();
            services.AddSingleton<IQuizAdminService, QuizAdminService>();
            services.AddSingleton<IRewardAdminService, RewardAdminService>();
            services.AddSingleton<ISkillsAdminService, SkillsAdminService>();
            services.AddSingleton<IStaffAdminService, StaffAdminService>();
            services.AddSingleton<IUserAdminService, UserAdminService>();
            services.AddSingleton<IRoleService, RoleService>();
        }

        services.AddSingleton<IAchievementService, AchievementService>();
        services.AddSingleton<ILeaderboardService, LeaderboardService>();
        services.AddSingleton<INotificationsService, NotificationsService>();
        services.AddSingleton<IQuizService, QuizService>();
        services.AddSingleton<IRewardService, RewardService>();
        services.AddSingleton<ISkillsService, SkillsService>();
        services.AddSingleton<IStaffService, StaffService>();
        services.AddSingleton<IUserService, UserService>();
        services.AddSingleton<IAddressService, AddressService>();
        services.AddSingleton<IActivityFeedService, ActivityFeedService>();
        services.AddSingleton<IPostsService, PostsService>();

        return services;
    }

    private sealed class PassThroughHandler : DelegatingHandler;

    /// <summary>
    /// Standard pipeline (retry + circuit breaker + timeouts), shared by the mobile app and AdminUI.
    /// </summary>
    private static void ConfigureResilience(HttpStandardResilienceOptions options, IServiceProvider serviceProvider)
    {
        // Same ceiling as HttpClient's default timeout, which this pipeline replaces. The standard
        // 10s attempt / 30s total would fail image uploads on a slow link and long admin operations.
        options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(100);
        options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(60);

        // Must be at least twice the attempt timeout.
        options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(120);

        // Never replay non-idempotent calls (quiz submit, redeem, scan, admin edits):
        // POST/PUT/PATCH/DELETE/CONNECT are not retried.
        options.Retry.DisableForUnsafeHttpMethods();

        // Offline, every retry fails the same way and only delays the error by ~14s of backoff.
        var networkStatus = serviceProvider.GetService<IApiNetworkStatus>();
        if (networkStatus is not null)
        {
            var shouldHandle = options.Retry.ShouldHandle;
            options.Retry.ShouldHandle = args => networkStatus.IsOnline ? shouldHandle(args) : ValueTask.FromResult(false);
        }
    }
}

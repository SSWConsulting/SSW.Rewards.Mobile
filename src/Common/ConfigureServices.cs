using Microsoft.Extensions.DependencyInjection;

namespace SSW.Rewards.Shared;

public static class ConfigureServices
{
    /// <summary>
    /// Register the API client services
    /// </summary>
    /// <typeparam name="THandler">The type of your authentication message handler</typeparam>
    /// <param name="baseAddress">The API base address</param>
    /// <returns></returns>
    public static IServiceCollection AddApiClientServices<THandler>(this IServiceCollection services, string baseAddress) where THandler : DelegatingHandler
    {
        services.AddHttpClient(Constants.AuthenticatedClient, client =>
        {
            client.BaseAddress = new Uri(baseAddress);
        })
        .AddHttpMessageHandler<THandler>();

        return services;
    }
}

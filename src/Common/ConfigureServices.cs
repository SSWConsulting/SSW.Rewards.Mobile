using Microsoft.Extensions.DependencyInjection;

namespace Shared;

public static class ConfigureServices
{
    public static IServiceCollection AddApiClientServices(this IServiceCollection services, string baseAddress)
    {
        services.AddHttpClient(Constants.AuthenticatedClient, client =>
        {
            client.BaseAddress = new Uri(baseAddress);
        });

        return services;
    }
}

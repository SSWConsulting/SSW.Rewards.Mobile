using FluentEmail.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Graph;

namespace FluentEmail.Graph;

public static class FluentEmailServicesBuilderExtensions
{
    public static FluentEmailServicesBuilder AddGraphSender(
        this FluentEmailServicesBuilder builder,
        GraphSenderOptions options)
    {
        builder.Services.TryAdd(ServiceDescriptor.Scoped<ISender>(_ => new GraphSender(options)));
        return builder;
    }

    public static FluentEmailServicesBuilder AddGraphSender(
        this FluentEmailServicesBuilder builder,
        string graphEmailClientId,
        string graphEmailTenantId,
        string graphEmailSecret)
    {
        var options = new GraphSenderOptions
        {
            ClientId = graphEmailClientId,
            TenantId = graphEmailTenantId,
            Secret = graphEmailSecret,
        };
        return builder.AddGraphSender(options);
    }

    public static FluentEmailServicesBuilder AddGraphSender(
        this FluentEmailServicesBuilder builder,
        GraphServiceClient graphClient)
    {
        builder.Services.TryAdd(ServiceDescriptor.Scoped<ISender>(_ => new GraphSender(graphClient)));
        return builder;
    }
}

using FluentValidation.AspNetCore;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.OpenApi;
using SSW.Rewards.Application.Common.Interfaces;
using SSW.Rewards.Infrastructure.Persistence;
using SSW.Rewards.WebAPI.Authorisation;
using SSW.Rewards.WebAPI.Filters;
using SSW.Rewards.WebAPI.Services;
using SSW.Rewards.WebAPI.Telemetry;

namespace Microsoft.Extensions.DependencyInjection;

public static class ConfigureServices
{
    public static IServiceCollection AddWebAPIServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDatabaseDeveloperPageExceptionFilter();

        services.AddSingleton<ICurrentUserService, CurrentUserService>();

        services.AddHttpContextAccessor();

        var healthChecksBuilder = services.AddHealthChecks()
            .AddDbContextCheck<ApplicationDbContext>();

        // Add Azure Blob Storage health check if connection string is configured
        var blobConnectionString = configuration["CloudBlobProviderOptions:ContentStorageConnectionString"];
        if (!string.IsNullOrWhiteSpace(blobConnectionString))
        {
            try
            {
                // Check if it's a connection string (contains '=') or a service URI
                if (blobConnectionString.Contains('='))
                {
                    // It's a connection string - add blob storage health check
                    healthChecksBuilder.AddAzureBlobStorage(
                        blobConnectionString,
                        name: "azure-blob-storage",
                        failureStatus: HealthStatus.Degraded,
                        tags: new[] { "storage", "blob" });
                }
                else if (Uri.TryCreate(blobConnectionString, UriKind.Absolute, out _))
                {
                    // It's a service URI - would need DefaultAzureCredential, skip health check for now
                    // as health checks with managed identity are more complex
                    Console.WriteLine("[HealthCheck] Blob storage configured with service URI - health check skipped (requires managed identity)");
                }
            }
            catch (Exception ex)
            {
                // Log but don't fail startup if health check can't be added
                Console.WriteLine($"[HealthCheck] Warning: Could not add blob storage health check: {ex.Message}");
            }
        }
        else
        {
            Console.WriteLine("[HealthCheck] Blob storage not configured - health check skipped");
        }

        services.AddControllersWithViews(options =>
            options.Filters.Add<ApiExceptionFilterAttribute>())
                .AddFluentValidation(x => x.AutomaticValidationEnabled = false);

        // Customise default API behaviour
        services.Configure<ApiBehaviorOptions>(options =>
            options.SuppressModelStateInvalidFilter = true);

        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "SSW.Rewards API",
                Version = "v1"
            });

            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                In = ParameterLocation.Header,
                Description = "Please enter JWT with Bearer into field",
                Name = "Authorization",
                Type = SecuritySchemeType.ApiKey
            });

            options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference("Bearer", document)] = []
            });
        });

        string _allowSpecificOrigins = "_AllowSpecificOrigins";
        string? AllowedOrigin = configuration[nameof(AllowedOrigin)];

        if (string.IsNullOrWhiteSpace(AllowedOrigin))
        {
            throw new Exception("AllowedOrigin is not configured");
        }

        services.AddCors(options =>
        {
            options.AddPolicy(_allowSpecificOrigins,
                builder =>
                {
                    if (AllowedOrigin == "*")
                    {
                        builder.WithOrigins(AllowedOrigin)
                            .AllowAnyHeader()
                            .AllowAnyMethod()
                            .AllowCredentials();
                    }
                });
        });

        services.AddApplicationInsightsTelemetry();
        services.AddSingleton<ITelemetryInitializer, WebApiTelemetryInitializer>();
        services.AddApplicationInsightsTelemetryProcessor<TelemetryProcessor>();
        services.Configure<TelemetryConfig>(configuration.GetSection("Telemetry"));

        services.AddDistributedMemoryCache();

        //TODO: Remove magic string
        services.AddAuthorization(options =>
            options.AddPolicy(Policies.MobileApp, policy => policy.RequireClaim("client_id", "ssw-rewards-mobile-app")));

        // Initialize UrlBlockList from configuration
        string[]? ignore404PathContains = configuration.GetSection("Ignore404PathContains").Get<string[]>();
        UrlBlockList.Init(ignore404PathContains);

        return services;
    }
}

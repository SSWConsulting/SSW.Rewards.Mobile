using FluentValidation.AspNetCore;
using Microsoft.ApplicationInsights.DependencyCollector;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
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

        services.AddHealthChecks()
            .AddDbContextCheck<ApplicationDbContext>();

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

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    new string[] {}
                }
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
                    builder.WithOrigins(AllowedOrigin)
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
                });
        });

        services.AddApplicationInsightsTelemetry();
        services.AddSingleton<ITelemetryInitializer, WebApiTelemetryInitializer>();
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

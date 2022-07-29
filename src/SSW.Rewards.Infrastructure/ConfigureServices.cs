using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SSW.Rewards.Application.Common.Interfaces;
using SSW.Rewards.Application.Common.Models;
using SSW.Rewards.Infrastructure;
using SSW.Rewards.Infrastructure.Persistence;
using SSW.Rewards.Infrastructure.Persistence.Interceptors;
using SSW.Rewards.Infrastructure.Services;

namespace Microsoft.Extensions.DependencyInjection;

public static class ConfigureServices
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<AuditableEntitySaveChangesInterceptor>();

        services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"),
                    builder => builder.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());

        services.AddOptions<NotificationHubOptions>()
                .Configure(configuration.GetSection("NotificationHub").Bind)
                .ValidateDataAnnotations();

        services.AddOptions<CloudBlobProviderOptions>()
                .Configure(configuration.GetSection(nameof(CloudBlobProviderOptions)).Bind)
                .ValidateDataAnnotations();

        services.AddSingleton<ICloudBlobClientProvider, CloudBlobClientProvider>();
        services.AddSingleton<IStorageProvider, AzureStorageProvider>();
        services.AddSingleton<INotificationService, NotificationsService>();

        services.AddScoped<IProfileStorageProvider, ProfileStorageProvider>();
        services.AddScoped<IProfilePicStorageProvider, ProfilePicStorageProvider>();
        services.AddScoped<IRewardPicStorageProvider, RewardPicStorageProvider>();

        services.AddScoped<IEmailService, EmailService>();

        services.AddScoped<IRewardSender, RewardSender>();

        SMTPSettings smtpSettings = new SMTPSettings();

        configuration.Bind("SMTPSettings", smtpSettings);

        string SendGridAPIKey = configuration.GetValue<string>(nameof(SendGridAPIKey));

        services.AddFluentEmail(smtpSettings.DefaultSender, smtpSettings.DefaultSenderName)
                    .AddRazorRenderer()
                    .AddSendGridSender(SendGridAPIKey);

        string signingAuthority = configuration.GetValue<string>(nameof(signingAuthority));

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.Authority = signingAuthority;
            options.Audience = "rewards";
            options.TokenValidationParameters.ValidTypes = new[] { "at+jwt" };
        });

        return services;
    }
}

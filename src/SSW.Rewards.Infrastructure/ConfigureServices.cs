using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SSW.Rewards.Application.Common.Interfaces;
using SSW.Rewards.Infrastructure;
using SSW.Rewards.Infrastructure.Persistence;
using SSW.Rewards.Infrastructure.Persistence.Interceptors;

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

        services.AddSingleton<ICloudBlobClientProvider, CloudBlobClientProvider>();
        services.AddSingleton<IStorageProvider, AzureStorageProvider>();

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

        return services;
    }
}

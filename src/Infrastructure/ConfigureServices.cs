using FluentEmail.Graph;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using SSW.Rewards.Application.Common.Interfaces;
using SSW.Rewards.Application.Common.Models;
using SSW.Rewards.Infrastructure;
using SSW.Rewards.Infrastructure.Options;
using SSW.Rewards.Infrastructure.Persistence;
using SSW.Rewards.Infrastructure.Persistence.Interceptors;
using SSW.Rewards.Infrastructure.Services;

namespace Microsoft.Extensions.DependencyInjection;

public static class ConfigureServices
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpClient();
        
        services.AddScoped<AuditableEntitySaveChangesInterceptor>();
        services.AddScoped<AchievementIntegrationIdInterceptor>();
        services.AddOptions<GPTServiceOptions>()
            .Configure(configuration.GetSection(nameof(GPTServiceOptions)).Bind);
        services.AddScoped<IQuizGPTService, QuizGPTService>();

        services.AddDbContext<ApplicationDbContext>(options =>
        {
#if DEBUG
            options.EnableSensitiveDataLogging();
#endif
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"),
                builder => builder.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName));
        });

        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());

        services.AddOptions<NotificationHubOptions>()
                .Configure(configuration.GetSection("NotificationHub").Bind)
                .ValidateDataAnnotations();

        services.AddAzureClients(clientBuilder =>
        {
            clientBuilder.AddBlobServiceClient(configuration["CloudBlobProviderOptions:ContentStorageConnectionString"], preferMsi: true);
        });

        services.AddSingleton<IStorageProvider, AzureStorageProvider>();
        services.AddSingleton<INotificationService, NotificationsService>();

        services.AddScoped<IProfileStorageProvider, ProfileStorageProvider>();
        services.AddScoped<IProfilePicStorageProvider, ProfilePicStorageProvider>();
        services.AddScoped<IRewardPicStorageProvider, RewardPicStorageProvider>();

        services.AddScoped<IEmailService, EmailService>();

        services.AddScoped<IRewardSender, RewardSender>();

        SMTPSettings smtpSettings = new SMTPSettings();

        configuration.Bind("SMTPSettings", smtpSettings);

        GraphSenderOptions GraphSenderOptions = new GraphSenderOptions();

        configuration.Bind(nameof(GraphSenderOptions), GraphSenderOptions);

        services.AddFluentEmail("verify@ssw.com.au")
                    .AddRazorRenderer()
                    .AddGraphSender(GraphSenderOptions);

        string signingAuthority = configuration.GetValue<string>(nameof(signingAuthority));

        services.AddTransient<IDateTime, DateTimeService>();

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

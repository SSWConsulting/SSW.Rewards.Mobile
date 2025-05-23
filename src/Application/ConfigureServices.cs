using System.Reflection;
using Microsoft.Extensions.Configuration;
using SSW.Rewards.Application.Common.Behaviours;
using SSW.Rewards.Application.Services;
using SSW.Rewards.Application.Users.Commands.DeleteMyProfile;

namespace Microsoft.Extensions.DependencyInjection;

public static class ConfigureServices
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAutoMapper(Assembly.GetExecutingAssembly());
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(Assembly.GetExecutingAssembly()));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(UnhandledExceptionBehaviour<,>));
        //services.AddTransient(typeof(IPipelineBehavior<,>), typeof(AuthorizationBehaviour<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PerformanceBehaviour<,>));

        services.Configure<UserServiceOptions>(configuration.GetSection(nameof(UserServiceOptions)));
        services.Configure<DeleteProfileOptions>(configuration.GetSection(nameof(DeleteProfileOptions)));
        services.Configure<FirebaseNotificationServiceOptions>(configuration.GetSection(nameof(FirebaseNotificationServiceOptions)));

        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IRolesService, UserService>();
        services.AddScoped<IFirebaseInitializerService, FirebaseInitializerService>();
        services.AddScoped<IFirebaseNotificationService, FirebaseNotificationService>();

        return services;
    }
}

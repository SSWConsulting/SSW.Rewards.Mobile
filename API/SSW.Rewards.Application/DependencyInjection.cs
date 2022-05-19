using System.Reflection;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using SSW.Rewards.Application.Common.Behaviours;
using SSW.Rewards.Application.Common.Interfaces;
using SSW.Rewards.Application.Services;
using SSW.Rewards.Application.Users.Common.Interfaces;

namespace SSW.Rewards.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddAutoMapper(Assembly.GetExecutingAssembly());
            services.AddMediatR(Assembly.GetExecutingAssembly());
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(RequestPerformanceBehaviour<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(RequestValidationBehavior<,>));
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IRolesService, UserService>();

            return services;
        }
    }
}

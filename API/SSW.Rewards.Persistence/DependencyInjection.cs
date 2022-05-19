using Microsoft.Extensions.DependencyInjection;
using SSW.Rewards.Application.Common.Interfaces;

namespace SSW.Rewards.Persistence
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddPersistence(this IServiceCollection services)
        {
            services.AddScoped<ISSWRewardsDbContext, SSWRewardsDbContext>();
            services.AddDbContext<SSWRewardsDbContext>();
            services.AddTransient<DBInitialiser>();

            return services;
        }
    }
}

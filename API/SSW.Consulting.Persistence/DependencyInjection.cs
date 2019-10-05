using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SSW.Consulting.Application.Common.Interfaces;

namespace SSW.Consulting.Persistence
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<ISSWConsultingDbContext, SSWConsultingDbContext>();
            services.AddDbContext<SSWConsultingDbContext>();

            return services;
        }
    }
}

using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.AzureADB2C.UI;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SSW.Consulting.Application.Interfaces;
using SSW.Consulting.Application.Leaderboard.Queries.GetLeaderboardList;
using SSW.Consulting.Infrastructure;
using SSW.Consulting.Persistence;
using SSW.Consulting.WebAPI.Settings;
using System;
using System.Reflection;

namespace SSW.Consulting
{
	public class Startup
	{
		public Startup(IWebHostEnvironment environment, IConfiguration configuration)
		{
			Environment = environment;
			Configuration = configuration;
		}

		public IWebHostEnvironment Environment { get; }
		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddAuthentication(AzureADB2CDefaults.BearerAuthenticationScheme)
				.AddAzureADB2CBearer(options => Configuration.Bind("AzureAdB2C", options));

			services.AddControllers();

			// Add MediatR & AutoMapper
			services
                .AddMediatR(typeof(GetLeaderboardListQuery).GetTypeInfo().Assembly)
                .AddAutoMapper(typeof(GetLeaderboardListQuery).GetTypeInfo().Assembly);

            // Configure all the stuffs
            ConfigureSecretsProviders(services);
			ConfigureSettings(services);
			ConfigureSecrets(services);
			ConfigureStorageProviders(services);

			// Add SSW Consulting DbContext
			services.AddScoped<ISSWConsultingDbContext, SSWConsultingDbContext>();
		}

		protected virtual void ConfigureSecrets(IServiceCollection services)
		{
			// Add Secrets
			// TODO: Perhaps add some registration via convention for anything that implements a nested ISecrets interface
			services.AddSingleton<SSWConsultingDbContext.ISecrets, Secrets>();
			services.AddSingleton<CloudBlobClientProvider.ISecrets, Secrets>();
		}

		protected virtual void ConfigureSettings(IServiceCollection services)
		{
			// Add Settings
			// TODO: Perhaps add some registration via convention for anything that implements a nested ISettings interface
			services.AddSingleton<KeyVaultSecretsProvider.ISettings, AppSettings>();
		}

		protected virtual void ConfigureSecretsProviders(IServiceCollection services)
		{
			// Add Secrets Provider
#if DEBUG
			bool useLocalSecrets = Convert.ToBoolean(Configuration["UseLocalSecrets"]);
			if (useLocalSecrets)
			{
				services.AddSingleton<ISecretsProvider, LocalSecretsProvider>();
			}
			else
#endif
			{
				services.AddSingleton<ISecretsProvider, KeyVaultSecretsProvider>();
				services.AddSingleton<IKeyVaultClientProvider, KeyVaultClientProvider>();
			}
		}


		protected virtual void ConfigureStorageProviders(IServiceCollection services)
		{
			services.AddSingleton<ICloudBlobClientProvider, CloudBlobClientProvider>();
			services.AddSingleton<IStorageProvider, AzureStorageProvider>();
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}

			app.UseHttpsRedirection();

			app.UseRouting();

			app.UseAuthentication();
			app.UseAuthorization();

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllers();
			});
		}
	}
}

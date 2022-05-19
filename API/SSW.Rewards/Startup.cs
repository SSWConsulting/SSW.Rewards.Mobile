using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using SSW.Rewards.Application;
using SSW.Rewards.Application.Common.Interfaces;
using SSW.Rewards.Application.Common.Models;
using SSW.Rewards.Infrastructure;
using SSW.Rewards.Persistence;
using SSW.Rewards.WebAPI.Services;
using SSW.Rewards.WebAPI.Settings;

namespace SSW.Rewards
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

        readonly string _allowSpecificOrigins = "_AllowSpecificOrigins";

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
		{
            // Configure all the stuffs
            ConfigureSettings(services);
			ConfigureSecrets(services);

			string SigningAuthority = Configuration.GetValue<string>(nameof(SigningAuthority));

			services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = SigningAuthority;

                options.Audience = "rewards";

                options.TokenValidationParameters.ValidTypes = new[] { "at+jwt" };
            });

            services.AddInfrastructure(Configuration);
            services.AddPersistence();
            services.AddApplication();

            services.AddScoped<ICurrentUserService, CurrentUserService>();
            services.AddHttpContextAccessor();
            services.AddApplicationInsightsTelemetry();
            services.AddDistributedMemoryCache();

			services.AddOpenApiDocument(d =>
			{
				d.DocumentName = "SSW.Rewards API";
				d.Version = "1.0";
				d.Description = "API Specification for the SSW Rewards mobile app.";
				d.Title = "SSW.Rewards API";
			});
			
			services
                .AddControllers()
                .AddNewtonsoftJson();

            services.AddCors(options => 
            {
                options.AddPolicy(_allowSpecificOrigins,
                    builder =>
                    {
                        builder.WithOrigins(Configuration["AllowedOrigin"])
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                    });
            });

			services.AddSingleton<INotificationService, NotificationHubService>();

			services.AddOptions<NotificationHubOptions>()
				.Configure(Configuration.GetSection("NotificationHub").Bind)
				.ValidateDataAnnotations();
		}
		
		protected virtual void ConfigureSecrets(IServiceCollection services)
		{
			// Add Secrets
			// TODO: Perhaps add some registration via convention for anything that implements a nested ISecrets interface
			services.AddSingleton<SSWRewardsDbContext.ISecrets, Secrets>();
			services.AddSingleton<CloudBlobClientProvider.ISecrets, Secrets>();
		}

		protected virtual void ConfigureSettings(IServiceCollection services)
		{
			// Add Settings
			// TODO: Perhaps add some registration via convention for anything that implements a nested ISettings interface
			services.AddSingleton<KeyVaultSecretsProvider.ISettings, AppSettings>();
            services.AddSingleton<IWWWRedirectSettings, AppSettings>();
        }

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}

			// This will make the HTTP requests log as rich logs instead of plain text.
			app.UseSerilogRequestLogging();

			app.UseHttpsRedirection();

			app.UseRouting();

            app.UseOpenApi();
            app.UseSwaggerUi3();

            app.UseCors(_allowSpecificOrigins);

            app.UseAuthentication();
			app.UseAuthorization();
			
			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllers();
			});
		}
	}
}

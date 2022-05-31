using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using SSW.Rewards.Persistence;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace SSW.Rewards
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
	        try
	        {
		        IHost host = CreateHostBuilder(args).Build();

				bool seedUserRoles = args.Contains("/SeedUserRoles");

				bool mapAchievementTypes = args.Contains("/MapAchievementTypes");

				bool mapStaffAchievements = args.Contains("/MapStaffAchievements");

		        if (!await ApplyDbMigrations(host, seedUserRoles, mapAchievementTypes, mapStaffAchievements))
		        {
			        return;
		        }

				host.Run();
			}
	        catch (Exception ex)
	        {
		        Log.Fatal(ex, "Host terminated unexpectedly");
			}
	        finally
	        {
		        Log.CloseAndFlush();
	        }
        }

        private static async Task<bool> ApplyDbMigrations(IHost host, bool seedUserRoles, bool mapAchievmentTypes, bool mapStaffAchievements)
        {
	        using (IServiceScope scope = host.Services.CreateScope())
	        {
		        IServiceProvider services = scope.ServiceProvider;

		        try
		        {
					var initialiser = services.GetRequiredService<DBInitialiser>();
					initialiser.Run();

					if (seedUserRoles)
                    {
						await initialiser.EnsureStaffAndAdminRolesSeeded();
                    }

					if (mapStaffAchievements)
                    {
						await initialiser.MapStaffToAchievements();
                    }

                    if (mapAchievmentTypes)
                    {
                        await initialiser.MapAchievementTypes();
                    }

                    return true;
				}
		        catch (Exception ex)
		        {
			        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
			        logger.LogCritical(ex, "An error occurred while migrating or initializing the database.");
			        return false;
				}
	        }
		}

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
	                webBuilder
		                .UseStartup<Startup>()
						.UseSerilog((hostingContext, loggerConfiguration) => {
			                loggerConfiguration
				                .ReadFrom.Configuration(hostingContext.Configuration)
				                .Enrich.FromLogContext()
				                .Enrich.WithProperty("ApplicationName", typeof(Program).Assembly.GetName().Name)
				                .Enrich.WithProperty("Environment", hostingContext.HostingEnvironment)
				                // TODO: Wait for a solution to have full AppInsights support:
				                // https://github.com/serilog/serilog-sinks-applicationinsights/issues/121
				                .WriteTo.Async(x => x.ApplicationInsights(TelemetryConfiguration.CreateDefault(), TelemetryConverter.Traces));


#if DEBUG
							// Used to filter out potentially bad data due debugging.
							// Very useful when doing Seq dashboards and want to remove logs under debugging session.
							loggerConfiguration.Enrich.WithProperty("DebuggerAttached", Debugger.IsAttached);
#endif
						});
                });
    }
}

using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using SSW.Rewards.Persistence;
using System;
using System.Diagnostics;
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

		        await ApplyDbMigrations(host);

				await host.RunAsync();
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

        private static async Task ApplyDbMigrations(IHost host)
        {
	        using IServiceScope scope = host.Services.CreateScope();
	        IServiceProvider services = scope.ServiceProvider;

	        try
	        {
		        var initializer = services.GetRequiredService<DbInitializer>();
		        await initializer.Run();
	        }
	        catch (Exception)
	        {
		        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
		        logger.LogCritical("An error occurred while migrating or initializing the database");
		        throw;
	        }
        }

        // ReSharper disable once MemberCanBePrivate.Global
        // needs to be public so nswag has an entrypoint
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

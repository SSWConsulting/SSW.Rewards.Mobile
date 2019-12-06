using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using SSW.Rewards.Application.System.Commands.SeedData;
using SSW.Rewards.Persistence;

namespace SSW.Rewards
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
	        try
	        {
		        IHost host = CreateHostBuilder(args).Build();

		        if (!await ApplyDbMigrations(host))
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

        private static async Task<bool> ApplyDbMigrations(IHost host)
        {
	        using (IServiceScope scope = host.Services.CreateScope())
	        {
		        IServiceProvider services = scope.ServiceProvider;

		        try
		        {
			        var dbContext = services.GetRequiredService<SSWRewardsDbContext>();
			        dbContext.Database.Migrate();

			        var mediator = services.GetRequiredService<IMediator>();
			        await mediator.Send(new SeedSampleDataCommand(), CancellationToken.None);

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
		                .UseSerilog();
                });
    }
}

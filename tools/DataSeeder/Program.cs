using Azure.Storage.Blobs;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SSW.Rewards.Application.Common.Interfaces;
using SSW.Rewards.Application.System.Commands.Common;
using SSW.Rewards.DataSeeder;
using SSW.Rewards.Infrastructure.Persistence;
using SSW.Rewards.Infrastructure.Persistence.Interceptors;

// SSW.Rewards.DataSeeder — re-runnable demo data seeding + DB reset for local dev.
//
//   dotnet run --project tools/DataSeeder -- seed  --dev-email you@example.com [--dev-name "Your Name"]
//                                                  [--years 3] [--connection-string CS] [--blob-connection-string CS]
//   dotnet run --project tools/DataSeeder -- reset [--yes] [--no-seed] [seed options]
//
// Normally invoked via `rewards-dev db seed|reset` or the Aspire dashboard commands.
// Connection strings resolve from args → env vars → the running docker containers.

const string RewardsDb = "ssw.rewards";
const string HangfireDb = "ssw.rewards.hangfire";

var verb = args.Length > 0 ? args[0].ToLowerInvariant() : "help";
if (verb is "help" or "-h" or "--help") return Usage();
if (verb is not ("seed" or "reset")) return Fail($"unknown command '{verb}'. Use: seed | reset");

string? Arg(string name)
{
    var i = Array.FindIndex(args, a => a.Equals(name, StringComparison.OrdinalIgnoreCase));
    return i >= 0 && i + 1 < args.Length ? args[i + 1] : null;
}
bool Flag(string name) => args.Any(a => a.Equals(name, StringComparison.OrdinalIgnoreCase));

var devEmail = Arg("--dev-email");
var noSeed = Flag("--no-seed");

if ((verb == "seed" || (verb == "reset" && !noSeed)) && string.IsNullOrWhiteSpace(devEmail))
    return Fail("--dev-email is required (the seeder pre-creates YOUR user so the app binds to your login). " +
                "Use --no-seed with reset to skip seeding.");

var sqlConn = ConnectionResolver.ResolveSql(Arg("--connection-string"), RewardsDb);
if (sqlConn is null)
    return Fail("could not resolve a SQL connection string. Pass --connection-string, set " +
                "ConnectionStrings__DefaultConnection, or start the stack first (`aspire run`).");

if (verb == "reset")
{
    var target = new SqlConnectionStringBuilder(sqlConn);
    if (!Flag("--yes"))
    {
        Console.WriteLine($"About to DROP '{RewardsDb}' and '{HangfireDb}' on {target.DataSource} and re-create from migrations.");
        Console.Write("Type 'reset' to confirm: ");
        if (Console.ReadLine()?.Trim().ToLowerInvariant() != "reset")
            return Fail("aborted.");
    }
    await DropDatabases(sqlConn);
    Console.WriteLine("✓ databases dropped");
}

await using (var services = BuildServices(sqlConn, Arg("--blob-connection-string")))
{
    var db = services.GetRequiredService<ApplicationDbContext>();

    Console.WriteLine("Applying EF migrations…");
    await db.Database.MigrateAsync();
    Console.WriteLine("✓ schema up to date");

    if (verb == "seed" || !noSeed)
    {
        var options = new DemoSeedOptions
        {
            DevEmail = devEmail!,
            DevName = Arg("--dev-name"),
            Years = int.TryParse(Arg("--years"), out var y) ? y : 3,
        };
        Console.WriteLine($"Seeding demo data ({options.Years}y of history, dev = {options.DevEmail})…");
        var seeder = new DemoDataSeeder(db, services.GetService<IDemoAssetProvider>(), Console.WriteLine);
        var summary = await seeder.SeedAsync(options, CancellationToken.None);
        Console.WriteLine($"✓ {summary.Users} users · {summary.StaffMembers} staff · {summary.Rewards} rewards · " +
                          $"{summary.Quizzes} quizzes · +{summary.AwardsAdded} achievements · +{summary.ClaimsAdded} claims · " +
                          $"+{summary.CompletionsAdded} quiz completions · +{summary.PendingAdded} pending redemptions");
        Console.WriteLine("Done. If the WebAPI was running during a reset, restart it (rewards-webapi in the dashboard).");
    }
}

return 0;

async Task DropDatabases(string connectionString)
{
    var master = new SqlConnectionStringBuilder(connectionString) { InitialCatalog = "master" };
    await using var conn = new SqlConnection(master.ConnectionString);
    await conn.OpenAsync();
    foreach (var db in new[] { RewardsDb, HangfireDb })
    {
        // Force-disconnect the WebAPI/Hangfire before dropping (they hold pooled connections).
        var sql = $"""
            IF DB_ID('{db}') IS NOT NULL
            BEGIN
                ALTER DATABASE [{db}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                DROP DATABASE [{db}];
            END
            """;
        await using var cmd = new SqlCommand(sql, conn) { CommandTimeout = 120 };
        await cmd.ExecuteNonQueryAsync();
    }
    // Hangfire creates its schema on WebAPI startup but needs the database to exist.
    await using var create = new SqlCommand($"CREATE DATABASE [{HangfireDb}]", conn) { CommandTimeout = 120 };
    await create.ExecuteNonQueryAsync();
}

ServiceProvider BuildServices(string connectionString, string? blobArg)
{
    // Deliberately minimal DI — NOT AddInfrastructureServices, which would start a
    // Hangfire server and bind Firebase/email/auth this tool doesn't need.
    var services = new ServiceCollection();
    services.AddSingleton<ICurrentUserService, SeederCurrentUserService>();
    services.AddSingleton<IDateTime, UtcDateTimeService>();
    services.AddScoped<AuditableEntitySaveChangesInterceptor>();
    services.AddScoped<AchievementIntegrationIdInterceptor>();
    services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(
        connectionString,
        builder => builder.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

    var blobConn = ConnectionResolver.ResolveBlob(blobArg);
    if (blobConn is not null)
    {
        var assetsDir = Path.Combine(AppContext.BaseDirectory, "Assets");
        services.AddSingleton<IDemoAssetProvider>(new AzuriteAssetProvider(new BlobServiceClient(blobConn), assetsDir));
    }
    else
    {
        Console.WriteLine("! blob storage not reachable — seeding without avatars/images");
    }

    return services.BuildServiceProvider();
}

int Usage()
{
    Console.WriteLine("""
        SSW.Rewards demo data seeder (usually via `rewards-dev db …` or the Aspire dashboard)

          seed  --dev-email <email> [--dev-name <name>] [--years N]     idempotent demo seed (re-run to top up)
          reset [--yes] [--no-seed] [seed options]                      drop DBs → migrate → seed

          --connection-string / --blob-connection-string override auto-discovery (env vars:
          ConnectionStrings__DefaultConnection, CloudBlobProviderOptions__ContentStorageConnectionString).
        """);
    return 0;
}

int Fail(string message)
{
    Console.Error.WriteLine($"rewards data-seeder: {message}");
    return 1;
}

file sealed class SeederCurrentUserService : ICurrentUserService
{
    public string GetUserId() => "demo-seeder";
    public string GetUserEmail() => "demo-seeder@local";
    public string GetUserFullName() => "Demo Seeder";
    public string? GetUserProfilePic() => null;
    public bool IsInRole(string role) => false;
}

file sealed class UtcDateTimeService : IDateTime
{
    public DateTime Now => DateTime.Now;
    public DateTime UtcNow => DateTime.UtcNow;
}

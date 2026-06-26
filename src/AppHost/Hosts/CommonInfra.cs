using Aspire.Hosting.Azure;

namespace SSW.Rewards.AppHost.Hosts;

// Cross-host infrastructure consumed by more than one host file: the SQL Server
// (persistent) with its two databases, and the Azurite blob emulator.
public static class CommonInfraExtensions
{
    public static CommonInfra AddCommonInfra(this IDistributedApplicationBuilder builder)
    {
        // One prompted SA password, stored in this AppHost's user-secrets.
        var saPassword = builder.AddParameter("sql-sa-password", secret: true);

        var sql = builder.AddSqlServer("rewards-sql", password: saPassword)
            .WithLifetime(ContainerLifetime.Persistent)
            .WithDataVolume("ssw-rewards-sql-data");

        var rewardsDb = sql.AddDatabase("rewards-db", "ssw.rewards");
        var hangfireDb = sql.AddDatabase("hangfire-db", "ssw.rewards.hangfire");

        var storage = builder.AddAzureStorage("rewards-storage")
            .RunAsEmulator(e => e.WithDataVolume("ssw-rewards-azurite-data"));
        var blobs = storage.AddBlobs("blobs");

        return new CommonInfra(sql, rewardsDb, hangfireDb, blobs);
    }
}

public sealed record CommonInfra(
    IResourceBuilder<SqlServerServerResource> SqlServer,
    IResourceBuilder<SqlServerDatabaseResource> RewardsDatabase,
    IResourceBuilder<SqlServerDatabaseResource> HangfireDatabase,
    IResourceBuilder<AzureBlobStorageResource> Blobs);

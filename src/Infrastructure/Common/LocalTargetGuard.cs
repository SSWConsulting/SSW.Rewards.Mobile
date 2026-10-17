using Azure.Storage.Blobs;
using Microsoft.Data.SqlClient;

namespace SSW.Rewards.Infrastructure.Common;

/// <summary>
/// Decides whether a connection string points at this machine — the demo seeder and DB reset
/// refuse to write anywhere else. Covers what Aspire hands out locally (SQL on
/// <c>127.0.0.1,port</c>/<c>localhost,port</c>, Azurite as <c>devstoreaccount1</c> on a loopback endpoint).
/// </summary>
public static class LocalTargetGuard
{
    private const string AzuriteAccountName = "devstoreaccount1";

    public static bool IsLocalSql(string connectionString)
    {
        SqlConnectionStringBuilder builder;
        try
        {
            builder = new SqlConnectionStringBuilder(connectionString);
        }
        catch (ArgumentException)
        {
            return false;
        }

        // SqlClient silently switches to the failover partner when the primary is down.
        if (!string.IsNullOrWhiteSpace(builder.FailoverPartner))
            return false;

        var host = builder.DataSource.Trim();
        foreach (var protocol in new[] { "tcp:", "np:", "lpc:" })
            if (host.StartsWith(protocol, StringComparison.OrdinalIgnoreCase))
                host = host[protocol.Length..];
        host = host.Split(',')[0].Split('\\')[0].Trim();

        return host is "." or "(local)"
            || host.StartsWith("(localdb)", StringComparison.OrdinalIgnoreCase)
            || IsLocalHost(host);
    }

    // Checks the endpoint the SDK will actually call, so a DevelopmentStorageProxyUri can't redirect it.
    public static bool IsLocalBlob(string connectionString)
    {
        BlobServiceClient client;
        try
        {
            client = new BlobServiceClient(connectionString);
        }
        catch (Exception e) when (e is FormatException or ArgumentException or InvalidOperationException)
        {
            return false;
        }

        return string.Equals(client.AccountName, AzuriteAccountName, StringComparison.OrdinalIgnoreCase)
            && IsLocalHost(client.Uri.Host);
    }

    private static bool IsLocalHost(string host) =>
        host.Equals("localhost", StringComparison.OrdinalIgnoreCase)
        || host.Equals("host.docker.internal", StringComparison.OrdinalIgnoreCase)
        || host is "127.0.0.1" or "::1" or "[::1]";
}

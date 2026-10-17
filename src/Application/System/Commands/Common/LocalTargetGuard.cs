using System.Data.Common;
using Microsoft.Data.SqlClient;

namespace SSW.Rewards.Application.System.Commands.Common;

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
        string source;
        try
        {
            source = new SqlConnectionStringBuilder(connectionString).DataSource;
        }
        catch (ArgumentException)
        {
            return false;
        }

        var host = source.Trim();
        foreach (var protocol in new[] { "tcp:", "np:", "lpc:" })
            if (host.StartsWith(protocol, StringComparison.OrdinalIgnoreCase))
                host = host[protocol.Length..];
        host = host.Split(',')[0].Split('\\')[0].Trim();

        return host is "." or "(local)"
            || host.StartsWith("(localdb)", StringComparison.OrdinalIgnoreCase)
            || IsLocalHost(host);
    }

    public static bool IsLocalBlob(string connectionString)
    {
        var parts = new DbConnectionStringBuilder();
        try
        {
            parts.ConnectionString = connectionString;
        }
        catch (ArgumentException)
        {
            return false;
        }

        if (parts.TryGetValue("UseDevelopmentStorage", out var dev) && bool.TryParse(dev?.ToString(), out var isDev) && isDev)
            return true;

        return parts.TryGetValue("AccountName", out var account)
            && string.Equals(account?.ToString(), AzuriteAccountName, StringComparison.OrdinalIgnoreCase)
            && parts.TryGetValue("BlobEndpoint", out var endpoint)
            && Uri.TryCreate(endpoint?.ToString(), UriKind.Absolute, out var uri)
            && IsLocalHost(uri.Host);
    }

    private static bool IsLocalHost(string host) =>
        host.Equals("localhost", StringComparison.OrdinalIgnoreCase)
        || host.Equals("host.docker.internal", StringComparison.OrdinalIgnoreCase)
        || host is "127.0.0.1" or "::1" or "[::1]";
}

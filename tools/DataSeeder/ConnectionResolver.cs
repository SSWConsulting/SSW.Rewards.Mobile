using System.Diagnostics;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace SSW.Rewards.DataSeeder;

/// <summary>
/// Resolves the SQL + blob connection strings for the local Aspire stack:
/// explicit CLI arg → environment variable → docker container discovery
/// (stable container names from AppHost CommonInfra + the AppHost user-secrets
/// SA password). Mirrors the lookup documented in .agents/skills/manage-database.
/// </summary>
public static partial class ConnectionResolver
{
    private const string AppHostUserSecretsId = "F76E3E10-FABB-4543-B949-549EEC500823";
    private const string SqlContainerName = "ssw-rewards-sql";
    private const string AzuriteContainerName = "ssw-rewards-azurite";

    // Azurite's well-known public dev credentials (not a secret).
    private const string AzuriteAccountName = "devstoreaccount1";
    private const string AzuriteAccountKey =
        "Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==";

    public static string? ResolveSql(string? cliValue, string database)
    {
        if (!string.IsNullOrWhiteSpace(cliValue)) return cliValue;

        var env = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");
        if (!string.IsNullOrWhiteSpace(env)) return env;

        var port = DockerHostPort(SqlContainerName, 1433);
        var password = ReadSaPasswordFromAppHostSecrets();
        if (port is null || password is null) return null;

        return $"Server=127.0.0.1,{port};Database={database};User ID=sa;Password={password};TrustServerCertificate=True";
    }

    public static string? ResolveBlob(string? cliValue)
    {
        if (!string.IsNullOrWhiteSpace(cliValue)) return cliValue;

        var env = Environment.GetEnvironmentVariable("CloudBlobProviderOptions__ContentStorageConnectionString");
        if (!string.IsNullOrWhiteSpace(env)) return env;

        var port = DockerHostPort(AzuriteContainerName, 10000);
        if (port is null) return null;

        return $"DefaultEndpointsProtocol=http;AccountName={AzuriteAccountName};AccountKey={AzuriteAccountKey};" +
               $"BlobEndpoint=http://127.0.0.1:{port}/{AzuriteAccountName};";
    }

    private static int? DockerHostPort(string containerName, int containerPort)
    {
        try
        {
            var psi = new ProcessStartInfo("docker", $"port {containerName} {containerPort}")
            { RedirectStandardOutput = true, RedirectStandardError = true, UseShellExecute = false };
            using var p = Process.Start(psi);
            if (p is null) return null;
            var output = p.StandardOutput.ReadToEnd();
            p.WaitForExit(10_000);
            // e.g. "0.0.0.0:59123" (possibly multiple lines for v4/v6)
            var match = HostPortRegex().Match(output);
            return match.Success ? int.Parse(match.Groups[1].Value) : null;
        }
        catch
        {
            return null;
        }
    }

    private static string? ReadSaPasswordFromAppHostSecrets()
    {
        var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var path = OperatingSystem.IsWindows()
            ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Microsoft", "UserSecrets", AppHostUserSecretsId, "secrets.json")
            : Path.Combine(home, ".microsoft", "usersecrets", AppHostUserSecretsId, "secrets.json");
        if (!File.Exists(path)) return null;
        using var doc = JsonDocument.Parse(File.ReadAllText(path));
        return doc.RootElement.TryGetProperty("Parameters:sql-sa-password", out var value) ? value.GetString() : null;
    }

    [GeneratedRegex(@":(\d+)\s*$", RegexOptions.Multiline)]
    private static partial Regex HostPortRegex();
}

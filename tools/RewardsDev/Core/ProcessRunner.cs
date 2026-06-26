using System.Diagnostics;
using System.Text.Json;

namespace SSW.Rewards.DevTool.Core;

// Small wrappers around external processes used by the switchers.
public static class ProcessRunner
{
    public static (bool ok, string log) Dotnet(string args) => Run("dotnet", args, 60_000);

    public static (bool ok, string output) Run(string file, string args, int timeoutMs)
    {
        try
        {
            var psi = new ProcessStartInfo(file, args)
            { RedirectStandardOutput = true, RedirectStandardError = true, UseShellExecute = false };
            using var p = Process.Start(psi);
            if (p is null) return (false, $"could not start {file}");
            var outp = p.StandardOutput.ReadToEnd();
            var err = p.StandardError.ReadToEnd();
            p.WaitForExit(timeoutMs);
            return (p.ExitCode == 0, outp + err);
        }
        catch (Exception ex) { return (false, ex.Message); }
    }

    // Resolve the machine's Tailscale MagicDNS name → https://<name>:<port>, or null if unavailable.
    public static string? ResolveTailscaleApi(int port)
    {
        var (ok, json) = Run("tailscale", "status --json", 5000);
        if (!ok) return null;
        try
        {
            using var doc = JsonDocument.Parse(json);
            var dns = doc.RootElement.GetProperty("Self").GetProperty("DNSName").GetString()?.TrimEnd('.');
            return string.IsNullOrWhiteSpace(dns) ? null : $"https://{dns}:{port}";
        }
        catch { return null; }
    }
}

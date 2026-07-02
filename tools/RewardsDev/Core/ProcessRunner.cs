using System.Diagnostics;
using System.Text.Json;

namespace SSW.Rewards.DevTool.Core;

// Small wrappers around external processes used by the switchers.
public static class ProcessRunner
{
    public static (bool ok, string log) Dotnet(string args) => Run("dotnet", args, 60_000);

    public static (bool ok, string output) Run(string file, string args, int timeoutMs)
        => Run(file, args, timeoutMs, closeStdin: false);

    public static (bool ok, string output) Run(string file, string args, int timeoutMs, bool closeStdin)
    {
        try
        {
            var psi = new ProcessStartInfo(file, args)
            { RedirectStandardOutput = true, RedirectStandardError = true, UseShellExecute = false };
            // Close the child's stdin so an unexpected interactive prompt (e.g. tailscale
            // asking to enable HTTPS) gets EOF and bails out fast instead of hanging.
            if (closeStdin) psi.RedirectStandardInput = true;
            using var p = Process.Start(psi);
            if (p is null) return (false, $"could not start {file}");
            if (closeStdin) p.StandardInput.Close();

            // Read both streams concurrently so a chatty child can't deadlock by
            // filling one buffer while we block on the other.
            var outTask = p.StandardOutput.ReadToEndAsync();
            var errTask = p.StandardError.ReadToEndAsync();

            if (!p.WaitForExit(timeoutMs))
            {
                try { p.Kill(entireProcessTree: true); } catch { /* already gone */ }
                return (false, $"{file} timed out after {timeoutMs}ms");
            }

            var outp = outTask.GetAwaiter().GetResult();
            var err = errTask.GetAwaiter().GetResult();
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

    // Start (idempotently) a Tailscale HTTPS proxy on :<port> → localhost:<port> so the phone
    // can reach the local API over a real, trusted cert. Best-effort: first-time setup can be slow
    // (cert issuance) and needs HTTPS/MagicDNS enabled on the tailnet.
    public static (bool ok, string log) EnsureTailscaleServe(int port)
    {
        // Fast path: if it's already serving this port, don't pay the first-time setup cost again.
        var (statusOk, status) = Run("tailscale", "serve status", 3_000, closeStdin: true);
        if (statusOk && status.Contains($":{port}")
            && (status.Contains($"localhost:{port}") || status.Contains($"127.0.0.1:{port}")))
            return (true, "already serving");

        return Run("tailscale", $"serve --bg --https {port} https+insecure://localhost:{port}", 8_000, closeStdin: true);
    }
}

using System.Text.Json;
using SSW.Rewards.DevTool.Apps;

namespace SSW.Rewards.DevTool.Core;

// Implementations behind each rewards-dev command. Program.cs only parses args and dispatches here.
public static class Commands
{
    public static int Usage()
    {
        Console.WriteLine("usage: rewards-dev <identity|api|env|show|reset> [target]");
        Console.WriteLine("  identity local | staging | prod              (Mobile + AdminUI + WebAPI)");
        Console.WriteLine("  api      local | staging | prod | tailscale  (Mobile + AdminUI)");
        Console.WriteLine("  env      local | staging | prod              (both of the above)");
        // Non-zero so scripts / AI callers can detect that no valid command was given.
        return 1;
    }

    public static int Fail(string msg)
    {
        Console.Error.WriteLine($"✗ {msg}");
        return 1;
    }

    // Resolve an api target (incl. tailscale) to a URL; returns null and prints why on failure.
    public static string? ResolveApi(string target)
    {
        if (target.Equals("tailscale", StringComparison.OrdinalIgnoreCase))
        {
            var url = ProcessRunner.ResolveTailscaleApi(Presets.ApiPort);
            return url ?? FailNull("Tailscale not detected. Install + `tailscale up`, then re-run. " +
                "Tip: `tailscale serve https / https+insecure://localhost:5001` gives the phone a trusted HTTPS URL.");
        }
        if (Presets.Api.TryGetValue(target, out var preset)) return preset;
        return FailNull($"unknown api target '{target}'. Use: {string.Join(" | ", Presets.Api.Keys)} | tailscale");

        static string? FailNull(string msg) { Fail(msg); return null; }
    }

    // Apply a target across all apps (the override files always carry both keys, in sync) and print it.
    public static int Apply(RepoPaths paths, TargetState next, bool changeIdentity)
    {
        MobileConfig.Write(paths, next);
        AdminUiConfig.Write(paths, next);
        Console.WriteLine("✓ Mobile  → " + paths.Rel(paths.MobileLocalFile));
        Console.WriteLine("✓ AdminUI → " + paths.Rel(paths.AdminLocalFile));

        if (changeIdentity)
        {
            var (ok, log) = WebApiConfig.SetSigningAuthority(paths, next.Authority);
            Console.WriteLine(ok
                ? "✓ WebAPI  → AppHost user-secret Parameters:signing-authority"
                : "⚠ WebAPI  → could not set AppHost signing-authority secret (set it manually). " + log.Trim());
        }

        Console.WriteLine();
        Console.WriteLine($"  ApiBaseUrl   = {next.Api}");
        Console.WriteLine($"  AuthorityUri = {next.Authority}");
        Console.WriteLine();
        Console.WriteLine("  Rebuild the mobile app, and restart `aspire run` (AdminUI refresh + WebAPI) to pick up changes.");
        return 0;
    }

    public static int Show(RepoPaths paths, bool json)
    {
        var state = MobileConfig.ReadCurrent(paths);
        var adminAuthority = AdminUiConfig.ReadAuthority(paths);
        var webApiAuthority = WebApiConfig.ReadSigningAuthority(paths);

        if (json)
        {
            var obj = new
            {
                apiBaseUrl = state.Api,
                authority = state.Authority,
                mobileSource = MobileConfig.Exists(paths) ? paths.Rel(paths.MobileLocalFile) : "committed default",
                adminUiAuthority = adminAuthority ?? "committed appsettings",
                webApiSigningAuthority = webApiAuthority ?? "(unset / prompted at run)"
            };
            Console.WriteLine(JsonSerializer.Serialize(obj, new JsonSerializerOptions { WriteIndented = true }));
            return 0;
        }

        Console.WriteLine($"ApiBaseUrl   = {state.Api}");
        Console.WriteLine($"AuthorityUri = {state.Authority}");
        Console.WriteLine($"  Mobile  : {(MobileConfig.Exists(paths) ? paths.Rel(paths.MobileLocalFile) : "committed default")}");
        Console.WriteLine($"  AdminUI : {(adminAuthority is null ? "committed appsettings" : paths.Rel(paths.AdminLocalFile))}");
        Console.WriteLine($"  WebAPI  : signing-authority = {webApiAuthority ?? "(unset / prompted at run)"}");
        return 0;
    }

    public static int Reset(RepoPaths paths)
    {
        var removed = new List<string>();
        if (MobileConfig.Exists(paths)) { MobileConfig.Delete(paths); removed.Add(paths.Rel(paths.MobileLocalFile)); }
        if (AdminUiConfig.Exists(paths)) { AdminUiConfig.Delete(paths); removed.Add(paths.Rel(paths.AdminLocalFile)); }
        Console.WriteLine(removed.Count > 0
            ? $"✓ removed local overrides: {string.Join(", ", removed)} — back to committed defaults."
            : "already on committed defaults (no local override files).");
        Console.WriteLine("  note: WebAPI signing-authority is an AppHost user-secret; set it via `identity <target>`.");
        return 0;
    }
}

using System.Text.Json;
using SSW.Rewards.DevTool.Apps;

namespace SSW.Rewards.DevTool.Core;

// Implementations behind each rewards-dev command. Program.cs only parses args and dispatches here.
public static class Commands
{
    public static int Usage()
    {
        Console.WriteLine("usage: rewards-dev <identity|api|env|secrets|show|reset> [target]   (try `rewards-dev help`)");
        Console.WriteLine("  identity local | staging | prod              (Mobile + AdminUI + WebAPI)");
        Console.WriteLine("  api      local | staging | prod | tailscale  (Mobile + AdminUI)");
        Console.WriteLine("  env      local | staging | prod              (both of the above)");
        Console.WriteLine("  secrets  check | edit | path                 (the one AppHost secrets store)");
        // Non-zero so scripts / AI callers can detect that no valid command was given.
        return 1;
    }

    // Full, self-teaching help — printed only when explicitly asked (`help`/`-h`/`--help`).
    // Exits 0 so it can be piped/read without looking like a failure.
    public static int Help()
    {
        Console.WriteLine("""
rewards-dev — switch which backend ALL SSW.Rewards apps point at, in sync.

USAGE
  rewards-dev <command> [target] [--json]

COMMANDS
  identity <target>   Set the identity authority   → Mobile + AdminUI + WebAPI
  api      <target>   Set the API base URL          → Mobile + AdminUI
  env      <target>   Set both identity AND api      → all of the above
  secrets  <action>   Manage the one AppHost secrets store (check | edit | path)
  show     [--json]   Print the current effective targets (and where each comes from)
  reset               Delete the local override files → back to committed defaults
  help                Show this help

TARGETS
  local      API https://localhost:5001         · identity https://localhost:14330
  staging    *-staging.azurewebsites.net          (safe default; works with no local backend)
  prod       api.rewards.ssw.com.au              · identity https://identity.ssw.com.au
  tailscale  (api only) this machine's Tailscale HTTPS URL — also starts `tailscale serve`
             so a phone/emulator can reach the local API over a trusted cert.

EXAMPLES
  rewards-dev env staging        # everything → staging
  rewards-dev api local          # AdminUI + Mobile → https://localhost:5001
  rewards-dev api tailscale      # stable phone URL + auto `tailscale serve`
  rewards-dev identity local     # all apps → local SSW Identity
  rewards-dev secrets check      # validate the AppHost secrets store (Keeper blob)
  rewards-dev secrets edit       # open secrets.json to paste the Keeper record
  rewards-dev show --json        # machine-readable current state (for scripts / AI)
  rewards-dev reset              # remove overrides

SECRETS (one store for the whole stack — Keeper is the only external resource)
  All stack secrets live in the AppHost user-secrets. Copy the single
  "SSW.Rewards — Aspire Dev Secrets" record from Keeper ▸ SSW.Rewards, then:
    rewards-dev secrets edit     # opens secrets.json — paste, save
    rewards-dev secrets check    # confirms every required key is present + real
    rewards-dev secrets path     # just print the file path

WHAT IT WRITES (all git-ignored, per-developer)
  Mobile   src/MobileUI/Constants.LocalDev.cs        (falls back to Constants.LocalDev.Default.cs)
  AdminUI  src/AdminUI/wwwroot/appsettings.Local.json (falls back to committed appsettings)
  WebAPI   AppHost user-secret Parameters:signing-authority

NOTES
  · `api`/`identity` change only their dimension; the other is preserved.
  · Rebuild the mobile app + restart `aspire run` (AdminUI/WebAPI) to pick up a change.
  · Emulators can't reach localhost — use `staging` or `tailscale`, not `local`.
  · Full guide: _docs/Aspire-Local-Dev.md
""");
        return 0;
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

    // Bring up the Tailscale HTTPS proxy so the `tailscale` API URL actually resolves on the phone.
    // Best-effort — prints how to do it manually if it can't (e.g. HTTPS not enabled on the tailnet).
    public static void EnsureTailscaleServe(int port)
    {
        var (ok, log) = ProcessRunner.EnsureTailscaleServe(port);
        Console.WriteLine(ok
            ? $"✓ tailscale serve → https :{port} → localhost:{port}"
            : $"⚠ couldn't auto-start `tailscale serve` (first-time cert issuance can be slow, and the " +
              $"tailnet needs HTTPS enabled). Run it once manually, then re-run: " +
              $"tailscale serve --bg --https {port} https+insecure://localhost:{port}  ({log.Trim()})");
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

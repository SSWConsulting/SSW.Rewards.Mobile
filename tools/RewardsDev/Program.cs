using System.Text.Json;
using SSW.Rewards.DevTool.Apps;
using SSW.Rewards.DevTool.Core;

// rewards-dev — one-command local-dev target switcher for ALL SSW.Rewards apps.
//
//   rewards-dev identity <local|staging|prod>            Mobile + AdminUI + WebAPI
//   rewards-dev api      <local|staging|prod|tailscale>  Mobile + AdminUI
//   rewards-dev env      <local|staging|prod>            both of the above
//   rewards-dev show [--json] | reset
//
// Logic lives in Core/ (presets, paths, state, process) and Apps/ (one config
// writer per app). Head-less / AI-automatable: no prompts, clear exit codes.
// See _docs/Aspire-Local-Dev.md.

if (args.Length == 0)
{
    Console.WriteLine("usage: rewards-dev <identity|api|env|show|reset> [target]");
    Console.WriteLine("  identity local | staging | prod              (Mobile + AdminUI + WebAPI)");
    Console.WriteLine("  api      local | staging | prod | tailscale  (Mobile + AdminUI)");
    Console.WriteLine("  env      local | staging | prod              (both of the above)");
    return 0;
}

var paths = RepoPaths.Discover();
if (paths is null) return Fail("could not locate the repo root (no SSW.Rewards.sln above cwd or tool dir).");

var cmd = args[0].ToLowerInvariant();
if (cmd == "show") return Show(paths, args.Contains("--json"));
if (cmd == "reset") return Reset(paths);

// The mobile override is the canonical current state; preserve the dimension a command isn't changing.
var current = MobileConfig.ReadCurrent(paths);
TargetState next;
bool changeIdentity;

switch (cmd)
{
    case "api":
        if (args.Length < 2) return Fail("api needs a target: local | staging | prod | tailscale");
        var apiUrl = ResolveApi(args[1]);
        if (apiUrl is null) return 1; // ResolveApi already printed the reason
        next = current with { Api = apiUrl };
        changeIdentity = false;
        break;

    case "identity":
        if (args.Length < 2) return Fail("identity needs a target: local | staging | prod");
        if (!Presets.Identity.TryGetValue(args[1], out var authority))
            return Fail($"unknown identity target '{args[1]}'. Use: {string.Join(" | ", Presets.Identity.Keys)}");
        next = current with { Authority = authority };
        changeIdentity = true;
        break;

    case "env":
        if (args.Length < 2) return Fail("env needs a target: local | staging | prod");
        if (!Presets.Api.TryGetValue(args[1], out var a) || !Presets.Identity.TryGetValue(args[1], out var au))
            return Fail($"unknown env target '{args[1]}'. Use: local | staging | prod");
        next = new TargetState(a, au);
        changeIdentity = true;
        break;

    default:
        return Fail($"unknown command '{cmd}'. Use identity | api | env | show | reset.");
}

// Apply across all apps — the two override files always carry both keys, in sync.
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

// ---- local helpers -------------------------------------------------------

int Fail(string msg)
{
    Console.Error.WriteLine($"✗ {msg}");
    return 1;
}

string? ResolveApi(string target)
{
    if (target.Equals("tailscale", StringComparison.OrdinalIgnoreCase))
    {
        var url = ProcessRunner.ResolveTailscaleApi(Presets.ApiPort);
        if (url is null)
        {
            Fail("Tailscale not detected. Install + `tailscale up`, then re-run. " +
                 "Tip: `tailscale serve https / https+insecure://localhost:5001` gives the phone a trusted HTTPS URL.");
            return null;
        }
        return url;
    }
    if (Presets.Api.TryGetValue(target, out var preset)) return preset;
    Fail($"unknown api target '{target}'. Use: {string.Join(" | ", Presets.Api.Keys)} | tailscale");
    return null;
}

int Show(RepoPaths p, bool json)
{
    var state = MobileConfig.ReadCurrent(p);
    var adminAuthority = AdminUiConfig.ReadAuthority(p);
    var webApiAuthority = WebApiConfig.ReadSigningAuthority(p);

    if (json)
    {
        var obj = new
        {
            apiBaseUrl = state.Api,
            authority = state.Authority,
            mobileSource = MobileConfig.Exists(p) ? p.Rel(p.MobileLocalFile) : "committed default",
            adminUiAuthority = adminAuthority ?? "committed appsettings",
            webApiSigningAuthority = webApiAuthority ?? "(unset / prompted at run)"
        };
        Console.WriteLine(JsonSerializer.Serialize(obj, new JsonSerializerOptions { WriteIndented = true }));
        return 0;
    }

    Console.WriteLine($"ApiBaseUrl   = {state.Api}");
    Console.WriteLine($"AuthorityUri = {state.Authority}");
    Console.WriteLine($"  Mobile  : {(MobileConfig.Exists(p) ? p.Rel(p.MobileLocalFile) : "committed default")}");
    Console.WriteLine($"  AdminUI : {(adminAuthority is null ? "committed appsettings" : p.Rel(p.AdminLocalFile))}");
    Console.WriteLine($"  WebAPI  : signing-authority = {webApiAuthority ?? "(unset / prompted at run)"}");
    return 0;
}

int Reset(RepoPaths p)
{
    var removed = new List<string>();
    if (MobileConfig.Exists(p)) { MobileConfig.Delete(p); removed.Add(p.Rel(p.MobileLocalFile)); }
    if (AdminUiConfig.Exists(p)) { AdminUiConfig.Delete(p); removed.Add(p.Rel(p.AdminLocalFile)); }
    Console.WriteLine(removed.Count > 0
        ? $"✓ removed local overrides: {string.Join(", ", removed)} — back to committed defaults."
        : "already on committed defaults (no local override files).");
    Console.WriteLine("  note: WebAPI signing-authority is an AppHost user-secret; set it via `identity <target>`.");
    return 0;
}

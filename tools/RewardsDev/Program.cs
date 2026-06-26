using System.Diagnostics;
using System.Text.Json;
using System.Text.RegularExpressions;

// rewards-dev — one-command local-dev target switcher for ALL SSW.Rewards apps.
//
//   rewards-dev identity <local|staging|prod>            switch identity authority for ALL apps
//   rewards-dev api      <local|staging|prod|tailscale>  switch API base URL for AdminUI + Mobile
//   rewards-dev env      <local|staging|prod>            switch BOTH at once
//   rewards-dev show                                     print the current effective targets
//   rewards-dev reset                                    remove local overrides (back to committed defaults)
//
// One invocation rewrites, in sync:
//   • Mobile  → git-ignored  src/MobileUI/Constants.LocalDev.cs        (LocalApiBaseUrl / LocalAuthorityUri)
//   • AdminUI → git-ignored  src/AdminUI/wwwroot/appsettings.Local.json (RewardsApiUrl / Local:Authority)
//   • WebAPI  → AppHost user-secret  Parameters:signing-authority      (identity only; token-validation authority)
//
// Designed to be head-less / AI-automatable: no prompts, clear exit codes, machine-readable `show --json`.

// ---- presets -------------------------------------------------------------
var apiTargets = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
{
    ["local"]   = "https://localhost:5001",
    ["staging"] = "https://app-sswrewards-api-staging.azurewebsites.net",
    ["prod"]    = "https://api.rewards.ssw.com.au",
};
var identityTargets = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
{
    ["local"]   = "https://localhost:14330",
    ["staging"] = "https://app-ssw-ident-staging-api.azurewebsites.net",
    ["prod"]    = "https://identity.ssw.com.au",
};
const string DefaultApi = "https://app-sswrewards-api-staging.azurewebsites.net";
const string DefaultAuthority = "https://identity.ssw.com.au";
const int ApiPort = 5001; // used for the tailscale preset

int Fail(string msg) { Console.Error.WriteLine($"✗ {msg}"); return 1; }

if (args.Length == 0)
{
    Console.WriteLine("usage: rewards-dev <identity|api|env|show|reset> [target]");
    Console.WriteLine("  identity local | staging | prod              (Mobile + AdminUI + WebAPI)");
    Console.WriteLine("  api      local | staging | prod | tailscale  (Mobile + AdminUI)");
    Console.WriteLine("  env      local | staging | prod              (both of the above)");
    return 0;
}

var repoRoot = FindRepoRoot();
if (repoRoot is null) return Fail("could not locate the repo root (no SSW.Rewards.sln above cwd or tool dir).");

var mobileLocalFile  = Path.Combine(repoRoot, "src", "MobileUI", "Constants.LocalDev.cs");
var adminLocalFile   = Path.Combine(repoRoot, "src", "AdminUI", "wwwroot", "appsettings.Local.json");
var appHostProject   = Path.Combine(repoRoot, "src", "AppHost", "SSW.Rewards.AppHost.csproj");

// Canonical current state lives in the mobile override (CLI keeps everything in sync).
var (api, authority) = ReadCurrent(mobileLocalFile);
var cmd = args[0].ToLowerInvariant();

bool changeIdentity = false;

switch (cmd)
{
    case "show":
        return Show(args.Contains("--json"));

    case "reset":
    {
        var removed = new List<string>();
        if (File.Exists(mobileLocalFile)) { File.Delete(mobileLocalFile); removed.Add(Rel(mobileLocalFile)); }
        if (File.Exists(adminLocalFile))  { File.Delete(adminLocalFile);  removed.Add(Rel(adminLocalFile)); }
        Console.WriteLine(removed.Count > 0
            ? $"✓ removed local overrides: {string.Join(", ", removed)} — back to committed defaults."
            : "already on committed defaults (no local override files).");
        Console.WriteLine("  note: WebAPI signing-authority is an AppHost user-secret; set it via `identity <target>`.");
        return 0;
    }

    case "api":
    {
        if (args.Length < 2) return Fail("api needs a target: local | staging | prod | tailscale");
        var target = args[1];
        if (target.Equals("tailscale", StringComparison.OrdinalIgnoreCase))
        {
            var url = ResolveTailscale(ApiPort);
            if (url is null) return Fail("Tailscale not detected. Install + `tailscale up`, then re-run. " +
                "Tip: `tailscale serve https / https+insecure://localhost:5001` gives the phone a trusted HTTPS URL.");
            api = url;
        }
        else if (apiTargets.TryGetValue(target, out var url)) api = url;
        else return Fail($"unknown api target '{target}'. Use: {string.Join(" | ", apiTargets.Keys)} | tailscale");
        break;
    }

    case "identity":
    {
        if (args.Length < 2) return Fail("identity needs a target: local | staging | prod");
        if (!identityTargets.TryGetValue(args[1], out var url))
            return Fail($"unknown identity target '{args[1]}'. Use: {string.Join(" | ", identityTargets.Keys)}");
        authority = url;
        changeIdentity = true;
        break;
    }

    case "env":
    {
        if (args.Length < 2) return Fail("env needs a target: local | staging | prod");
        if (!apiTargets.TryGetValue(args[1], out var a) || !identityTargets.TryGetValue(args[1], out var au))
            return Fail($"unknown env target '{args[1]}'. Use: local | staging | prod");
        api = a; authority = au;
        changeIdentity = true;
        break;
    }

    default:
        return Fail($"unknown command '{cmd}'. Use identity | api | env | show | reset.");
}

// ---- apply across all apps (both override files always carry both keys, in sync) ----
WriteMobile(mobileLocalFile, api, authority);
WriteAdminUi(adminLocalFile, api, authority);
Console.WriteLine("✓ Mobile  → " + Rel(mobileLocalFile));
Console.WriteLine("✓ AdminUI → " + Rel(adminLocalFile));

if (changeIdentity)
{
    var (ok, log) = SetSigningAuthority(appHostProject, authority);
    Console.WriteLine(ok
        ? "✓ WebAPI  → AppHost user-secret Parameters:signing-authority"
        : "⚠ WebAPI  → could not set AppHost signing-authority secret (set it manually). " + log.Trim());
}

Console.WriteLine();
Console.WriteLine($"  ApiBaseUrl   = {api}");
Console.WriteLine($"  AuthorityUri = {authority}");
Console.WriteLine();
Console.WriteLine("  Rebuild the mobile app, and restart `aspire run` (AdminUI refresh + WebAPI) to pick up changes.");
return 0;

// ---- commands ------------------------------------------------------------

int Show(bool json)
{
    var (a, auth) = ReadCurrent(mobileLocalFile);
    var adminAuthority = ReadAdminUi(adminLocalFile)?.authority;
    var webApiAuthority = ReadSigningAuthority(appHostProject);
    if (json)
    {
        var obj = new
        {
            apiBaseUrl = a,
            authority = auth,
            mobileSource = File.Exists(mobileLocalFile) ? Rel(mobileLocalFile) : "committed default",
            adminUiAuthority = adminAuthority ?? "committed appsettings",
            webApiSigningAuthority = webApiAuthority ?? "(unset / prompted at run)"
        };
        Console.WriteLine(JsonSerializer.Serialize(obj, new JsonSerializerOptions { WriteIndented = true }));
        return 0;
    }
    Console.WriteLine($"ApiBaseUrl   = {a}");
    Console.WriteLine($"AuthorityUri = {auth}");
    Console.WriteLine($"  Mobile  : {(File.Exists(mobileLocalFile) ? Rel(mobileLocalFile) : "committed default")}");
    Console.WriteLine($"  AdminUI : {(adminAuthority is null ? "committed appsettings" : Rel(adminLocalFile))}");
    Console.WriteLine($"  WebAPI  : signing-authority = {webApiAuthority ?? "(unset / prompted at run)"}");
    return 0;
}

// ---- read/write helpers --------------------------------------------------

(string api, string authority) ReadCurrent(string file)
{
    var a = DefaultApi; var auth = DefaultAuthority;
    if (File.Exists(file))
    {
        var text = File.ReadAllText(file);
        a = Match(text, "LocalApiBaseUrl") ?? a;
        auth = Match(text, "LocalAuthorityUri") ?? auth;
    }
    return (a, auth);

    static string? Match(string text, string name)
    {
        var m = Regex.Match(text, name + @"\s*=\s*""([^""]*)""");
        return m.Success ? m.Groups[1].Value : null;
    }
}

void WriteMobile(string file, string apiUrl, string authorityUrl)
{
    File.WriteAllText(file,
$$"""
// <auto-generated> rewards-dev — local dev overrides. GIT-IGNORED. Do not commit.
namespace SSW.Rewards;

public static partial class Constants
{
    private const string LocalApiBaseUrl = "{{apiUrl}}";
    private const string LocalAuthorityUri = "{{authorityUrl}}";
}
""");
}

(string api, string authority)? ReadAdminUi(string file)
{
    if (!File.Exists(file)) return null;
    try
    {
        using var doc = JsonDocument.Parse(File.ReadAllText(file));
        var root = doc.RootElement;
        var a = root.TryGetProperty("RewardsApiUrl", out var ap) ? ap.GetString() ?? DefaultApi : DefaultApi;
        var auth = root.TryGetProperty("Local", out var local) && local.TryGetProperty("Authority", out var au)
            ? au.GetString() ?? DefaultAuthority : DefaultAuthority;
        return (a, auth);
    }
    catch { return null; }
}

void WriteAdminUi(string file, string apiUrl, string authorityUrl)
{
    // Partial override — merged over the committed appsettings(.Development).json by
    // the loader in AdminUI/Program.cs. Only the two switchable keys are written.
    var obj = new Dictionary<string, object?>
    {
        ["_comment"] = "GIT-IGNORED. Written by `rewards-dev`. Overrides committed appsettings. Do not commit.",
        ["RewardsApiUrl"] = apiUrl,
        ["Local"] = new Dictionary<string, object?> { ["Authority"] = authorityUrl },
    };
    Directory.CreateDirectory(Path.GetDirectoryName(file)!);
    File.WriteAllText(file, JsonSerializer.Serialize(obj, new JsonSerializerOptions { WriteIndented = true }));
}

(bool ok, string log) SetSigningAuthority(string appHostCsproj, string authorityUrl)
{
    if (!File.Exists(appHostCsproj)) return (false, "AppHost project not found.");
    return RunDotnet($"user-secrets set \"Parameters:signing-authority\" \"{authorityUrl}\" --project \"{appHostCsproj}\"");
}

string? ReadSigningAuthority(string appHostCsproj)
{
    if (!File.Exists(appHostCsproj)) return null;
    var (ok, log) = RunDotnet($"user-secrets list --project \"{appHostCsproj}\"");
    if (!ok) return null;
    var m = Regex.Match(log, @"Parameters:signing-authority\s*=\s*(\S+)");
    return m.Success ? m.Groups[1].Value : null;
}

// ---- process / environment ----------------------------------------------

static (bool ok, string log) RunDotnet(string args)
{
    try
    {
        var psi = new ProcessStartInfo("dotnet", args)
        { RedirectStandardOutput = true, RedirectStandardError = true, UseShellExecute = false };
        using var p = Process.Start(psi);
        if (p is null) return (false, "could not start dotnet");
        var outp = p.StandardOutput.ReadToEnd();
        var err = p.StandardError.ReadToEnd();
        p.WaitForExit(60_000);
        return (p.ExitCode == 0, outp + err);
    }
    catch (Exception ex) { return (false, ex.Message); }
}

string? ResolveTailscale(int port)
{
    try
    {
        var psi = new ProcessStartInfo("tailscale", "status --json")
        { RedirectStandardOutput = true, RedirectStandardError = true, UseShellExecute = false };
        using var p = Process.Start(psi);
        if (p is null) return null;
        var json = p.StandardOutput.ReadToEnd();
        p.WaitForExit(5000);
        if (p.ExitCode != 0) return null;
        using var doc = JsonDocument.Parse(json);
        var dns = doc.RootElement.GetProperty("Self").GetProperty("DNSName").GetString()?.TrimEnd('.');
        return string.IsNullOrWhiteSpace(dns) ? null : $"https://{dns}:{port}";
    }
    catch { return null; }
}

static string? FindRepoRoot()
{
    foreach (var start in new[] { Directory.GetCurrentDirectory(), AppContext.BaseDirectory })
    {
        var d = new DirectoryInfo(start);
        while (d is not null)
        {
            if (File.Exists(Path.Combine(d.FullName, "SSW.Rewards.sln"))) return d.FullName;
            d = d.Parent;
        }
    }
    return null;
}

string Rel(string p) => Path.GetRelativePath(Directory.GetCurrentDirectory(), p);

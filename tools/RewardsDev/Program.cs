using SSW.Rewards.DevTool.Apps;
using SSW.Rewards.DevTool.Core;

// rewards-dev — one-command local-dev target switcher for ALL SSW.Rewards apps.
//
//   rewards-dev identity <local|staging|prod>            Mobile + AdminUI + WebAPI
//   rewards-dev api      <local|staging|prod|tailscale>  Mobile + AdminUI
//   rewards-dev env      <local|staging|prod>            both of the above
//   rewards-dev secrets  <check|edit|path|sync-mobile>   AppHost store + isolated mobile store
//   rewards-dev mobile   <android|ios>                    build/run MAUI for one platform
//   rewards-dev show [--json] | reset
//
// This file is just arg-parsing + dispatch. Command implementations live in Core/Commands.cs;
// presets/paths/state/process in Core/; per-app config writers in Apps/. See _docs/Aspire-Local-Dev.md.

if (args.Length == 0) return Commands.Usage();

var first = args[0].ToLowerInvariant();
if (first is "help" or "-h" or "--help" or "-?" or "/?") return Commands.Help();

var paths = RepoPaths.Discover();
if (paths is null) return Commands.Fail("could not locate the repo root (no SSW.Rewards.sln above cwd or tool dir).");

var cmd = first;
if (cmd == "show") return Commands.Show(paths, args.Contains("--json"));
if (cmd == "reset") return Commands.Reset(paths);
if (cmd == "secrets") return Secrets.Dispatch(paths, args);
if (cmd == "mobile") return Mobile.Dispatch(paths, args);
if (cmd == "db") return Db.Dispatch(paths, args);

// The mobile override is the canonical current state; preserve the dimension a command isn't changing.
var current = MobileConfig.ReadCurrent(paths);

switch (cmd)
{
    case "api":
        if (args.Length < 2) return Commands.Fail("api needs a target: local | staging | prod | tailscale");
        var apiUrl = Commands.ResolveApi(args[1]);
        if (apiUrl is null) return 1;
        // Switching to tailscale is only useful if the HTTPS proxy is up — start it for the user.
        if (args[1].Equals("tailscale", StringComparison.OrdinalIgnoreCase))
            Commands.EnsureTailscaleServe(Presets.ApiPort);
        return Commands.Apply(paths, current with { Api = apiUrl }, changeIdentity: false);

    case "identity":
        if (args.Length < 2) return Commands.Fail("identity needs a target: local | staging | prod");
        if (!Presets.Identity.TryGetValue(args[1], out var authority))
            return Commands.Fail($"unknown identity target '{args[1]}'. Use: {string.Join(" | ", Presets.Identity.Keys)}");
        return Commands.Apply(paths, current with { Authority = authority }, changeIdentity: true);

    case "env":
        if (args.Length < 2) return Commands.Fail("env needs a target: local | staging | prod");
        if (!Presets.Api.TryGetValue(args[1], out var a) || !Presets.Identity.TryGetValue(args[1], out var au))
            return Commands.Fail($"unknown env target '{args[1]}'. Use: local | staging | prod");
        return Commands.Apply(paths, new TargetState(a, au), changeIdentity: true);

    default:
        return Commands.Fail($"unknown command '{cmd}'. Use identity | api | env | secrets | mobile | db | show | reset.");
}

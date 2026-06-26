using SSW.Rewards.DevTool.Apps;
using SSW.Rewards.DevTool.Core;

// rewards-dev — one-command local-dev target switcher for ALL SSW.Rewards apps.
//
//   rewards-dev identity <local|staging|prod>            Mobile + AdminUI + WebAPI
//   rewards-dev api      <local|staging|prod|tailscale>  Mobile + AdminUI
//   rewards-dev env      <local|staging|prod>            both of the above
//   rewards-dev show [--json] | reset
//
// This file is just arg-parsing + dispatch. Command implementations live in Core/Commands.cs;
// presets/paths/state/process in Core/; per-app config writers in Apps/. See _docs/Aspire-Local-Dev.md.

if (args.Length == 0) return Commands.Usage();

var paths = RepoPaths.Discover();
if (paths is null) return Commands.Fail("could not locate the repo root (no SSW.Rewards.sln above cwd or tool dir).");

var cmd = args[0].ToLowerInvariant();
if (cmd == "show") return Commands.Show(paths, args.Contains("--json"));
if (cmd == "reset") return Commands.Reset(paths);

// The mobile override is the canonical current state; preserve the dimension a command isn't changing.
var current = MobileConfig.ReadCurrent(paths);

switch (cmd)
{
    case "api":
        if (args.Length < 2) return Commands.Fail("api needs a target: local | staging | prod | tailscale");
        var apiUrl = Commands.ResolveApi(args[1]);
        return apiUrl is null ? 1 : Commands.Apply(paths, current with { Api = apiUrl }, changeIdentity: false);

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
        return Commands.Fail($"unknown command '{cmd}'. Use identity | api | env | show | reset.");
}

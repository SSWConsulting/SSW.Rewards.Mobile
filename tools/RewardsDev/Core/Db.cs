using System.Diagnostics;

namespace SSW.Rewards.DevTool.Core;

// `rewards-dev db …` — demo-data seeding + DB reset, delegated to tools/DataSeeder.
// Stdio is inherited so the reset confirmation prompt and seed progress stream live.
public static class Db
{
    public static int Dispatch(RepoPaths paths, string[] args)
    {
        var sub = args.Length > 1 ? args[1].ToLowerInvariant() : "help";
        if (sub is not ("seed" or "reset"))
            return Commands.Fail("db needs a sub-command: seed | reset (see `rewards-dev help`)");

        var project = Path.Combine(paths.Root, "tools", "DataSeeder");
        var passthrough = string.Join(' ', args.Skip(1).Select(Quote));
        var psi = new ProcessStartInfo("dotnet", $"run --project {Quote(project)} --verbosity quiet -- {passthrough}")
        { UseShellExecute = false };
        using var p = Process.Start(psi);
        if (p is null) return Commands.Fail("could not start dotnet");
        p.WaitForExit();
        return p.ExitCode;
    }

    private static string Quote(string value) =>
        value.Contains(' ') || value.Contains('"') ? $"\"{value.Replace("\"", "\\\"")}\"" : value;
}

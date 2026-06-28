using System.Diagnostics;

namespace SSW.Rewards.DevTool.Core;

// `rewards-dev mobile …` — build/run the MAUI app with the right flags so the documented
// command "just works", including on Android-only machines (no iOS workload).
//
// The MobileUI csproj multi-targets net10.0-ios;net10.0-android. Restoring with the default
// TFM list demands the iOS workload even for an Android-only build, so we pass the project's
// own overridable `MobileTargetFrameworks` property (NOT the well-known `TargetFrameworks`,
// which would leak into referenced libraries and break them).
public static class Mobile
{
    public static int Dispatch(RepoPaths paths, string[] args)
    {
        var sub = (args.Length > 1 ? args[1] : "").ToLowerInvariant();
        return sub switch
        {
            "android"        => Build(paths, run: true),
            "android-build"  => Build(paths, run: false),
            _ => Commands.Fail("usage: rewards-dev mobile <android|android-build>  (Android-only build that doesn't need the iOS workload)"),
        };
    }

    private static int Build(RepoPaths paths, bool run)
    {
        var project = paths.MobileProject;

        // Firebase config must exist or the Android build fails with a clear MSBuild error anyway —
        // nudge the dev toward the one command that produces it.
        if (!File.Exists(paths.AndroidFirebaseFile))
        {
            Console.WriteLine("⚠ Platforms/Android/google-services.json is missing.");
            Console.WriteLine("  Run `rewards-dev secrets sync-mobile` first (writes it from the isolated mobile store).");
            return 1;
        }

        var target = run ? "-t:Run " : "";
        var dotnetArgs = $"build \"{project}\" {target}-f net10.0-android -c Debug -p:MobileTargetFrameworks=net10.0-android";
        Console.WriteLine($"› dotnet {dotnetArgs}");
        Console.WriteLine(run
            ? "  (emulators can't reach localhost — make sure you ran `rewards-dev api staging` or `api tailscale`)"
            : "");

        // Inherit the console so the (long) build streams live; no timeout.
        var psi = new ProcessStartInfo("dotnet", dotnetArgs) { UseShellExecute = false };
        using var p = Process.Start(psi);
        if (p is null) return Commands.Fail("could not start dotnet");
        p.WaitForExit();
        return p.ExitCode == 0 ? 0 : Commands.Fail($"mobile build failed (exit {p.ExitCode}).");
    }
}

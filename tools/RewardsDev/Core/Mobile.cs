using System.Diagnostics;

namespace SSW.Rewards.DevTool.Core;

// `rewards-dev mobile …` — build/run the MAUI app for a single platform with the right flags so
// the documented command "just works", including on machines that only have one workload installed
// (e.g. an Android-only box with no iOS workload — handy when you test on an Android device).
//
// The MobileUI csproj multi-targets net10.0-ios;net10.0-android. Restoring with the default TFM
// list demands BOTH workloads, so we pass the project's own overridable `MobileTargetFrameworks`
// property (NOT the well-known `TargetFrameworks`, which would leak into referenced libraries and
// break them) to scope the restore/build to the one platform you asked for.
public static class Mobile
{
    private record Platform(string Name, string Tfm, string Workload);

    private static readonly Platform Android = new("Android", "net10.0-android", "maui-android");
    private static readonly Platform Ios     = new("iOS",     "net10.0-ios",     "maui-ios");

    public static int Dispatch(RepoPaths paths, string[] args)
    {
        var sub = (args.Length > 1 ? args[1] : "").ToLowerInvariant();
        return sub switch
        {
            "android"       => Build(paths, Android, run: true),
            "android-build" => Build(paths, Android, run: false),
            "ios"           => Build(paths, Ios, run: true),
            "ios-build"     => Build(paths, Ios, run: false),
            _ => Commands.Fail("usage: rewards-dev mobile <android|android-build|ios|ios-build>  (single-platform build; only that platform's workload is needed)"),
        };
    }

    private static int Build(RepoPaths paths, Platform platform, bool run)
    {
        var project = paths.MobileProject;

        // Firebase config must exist or the build fails with a clear MSBuild error anyway —
        // nudge the dev toward the one command that produces it.
        var firebaseFile = platform == Android ? paths.AndroidFirebaseFile : paths.IosFirebaseFile;
        if (!File.Exists(firebaseFile))
        {
            var rel = paths.Rel(firebaseFile);
            Console.WriteLine($"⚠ {rel} is missing.");
            Console.WriteLine("  Run `rewards-dev secrets sync-mobile` first (writes it from the isolated mobile store).");
            return 1;
        }

        var target = run ? "-t:Run " : "";
        var dotnetArgs = $"build \"{project}\" {target}-f {platform.Tfm} -c Debug -p:MobileTargetFrameworks={platform.Tfm}";
        Console.WriteLine($"› dotnet {dotnetArgs}");
        if (run)
            Console.WriteLine("  (emulators/simulators can't reach localhost — make sure you ran `rewards-dev api staging` or `api tailscale`)");

        // Inherit the console so the (long) build streams live; no timeout.
        var psi = new ProcessStartInfo("dotnet", dotnetArgs) { UseShellExecute = false };
        using var p = Process.Start(psi);
        if (p is null) return Commands.Fail("could not start dotnet");
        p.WaitForExit();
        return p.ExitCode == 0
            ? 0
            : Commands.Fail($"{platform.Name} build failed (exit {p.ExitCode}). Need the {platform.Workload} workload? `dotnet workload install {platform.Workload}`.");
    }
}

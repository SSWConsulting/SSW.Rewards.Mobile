using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.Json;

namespace SSW.Rewards.DevTool.Core;

// `rewards-dev secrets …` — manage the ONE secrets store the whole stack reads from:
// the AppHost user-secrets (see _docs/Aspire-Local-Dev.md). The flow is deliberately
// dumb-simple so Keeper is the only external resource a dev needs:
//   1. copy the "SSW.Rewards — Aspire Dev Secrets" record from Keeper
//   2. `rewards-dev secrets edit`   → opens secrets.json in your editor; paste, save
//   3. `rewards-dev secrets check`  → confirms every required key is present + real
public static class Secrets
{
    // The keys a dev must supply from Keeper (the AppHost:* / Aspire:* keys are generated
    // per-machine by Aspire and must NOT be shared). Order = paste order in the doc.
    public static readonly (string Key, string Source)[] Required =
    [
        ("Parameters:firebase-credentials",            "Keeper ▸ Firebase-Credentials.Local.json (STAGING)"),
        ("Parameters:sendgrid-api-key",                "Keeper ▸ Developer Secrets ▸ SendGridAPIKey"),
        ("Parameters:email-user",                      "Keeper ▸ Developer Secrets ▸ EmailUser"),
        ("Parameters:email-password",                  "Keeper ▸ Developer Secrets ▸ EmailPassword"),
        ("Parameters:signing-authority",               "Keeper ▸ Developer Secrets ▸ SigningAuthority (staging is the safe default)"),
        ("Parameters:sql-sa-password",                 "any strong password (set once; stays stable with the SQL data volume)"),
        ("Parameters:mobile-google-services-json",     "Keeper ▸ google-services.json (STAGING) — Android Firebase"),
        ("Parameters:mobile-google-service-info-plist","Keeper ▸ GoogleService-Info.plist (STAGING) — iOS Firebase"),
    ];

    public static int Dispatch(RepoPaths paths, string[] args)
    {
        var sub = (args.Length > 1 ? args[1] : "check").ToLowerInvariant();
        return sub switch
        {
            "check" or "validate" or "doctor" => Check(paths),
            "edit" or "open"                  => Edit(paths),
            "path" or "where"                 => Path(paths),
            _ => Commands.Fail($"unknown secrets sub-command '{sub}'. Use: check | edit | path"),
        };
    }

    public static int Path(RepoPaths paths)
    {
        var file = paths.UserSecretsFile();
        if (file is null) return Commands.Fail("AppHost has no <UserSecretsId> — cannot locate the secrets file.");
        Console.WriteLine(file);
        return 0;
    }

    // Open the secrets.json in the OS default editor so the dev can paste the Keeper blob.
    public static int Edit(RepoPaths paths)
    {
        var file = paths.UserSecretsFile();
        if (file is null) return Commands.Fail("AppHost has no <UserSecretsId> — cannot locate the secrets file.");

        var dir = System.IO.Path.GetDirectoryName(file)!;
        Directory.CreateDirectory(dir);
        if (!File.Exists(file)) File.WriteAllText(file, "{\n}\n");

        Console.WriteLine($"secrets file: {file}");
        Console.WriteLine("Paste the \"SSW.Rewards — Aspire Dev Secrets\" record from Keeper, save, then run: rewards-dev secrets check");
        try
        {
            // UseShellExecute lets the OS pick the right opener (open / xdg-open / default editor).
            Process.Start(new ProcessStartInfo(file) { UseShellExecute = true });
            return 0;
        }
        catch (Exception ex)
        {
            // Headless / no GUI editor — fall back to the literal per-OS command.
            var manual = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? $"notepad \"{file}\""
                : RuntimeInformation.IsOSPlatform(OSPlatform.OSX)
                    ? $"open \"{file}\""
                    : $"xdg-open \"{file}\"";
            Console.WriteLine($"⚠ couldn't auto-open ({ex.Message}). Open it yourself:");
            Console.WriteLine($"  {manual}");
            return 0;
        }
    }

    // Validate that every required key is present and non-placeholder. Exit non-zero if not,
    // so scripts / the dashboard / AI callers can gate on it.
    public static int Check(RepoPaths paths)
    {
        var file = paths.UserSecretsFile();
        if (file is null) return Commands.Fail("AppHost has no <UserSecretsId> — cannot locate the secrets file.");

        Dictionary<string, string> values;
        if (!File.Exists(file))
        {
            Console.WriteLine($"✗ no secrets file yet: {file}");
            Console.WriteLine("  Run `rewards-dev secrets edit`, paste the Keeper record, then re-check.");
            return 1;
        }

        try
        {
            // BOM-safe: dotnet user-secrets writes UTF-8, sometimes with a BOM.
            using var doc = JsonDocument.Parse(File.ReadAllText(file));
            values = doc.RootElement.EnumerateObject()
                .ToDictionary(p => p.Name, p => p.Value.ValueKind == JsonValueKind.String ? p.Value.GetString() ?? "" : p.Value.GetRawText());
        }
        catch (Exception ex)
        {
            return Commands.Fail($"secrets file is not valid JSON ({ex.Message}). Fix it: rewards-dev secrets edit");
        }

        var missing = new List<(string Key, string Source)>();
        Console.WriteLine($"Secrets store: {file}");
        foreach (var (key, source) in Required)
        {
            var ok = values.TryGetValue(key, out var v) && !IsPlaceholder(v);
            Console.WriteLine($"  {(ok ? "✓" : "✗")} {key}");
            if (!ok) missing.Add((key, source));
        }

        if (missing.Count == 0)
        {
            Console.WriteLine($"\n✓ all {Required.Length} secrets present. You're ready: `aspire run`.");
            return 0;
        }

        Console.WriteLine($"\n✗ {missing.Count} missing/placeholder — get them from Keeper ▸ SSW.Rewards:");
        foreach (var (key, source) in missing)
            Console.WriteLine($"    {key.Replace("Parameters:", "")}  ←  {source}");
        Console.WriteLine("\n  Easiest: copy the single \"SSW.Rewards — Aspire Dev Secrets\" record, then:");
        Console.WriteLine("    rewards-dev secrets edit   # paste it, save");
        Console.WriteLine("    rewards-dev secrets check  # re-validate");
        return 1;
    }

    // Empty, whitespace, or one of the committed placeholder markers = "not really set".
    private static bool IsPlaceholder(string v)
    {
        if (string.IsNullOrWhiteSpace(v)) return true;
        var t = v.Trim();
        return t.StartsWith('<')
            || t.Contains("Copy from Keeper", StringComparison.OrdinalIgnoreCase)
            || t.Contains("placeholder", StringComparison.OrdinalIgnoreCase)
            || t.Contains("REPLACE", StringComparison.Ordinal);
    }
}

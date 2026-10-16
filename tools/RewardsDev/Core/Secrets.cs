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

    // The subset of keys the MOBILE app needs — the Firebase client config that ends up bundled
    // in the APK/IPA. These (and ONLY these) are copied into the isolated mobile store. Backend
    // secrets (sql/sendgrid/email/firebase-credentials/signing-authority) must NEVER be here.
    public static readonly (string Key, string FileRelPath)[] MobileKeys =
    [
        ("Parameters:mobile-google-services-json",     "src/MobileUI/Platforms/Android/google-services.json"),
        ("Parameters:mobile-google-service-info-plist","src/MobileUI/Platforms/iOS/GoogleService-Info.plist"),
    ];

    public static int Dispatch(RepoPaths paths, string[] args)
    {
        var sub = (args.Length > 1 ? args[1] : "check").ToLowerInvariant();
        return sub switch
        {
            "check" or "validate" or "doctor" => Check(paths),
            "edit" or "open"                  => Edit(paths),
            "path" or "where"                 => Path(paths),
            "sync-mobile" or "mobile"         => SyncMobile(paths),
            _ => Commands.Fail($"unknown secrets sub-command '{sub}'. Use: check | edit | path | sync-mobile"),
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
        if (!File.Exists(file))
        {
            // Fresh machine: seed a skeleton with every expected key empty, so the dev either pastes
            // the whole Keeper record over it, or fills values key-by-key and `secrets check` shows
            // exactly what's still missing.
            var skeleton = new Dictionary<string, string>();
            foreach (var (key, _) in Required) skeleton[key] = "";
            File.WriteAllText(file, JsonSerializer.Serialize(skeleton, new JsonSerializerOptions { WriteIndented = true }));
        }

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

        if (!Load(file, out values, out var loadErr))
            return Commands.Fail(loadErr);

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

    // Copy ONLY the mobile Firebase keys from the AppHost (paste-target) store into the ISOLATED
    // mobile user-secrets store, then write the git-ignored Firebase files from those values.
    // By construction this never touches backend secrets, so the mobile store / APK can't leak them.
    public static int SyncMobile(RepoPaths paths)
    {
        var source = paths.UserSecretsFile();
        if (source is null) return Commands.Fail("AppHost has no <UserSecretsId> — cannot locate the source secrets file.");
        var dest = paths.MobileUserSecretsFile();
        if (dest is null) return Commands.Fail("MobileUI has no <UserSecretsId> — add one to MobileUI.csproj.");

        if (!Load(source, out var values, out var err))
        {
            Console.WriteLine($"✗ {err}");
            Console.WriteLine("  Run `rewards-dev secrets edit`, paste the Keeper record, then re-try.");
            return 1;
        }

        // Validate the 2 mobile keys are present + real BEFORE writing anything.
        var missing = MobileKeys
            .Where(k => !(values.TryGetValue(k.Key, out var v) && !IsPlaceholder(v)))
            .Select(k => k.Key.Replace("Parameters:", ""))
            .ToList();
        if (missing.Count > 0)
        {
            Console.WriteLine($"✗ mobile secrets not set in the AppHost store: {string.Join(", ", missing)}");
            Console.WriteLine("  Paste the full Keeper record (`rewards-dev secrets edit`), then re-run sync-mobile.");
            return 1;
        }

        // Build the isolated store with ONLY the mobile keys — nothing else, ever.
        var mobileOnly = MobileKeys.ToDictionary(k => k.Key, k => values[k.Key]);
        Directory.CreateDirectory(System.IO.Path.GetDirectoryName(dest)!);
        File.WriteAllText(dest, JsonSerializer.Serialize(mobileOnly, new JsonSerializerOptions { WriteIndented = true }));

        // Materialize the git-ignored Firebase files the MAUI build needs.
        File.WriteAllText(paths.AndroidFirebaseFile, values["Parameters:mobile-google-services-json"]);
        File.WriteAllText(paths.IosFirebaseFile,     values["Parameters:mobile-google-service-info-plist"]);

        Console.WriteLine("✓ mobile secrets isolated + materialized (no backend secrets copied):");
        Console.WriteLine($"    store : {dest}  (2 keys only)");
        Console.WriteLine($"    files : {paths.Rel(paths.AndroidFirebaseFile)}");
        Console.WriteLine($"            {paths.Rel(paths.IosFirebaseFile)}");
        return 0;
    }

    // Read a user-secrets file into a flat string map. BOM-safe (dotnet writes UTF-8 ± BOM).
    private static bool Load(string file, out Dictionary<string, string> values, out string error)
    {
        values = new();
        if (!File.Exists(file)) { error = $"no secrets file yet: {file}"; return false; }
        try
        {
            using var doc = JsonDocument.Parse(File.ReadAllText(file));
            values = doc.RootElement.EnumerateObject()
                .ToDictionary(p => p.Name, p => p.Value.ValueKind == JsonValueKind.String ? p.Value.GetString() ?? "" : p.Value.GetRawText());
            error = "";
            return true;
        }
        catch (Exception ex)
        {
            error = $"secrets file is not valid JSON ({ex.Message}). Fix it: rewards-dev secrets edit";
            return false;
        }
    }

    // Empty, whitespace, or a short single-line "<…>" token (e.g. "<PASTE FROM KEEPER: …>") =
    // "not really set". A real plist value legitimately starts with "<?xml" and is multi-line, so
    // only treat a SHORT, single-line angle-bracket token as a placeholder — never real XML.
    private static bool IsPlaceholder(string v)
    {
        if (string.IsNullOrWhiteSpace(v)) return true;
        var t = v.Trim();
        if (t.Contains("Copy from Keeper", StringComparison.OrdinalIgnoreCase)) return true;
        if (t.Contains("REPLACE:", StringComparison.OrdinalIgnoreCase)) return true;
        if (t.Contains("PASTE FROM KEEPER", StringComparison.OrdinalIgnoreCase)) return true;
        return t.Length < 120
            && !t.Contains('\n')
            && t.StartsWith('<') && t.EndsWith('>')
            && !t.StartsWith("<?xml", StringComparison.OrdinalIgnoreCase);
    }
}

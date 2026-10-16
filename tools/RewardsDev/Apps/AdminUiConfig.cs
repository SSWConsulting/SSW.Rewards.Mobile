using System.Text.Json;
using SSW.Rewards.DevTool.Core;

namespace SSW.Rewards.DevTool.Apps;

// AdminUI: git-ignored src/AdminUI/wwwroot/appsettings.Local.json (RewardsApiUrl /
// Local:Authority), merged over the committed appsettings by the loader in
// AdminUI/Program.cs.
public static class AdminUiConfig
{
    public static bool Exists(RepoPaths paths) => File.Exists(paths.AdminLocalFile);

    public static string? ReadAuthority(RepoPaths paths)
    {
        if (!File.Exists(paths.AdminLocalFile)) return null;
        try
        {
            using var doc = JsonDocument.Parse(File.ReadAllText(paths.AdminLocalFile));
            return doc.RootElement.TryGetProperty("Local", out var local) && local.TryGetProperty("Authority", out var authority)
                ? authority.GetString()
                : null;
        }
        catch { return null; }
    }

    public static void Write(RepoPaths paths, TargetState state)
    {
        // Partial override — only the two switchable keys; everything else stays in
        // the committed appsettings(.Development).json.
        var obj = new Dictionary<string, object?>
        {
            ["_comment"] = "GIT-IGNORED. Written by rewards-dev. Overrides committed appsettings. Do not commit.",
            ["RewardsApiUrl"] = state.Api,
            ["Local"] = new Dictionary<string, object?> { ["Authority"] = state.Authority },
        };
        Directory.CreateDirectory(Path.GetDirectoryName(paths.AdminLocalFile)!);
        File.WriteAllText(paths.AdminLocalFile, JsonSerializer.Serialize(obj, new JsonSerializerOptions { WriteIndented = true }));
    }

    public static void Delete(RepoPaths paths)
    {
        if (File.Exists(paths.AdminLocalFile)) File.Delete(paths.AdminLocalFile);
    }
}

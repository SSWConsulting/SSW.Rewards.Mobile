using System.Text.RegularExpressions;
using SSW.Rewards.DevTool.Core;

namespace SSW.Rewards.DevTool.Apps;

// WebAPI: its token-validation authority lives in the AppHost user-secret
// Parameters:signing-authority (the WebAPI reads it via Aspire injection). Only
// identity switches touch it; the API URL dimension doesn't apply to the WebAPI.
public static class WebApiConfig
{
    public static (bool ok, string log) SetSigningAuthority(RepoPaths paths, string authority)
    {
        if (!File.Exists(paths.AppHostProject)) return (false, "AppHost project not found.");
        return ProcessRunner.Dotnet(
            $"user-secrets set \"Parameters:signing-authority\" \"{authority}\" --project \"{paths.AppHostProject}\"");
    }

    public static string? ReadSigningAuthority(RepoPaths paths)
    {
        if (!File.Exists(paths.AppHostProject)) return null;
        var (ok, log) = ProcessRunner.Dotnet($"user-secrets list --project \"{paths.AppHostProject}\"");
        if (!ok) return null;
        var m = Regex.Match(log, @"Parameters:signing-authority\s*=\s*(\S+)");
        return m.Success ? m.Groups[1].Value : null;
    }
}

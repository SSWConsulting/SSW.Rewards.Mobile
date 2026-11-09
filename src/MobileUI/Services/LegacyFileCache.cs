using Microsoft.Extensions.Logging;

namespace SSW.Rewards.Mobile.Services;

/// <summary>
/// The JSON files the app cached before the Akavache store. Nothing reads them any more and the
/// logout wipe can't reach them, so any left from an older version would outlive every session.
/// </summary>
public static class LegacyFileCache
{
    // Every key the old FileCacheService (and UserService's direct write) used, as "<key>.json".
    private static readonly string[] FilePatterns =
    [
        "MyUserProfile.json",
        "RewardsList.json",
        "QuizzesList.json",
        "NetworkProfilesCache.json",
        "leaderboard_*.json",
        "profile_*.json",
    ];

    /// <summary>Deletes the legacy cache files. Never throws.</summary>
    public static void Delete(ILogger logger)
    {
        var directory = FileSystem.CacheDirectory;

        foreach (var pattern in FilePatterns)
        {
            try
            {
                if (!Directory.Exists(directory))
                {
                    return;
                }

                foreach (var file in Directory.EnumerateFiles(directory, pattern))
                {
                    File.Delete(file);
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to delete legacy cache files matching {Pattern}", pattern);
            }
        }
    }
}

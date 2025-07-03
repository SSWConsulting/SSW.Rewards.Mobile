namespace SSW.Rewards.Application.Common.Constants;

public static class CacheKeys
{
    public const string Leaderboard = "LeaderboardAllTime";
    public const string UserRanking = "UserRankingsAllTime";
    public const string ClaimPrizeAchievementId = "ClaimPrizeAchievementId";
}

/// <summary>
/// Used to manage and remove multiple caches relative to their usage.
/// For instance, claiming an achievement impacts leaderboard and rankings while claiming a reward impacts only leaderboard (due to AdminUI).
/// 
/// As number of cache keys grows, this helps developer to organise cache keys by impact so they can be removed all at once for certain use case.
/// </summary>
public static class CacheTags
{
    public static readonly string[] UpdatedOnlyRewards = [CacheKeys.Leaderboard];
    public static readonly string[] Rankings = [CacheKeys.UserRanking];

    public static readonly string[] UpdatedRanking = [.. UpdatedOnlyRewards, .. Rankings];
    public static readonly string[] NewlyUserCreated = [.. UpdatedRanking];

    public static readonly string[] AllStatic = [.. UpdatedRanking];
}

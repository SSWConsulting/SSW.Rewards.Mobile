using SSW.Rewards.Enums;
using SSW.Rewards.Shared.DTOs.ActivityFeed;
using SSW.Rewards.Shared.DTOs.Users;
using SSW.Rewards.Shared.Utils;

namespace SSW.Rewards.Mobile.ViewModels;

public static class ActivityFeedItemExtensions
{
    /// <summary>
    /// Fills the display-only fields. Runs for cached and fresh items alike,
    /// so relative timestamps are always current.
    /// </summary>
    public static void PrepareForDisplay(this ActivityFeedItemDto item)
    {
        item.UserAvatar = string.IsNullOrWhiteSpace(item.UserAvatar) ? "v2sophie" : item.UserAvatar;
        item.UserTitle = RegexHelpers.WebsiteRegex().Replace(item.UserTitle, string.Empty);
        item.TimeElapsed = DateTimeHelpers.GetTimeElapsed(item.AwardedAt);
        if (item.Achievement is not null)
        {
            item.AchievementMessage = BuildAchievementMessage(item.Achievement);
        }
    }

    /// <summary>Compares every rendered field — matching items are kept on refresh, so a newly bound field belongs here.</summary>
    public static bool HasSameDisplayAs(this ActivityFeedItemDto item, ActivityFeedItemDto other) =>
        item.UserId == other.UserId
        && item.AwardedAt == other.AwardedAt
        && item.UserName == other.UserName
        && item.UserTitle == other.UserTitle
        && item.UserAvatar == other.UserAvatar
        && item.TimeElapsed == other.TimeElapsed
        && item.AchievementMessage == other.AchievementMessage
        && item.Achievement?.AchievementType == other.Achievement?.AchievementType;

    private static string BuildAchievementMessage(UserAchievementDto achievement)
    {
        string name = achievement.AchievementName;
        string scored = $"just scored {achievement.AchievementValue}pts for";

        string action = achievement.AchievementType switch
        {
            AchievementType.Attended => "checked into",
            AchievementType.Linked => $"{scored} linking",
            AchievementType.Scanned => $"{scored} scanning",
            _ => $"{scored} completing",
        };

        if (achievement.AchievementType == AchievementType.Linked)
        {
            name = name.Split(' ').Last();
        }

        action = char.ToUpper(action[0]) + action[1..];
        return $"{action} {name}";
    }
}

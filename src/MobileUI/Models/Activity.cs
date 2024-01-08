using SSW.Rewards.Enums;

namespace SSW.Rewards.Models;

public class Activity
{
    public ActivityType Type { get; set; }

    public string ActivityName { get; set; }

    public DateTime? OcurredAt { get; set; }
}

public enum ActivityType
{
    Met,
    Attended,
    Claimed,
    Completed,
    Linked
}

public static class ActivityTypeConverters
{
    public static ActivityType ToActivityType(this AchievementType type)
    {
        switch(type)
        {
            case AchievementType.Attended:
                return ActivityType.Attended;
            case AchievementType.Completed:
                return ActivityType.Completed;
            case AchievementType.Scanned:
                return ActivityType.Met;
            case AchievementType.Linked:
                return ActivityType.Linked;
            default:
                return ActivityType.Completed;
        }
    }
}

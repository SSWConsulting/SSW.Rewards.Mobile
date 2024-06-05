namespace SSW.Rewards.Mobile.Helpers;

public static class DateTimeHelpers
{
    public static string GetTimeElapsed(DateTime occurredAt)
    {
        return (DateTime.UtcNow - occurredAt) switch
        {
            { TotalMinutes: < 5 } ts => "Just now",
            { TotalHours: < 1 } ts => $"{ts.Minutes}m ago",
            { TotalDays: < 1 } ts => $"{ts.Hours}h ago",
            { TotalDays: < 31 } ts => $"{ts.Days}d ago",
            _ => occurredAt.ToLocalTime().ToString("dd MMMM yyyy"),
        };
    }
}
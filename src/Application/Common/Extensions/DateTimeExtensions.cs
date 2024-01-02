using System.Globalization;

namespace SSW.Rewards.Application.Common.Extensions;

public static class DateTimeExtensions
{
    public static DateTime FirstDayOfWeek(this DateTime dt)
    {
        var culture = CultureInfo.CurrentCulture;
        // (monday to sunday)
        var diff = dt.DayOfWeek - culture.DateTimeFormat.FirstDayOfWeek;
        if (diff < 0)
            diff += 7;
        return dt.AddDays(-diff).Date;
    }

    public static bool IsBetween(this DateTime dt, DateTime start, DateTime end)
    {
        return start <= dt && dt <= end;
    }
}
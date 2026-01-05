namespace SSW.Rewards.Admin.UI.Helpers;

public static class DateTimeFormatter
{
    public static string FormatLongDate(DateTime? date)
        => date.HasValue
            // Format based on https://www.ssw.com.au/rules/weekdays-on-date-selectors/
            ? $"{date.Value:ddd} {date.Value:d} {date.Value:t}"
            : "-";
}
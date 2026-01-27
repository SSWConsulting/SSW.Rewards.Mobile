using System.Globalization;

namespace SSW.Rewards.Mobile.Converters;

/// <summary>
/// Converts a UTC DateTime to local time
/// </summary>
public class UtcToLocalTimeConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is DateTime utcDateTime)
        {
            return utcDateTime.ToLocalTime();
        }
        return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

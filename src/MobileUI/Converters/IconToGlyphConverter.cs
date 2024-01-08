using System.Globalization;
using SSW.Rewards.Enums;

namespace SSW.Rewards.Mobile.Converters;

public class IconToGlyphConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var icon = (Icons)value;

        return (string)typeof(Icon).GetField(icon.ToString()).GetValue(null);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

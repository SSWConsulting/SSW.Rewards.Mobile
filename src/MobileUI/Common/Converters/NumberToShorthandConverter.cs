using System.Globalization;
using Microsoft.Maui.Controls;

namespace SSW.Rewards.Mobile.Converters;

public class NumberToShorthandConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int number)
        {
            return number >= 1000
                ? (number / 1000).ToString("0.#") + "k"
                : number.ToString("#,0");
        }
        return value?.ToString() ?? string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
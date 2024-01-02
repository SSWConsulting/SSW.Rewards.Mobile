using System.Globalization;

namespace SSW.Rewards.Mobile.Converters;

public class BoolToFontConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if ((bool)value)
        {
            // return FA
            //return Application.Current.Resources["FA6Brands"];
            return "FA6Brands";
        }
        else
        {
            // return fluent
            //return Application.Current.Resources["FluentIcons"];
            return "FluentIcons";
        }
        
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

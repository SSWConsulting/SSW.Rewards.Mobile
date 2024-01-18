using System.Globalization;

namespace SSW.Rewards.Mobile.Converters
{
    public class StaffToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value as bool? == true) ? "SSW" : "Community";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException("Only one way bindings are supported for this converter");
        }
    }
}
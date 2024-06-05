using System.Globalization;

namespace SSW.Rewards.Mobile.Converters
{
    public class IsMeToColorConverter : IValueConverter
    {
        public Color Me
        {
            get
            {
                if (Application.Current.Resources.TryGetValue("SSWRed", out var color))
                {
                    return (Color)color;
                }
                return Colors.Aqua;
            }
        }
        public Color NotMe
        {
            get
            {
                if (Application.Current.Resources.TryGetValue("FlyoutBackgroundColour", out var color))
                {
                    return (Color)color;
                }
                return Colors.Aqua;
            }
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool)value)
                return Me;
            else
                return NotMe;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException("One way bindings only on this converter");
        }
    }
}

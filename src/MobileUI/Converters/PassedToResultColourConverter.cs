using System.Globalization;

namespace SSW.Rewards.Mobile.Converters
{
    public class PassedToResultColourConverter : IValueConverter
    {
        public Color Passed
        {
            get
            {
                Application.Current.Resources.TryGetValue("SSWRed", out var color);
                return (Color)color;
            }
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var myVal = value;

            if ((bool)value)
                    return Passed;

            return Colors.White;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

using System.Globalization;

namespace SSW.Rewards.Mobile.Converters
{
    public class PassedToResultColourConverter : IValueConverter
    {
        public Color Passed
        {
            get
            {
                return (Color)Application.Current.Resources["SSWRed"];
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

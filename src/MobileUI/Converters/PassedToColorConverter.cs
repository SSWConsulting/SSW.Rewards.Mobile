using System.Globalization;

namespace SSW.Rewards.Mobile.Converters
{
    public class PassedToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var passed = (bool)value;

            if (passed)
            {
                Application.Current.Resources.TryGetValue("CorrectColor", out var color);
                return (Color)color;
            }
            else
            {
                Application.Current.Resources.TryGetValue("SSWRed", out var color);
                return (Color)color;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

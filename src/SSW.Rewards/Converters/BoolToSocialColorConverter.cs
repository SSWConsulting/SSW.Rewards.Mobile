using System.Globalization;

namespace SSW.Rewards.Mobile.Converters
{
    public class BoolToSocialColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var isEnabled = (bool)value;

            if (isEnabled)
            {
                return Colors.White;
            }
            else
            {
                return Color.FromHex("707070");
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

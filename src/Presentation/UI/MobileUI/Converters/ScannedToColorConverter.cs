using System.Globalization;

namespace SSW.Rewards.Mobile.Converters
{
    public class ScannedToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var scanned = (bool)value;

            if (scanned)
            {
                return Color.FromHex("cc4141");
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

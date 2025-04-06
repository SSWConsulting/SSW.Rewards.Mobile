using System.Globalization;
using SkiaSharp.Extended.UI.Controls.Converters;

namespace SSW.Rewards.Mobile.Converters
{
    class StringToSkLottieImageSourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var file = (string)value;
            SKLottieImageSourceConverter _lottieConverter = new();
            return String.IsNullOrEmpty(file) ? null : _lottieConverter.ConvertFromString(file);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

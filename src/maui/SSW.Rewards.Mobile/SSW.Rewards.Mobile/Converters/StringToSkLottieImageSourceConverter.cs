using System.Globalization;
using Microsoft.IdentityModel.Tokens;
using SkiaSharp.Extended.UI.Controls.Converters;

namespace SSW.Rewards.Mobile.Converters
{
    class StringToSkLottieImageSourceConverter : IValueConverter, IMarkupExtension
    {
        private readonly SKLottieImageSourceConverter _lottieConverter = new();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var file = (string)value;
            return file.IsNullOrEmpty() ? null : _lottieConverter.ConvertFromString(file);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}

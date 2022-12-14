using System;
using System.Globalization;
using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace SSW.Rewards.Converters
{
    public class ScannedToColorConverter : IValueConverter, IMarkupExtension
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

        public object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}

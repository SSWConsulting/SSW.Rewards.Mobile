using System;
using System.Globalization;
using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace SSW.Rewards.Converters
{
    public class AchievementCompletedColorConverter : IValueConverter, IMarkupExtension
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var completed = (bool)value;

            if (completed)
            {
                return Color.FromHex("FFCC4141");
            }
            else
            {
                return Color.FromHex("#414141");
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

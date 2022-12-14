using System;
using System.Globalization;
using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace SSW.Rewards.Converters
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

            return Color.White;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

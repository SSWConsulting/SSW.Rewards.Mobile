using System;
using System.Globalization;
using Xamarin.Forms;

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

using System;
using System.Globalization;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SSW.Rewards.Converters
{
    public class IsMeToColorConverter : IValueConverter, IMarkupExtension
    {
        public Color Me
        {
            get
            {
                return (Color)Application.Current.Resources["LeaderSummarySelected"];
            }
        }
        public Color NotMe
        {
            get
            {
                return (Color)Application.Current.Resources["LeaderSummary"];
            }
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool)value)
                return Me;
            else
                return NotMe;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException("One way bindings only on this converter");
        }

        public object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}

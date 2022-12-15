using System.Globalization;

namespace SSW.Rewards.Mobile.Converters
{
    public class BoolToFontConverter : IValueConverter, IMarkupExtension
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool)value)
            {
                // return FA
                return Application.Current.Resources["FA6Brands"];
            }
            else
            {
                // return fluent
                return Application.Current.Resources["FluentIcons"];
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

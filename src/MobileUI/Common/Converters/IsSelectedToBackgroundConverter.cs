using System.Globalization;

namespace SSW.Rewards.Mobile.Converters;

public class IsSelectedToBackgroundConverter : IValueConverter
{
    public GradientBrush Selected
    {
        get
        {
            Application.Current.Resources.TryGetValue("BrandBrush", out var brush);
            return (GradientBrush)brush;
        }
    }

    public SolidColorBrush Unselected => new SolidColorBrush(Colors.Transparent);
    
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return (bool)value ? Selected : Unselected;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
using System.Globalization;

namespace SSW.Rewards.Mobile.Converters;

public class IsMeTagConverter: IValueConverter, IMarkupExtension
{
    public Color Me
    {
        get
        {
            return Colors.Red;
        }
    }
    public Color NotMe
    {
        get
        {
            Application.Current.Resources.TryGetValue("LeaderSummary", out var color);
            return (Color)color;
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

using System.Globalization;

namespace SSW.Rewards.Mobile.Converters;

public class AchievementCompletedColorConverter : IValueConverter, IMarkupExtension
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var completed = (bool)value;

        if (completed)
        {
            return Color.FromArgb("FFCC4141");
        }
        else
        {
            return Color.FromArgb("#414141");
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

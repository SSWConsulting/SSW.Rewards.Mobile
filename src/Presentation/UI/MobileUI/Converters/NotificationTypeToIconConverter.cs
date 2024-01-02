using System.Globalization;

namespace SSW.Rewards.Mobile.Converters
{
    public class NotificationTypeToIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var type = (NotificationType)value;

            switch (type)
            {
                case NotificationType.Alert:
                default:
                    return "\ue016";
                case NotificationType.Event:
                    return "\ue248";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

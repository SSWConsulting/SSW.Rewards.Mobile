using System.Globalization;

namespace SSW.Rewards.Mobile.Converters
{
    public class ActivityTypeToIconConverter : IValueConverter, IMarkupExtension
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var type = (ActivityType)value;

            switch(type)
            {
                case ActivityType.Attended:
                    return "\ue248";
                case ActivityType.Met:
                    return "\ue8ed";
                case ActivityType.Linked:
                    return "\ue99b";
                case ActivityType.Completed:
                default:
                    return "\uea30";
                case ActivityType.Claimed:
                    return "\uedba";
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

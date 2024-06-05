using System.Globalization;
using SSW.Rewards.Enums;

namespace SSW.Rewards.Mobile.Converters
{
    class LevelToStarsConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
            List<bool> list = new ();
            int stars;
            
            switch (value)
            {
                case SkillLevel.Beginner:
                    stars = 1;
                    break;
                case SkillLevel.Intermediate:
                    stars = 3;
                    break;
                case SkillLevel.Advanced:
                    stars = 5;
                    break;
                default:
                    return 0;
            }
            
            for (int i = 0; i < 5; i++)
            {
                list.Add(i < stars);
            }

            return list;
        }

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotSupportedException("Only one way bindings are supported for this converter");
		}
	}
}

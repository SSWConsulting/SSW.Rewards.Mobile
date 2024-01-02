using System.Globalization;

namespace SSW.Rewards.Mobile.Converters
{
    public class PassedToBackgroundConverter : IValueConverter
    {
        public Color Passed
        {
            get
            {
                Application.Current.Resources.TryGetValue("PassedQuiz", out var color);
                return (Color)color;
            }
        }

        public Color NotPassed
        {
            get
            {
                Application.Current.Resources.TryGetValue("Quiz", out var color);
                return (Color)color;
            }
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if((bool)value)
            {
                return Passed;
            }
            else
            {
                return NotPassed;
            }
                    
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

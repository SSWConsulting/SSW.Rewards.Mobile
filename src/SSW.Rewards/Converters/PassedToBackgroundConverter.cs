﻿using System;
using System.Globalization;
using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace SSW.Rewards.Converters
{
    public class PassedToBackgroundConverter : IValueConverter
    {
        public Color Passed
        {
            get
            {
                return (Color)Application.Current.Resources["PassedQuiz"];
            }
        }

        public Color NotPassed
        {
            get
            {
                return (Color)Application.Current.Resources["Quiz"];
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
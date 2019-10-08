using System;
using System.Globalization;
using SSW.Consulting.Models;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SSW.Consulting.Converters
{
    public class SkillToBadgeConverter : IValueConverter, IMarkupExtension
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return string.Empty;
            //ImageSource source;
            string imageResourceName;

            switch((DevSkills)value)
            {
                case DevSkills.Angular:
                    imageResourceName = "skill_angular";
                    break;
                case DevSkills.Beer:
                    imageResourceName = "skill_beer";
                    break;
                case DevSkills.Dancing:
                    imageResourceName = "skill_dancing";
                    break;
                case DevSkills.DevOps:
                    imageResourceName = "skill_devops";
                    break;
                case DevSkills.Scrum:
                    imageResourceName = "skill_scrum";
                    break;
                case DevSkills.SharePoint:
                    imageResourceName = "skill_sharepoint";
                    break;
                case DevSkills.iOS:
                    imageResourceName = "skill_ios";
                    break;
                case DevSkills.NETCore:
                    imageResourceName = "skill_netcore";
                    break;
                case DevSkills.Node:
                    imageResourceName = "skill_node";
                    break;
                case DevSkills.PowerBI:
                    imageResourceName = "skill_powerbi";
                    break;
                case DevSkills.React:
                    imageResourceName = "skill_react";
                    break;
                case DevSkills.Smoking:
                    imageResourceName = "skill_smoking";
                    break;
                default:
                    imageResourceName = "skill_dev";
                    break;
            }

            return imageResourceName;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException("Only one way bindings are supported for this converter");
        }

        public object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}

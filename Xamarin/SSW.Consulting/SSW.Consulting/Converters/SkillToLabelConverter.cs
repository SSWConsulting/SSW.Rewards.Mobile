using System;
using System.Collections.Generic;
using System.Globalization;
using SSW.Consulting.Models;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SSW.Consulting.Converters
{
	class SkillToLabelConverter : IValueConverter, IMarkupExtension
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value == null)
				return string.Empty;
			string label;

			switch ((DevSkills)value)
			{
				case DevSkills.Angular:
					label = "Angular";
					break;
				case DevSkills.Beer:
					label = "Beer";
					break;
				case DevSkills.Dancing:
					label = "Dancing";
					break;
				case DevSkills.DevOps:
					label = "Dev Ops";
					break;
				case DevSkills.iOS:
					label = "iOS";
					break;
				case DevSkills.NETCore:
					label = ".NET Core";
					break;
				case DevSkills.Node:
					label = "NodeJS";
					break;
				case DevSkills.PowerBI:
					label = "Power BI";
					break;
				case DevSkills.React:
					label = "React";
					break;
				case DevSkills.Smoking:
					label = "Smoking";
					break;
				default:
					label = "";
					break;
			}

			return label;
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

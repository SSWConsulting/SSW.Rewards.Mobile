using System;
using System.Collections.Generic;
using System.Globalization;
using SSW.Rewards.Models;
using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace SSW.Rewards.Mobile.Converters
{
	class SkillToLabelConverter : IValueConverter, IMarkupExtension
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value == null)
				return string.Empty;
			return "";
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

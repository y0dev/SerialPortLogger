using System;
using System.Globalization;
using System.Windows.Data;

namespace SerialLogAnalyzer.Helpers.Converters
{
	public class ViewToBoolConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			// Check if the value is the same as the parameter
			return value?.GetType().Name == parameter.ToString();
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return value is bool boolValue && boolValue ? parameter : null;
		}
	}
}

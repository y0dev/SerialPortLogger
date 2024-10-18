using System;
using System.Globalization;
using System.Windows.Data;

namespace SerialLogAnalyzer.Helpers
{
	public class ThemeConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value == null || parameter == null)
				return false;

			string selectedTheme = value.ToString();
			string targetTheme = parameter.ToString();

			// Check if the current theme matches the parameter (target theme)
			return selectedTheme.Equals(targetTheme, StringComparison.OrdinalIgnoreCase);
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}

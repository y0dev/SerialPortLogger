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
			if (value == null || parameter == null)
				return Binding.DoNothing;

			// Check if the value is a boolean representing whether the theme is selected
			bool isChecked = (bool)value;
			string themeName = parameter.ToString();

			// Return the theme name if the checkbox is checked
			if (isChecked)
			{
				return themeName; // Return the name of the theme if it's selected
			}

			return Binding.DoNothing; // If not checked, do nothing
		}
	}
}

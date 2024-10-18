using System;
using System.Windows;

namespace SerialLogAnalyzer.Helpers
{
	public class LightTheme : ResourceDictionary
	{
		public LightTheme()
		{
			// Load the LightTheme resource dictionary
			Source = new Uri("pack://application:,,,/Themes/LightTheme.xaml");
		}
	}

	public class DarkTheme : ResourceDictionary
	{
		public DarkTheme()
		{
			// Load the DarkTheme resource dictionary
			Source = new Uri("pack://application:,,,/Themes/DarkTheme.xaml");
		}
	}
}

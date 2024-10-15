using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SerialLogAnalyzer.Views
{
	/// <summary>
	/// Interaction logic for SettingsWindow.xaml
	/// </summary>
	public partial class SettingsWindow : Window
	{
		public SettingsWindow()
		{
			InitializeComponent();
			LoadThemes();
		}


		// Logic to save the settings
		private void SaveButton_Click(object sender, RoutedEventArgs e)
		{
			// bool darkTheme = darkThemeCheckBox.IsChecked ?? false;
			bool showLineNumbers = lineNumbersCheckBox.IsChecked ?? false;
			bool enableAutoSave = autoSaveCheckBox.IsChecked ?? false;

			// Save these settings to config (or apply them immediately)
			MessageBox.Show("Settings saved!", "Settings", MessageBoxButton.OK, MessageBoxImage.Information);

			this.Close(); // Close the settings window
		} // End of SaveButton_Click()


		private void LoadThemes()
		{
			// Example themes, replace with actual themes as needed
			List<string> themes = new List<string>
			{
				"Light Theme",
				"Dark Theme",
				"Blue Theme",
				"Green Theme"
			};

			// Populate the ComboBox with the themes
			themeComboBox.ItemsSource = themes;
			themeComboBox.SelectedIndex = 0; // Optionally set a default selection
		} // End of LoadThemes()

		private void ThemeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			// Handle theme change here
			if (themeComboBox.SelectedItem != null)
			{
				string selectedTheme = themeComboBox.SelectedItem.ToString();
				ApplyTheme(selectedTheme);
			}
		}

		private void ApplyTheme(string theme)
		{
			// Logic to apply the selected theme
			// This could involve changing resources or applying styles
			switch (theme)
			{
				case "Light Theme":
					// Apply light theme settings
					break;
				case "Dark Theme":
					// Apply dark theme settings
					break;
				case "Blue Theme":
					// Apply blue theme settings
					break;
				case "Green Theme":
					// Apply green theme settings
					break;
				default:
					break;
			}
		}
	}

}

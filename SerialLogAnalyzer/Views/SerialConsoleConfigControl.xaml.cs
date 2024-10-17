using SerialLogAnalyzer.Helpers;
using SerialLogAnalyzer.Models;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SerialLogAnalyzer
{
	public partial class SerialConsoleConfigWindow : Window
	{
		public SerialConsoleConfigWindow()
		{
			InitializeComponent();
		}

		private void CreateButton_Click(object sender, RoutedEventArgs e)
		{
			// Collect user input
			string configName = ConfigNameTextBox.Text;
			string title = TitleTextBox.Text;
			string selectedColorScheme = (ColorSchemeComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
			int fontSize = int.Parse((FontSizeComboBox.SelectedItem as ComboBoxItem)?.Content.ToString());

			// Ensure required fields are filled
			if (string.IsNullOrWhiteSpace(configName) || !configName.Contains("COM"))
			{
				MessageBox.Show("Please provide a valid Serial Console Config name that contains 'COM'.");
				return;
			}

			// Close window and return result (create a SerialConsoleConfig object)
			SerialConsoleConfig newConfig = new SerialConsoleConfig
			{
				Name = configName,
				Title = title,
				ColorScheme = selectedColorScheme,
				FontSize = 14
			};

			this.Tag = newConfig;  // Store the result in the window's Tag property
			this.DialogResult = true; // Close the window and return success
			this.Close();
		} // End of CreateButton_Click()

		private void ColorSchemeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (ColorSchemeComboBox.SelectedItem is ComboBoxItem selectedItem)
			{
				string selectedScheme = selectedItem.Content.ToString();
				// Set background and text color based on selected scheme
				switch (selectedScheme)
				{
					case "Light Mode":
						ColorPreviewLabel.Background = ColorScheme.ConvertToBrush(ColorScheme.LightMode.BackgroundColor);
						ColorPreviewLabel.Foreground = ColorScheme.ConvertToBrush(ColorScheme.LightMode.TextColor);
						break;
					case "Dark Mode":
						ColorPreviewLabel.Background = ColorScheme.ConvertToBrush(ColorScheme.DarkMode.BackgroundColor);
						ColorPreviewLabel.Foreground = ColorScheme.ConvertToBrush(ColorScheme.DarkMode.TextColor);
						break;
					case "Solarized Dark":
						ColorPreviewLabel.Background = ColorScheme.ConvertToBrush(ColorScheme.SolarizedDark.BackgroundColor);
						ColorPreviewLabel.Foreground = ColorScheme.ConvertToBrush(ColorScheme.SolarizedDark.TextColor);
						break;
					case "Solarized Light":
						ColorPreviewLabel.Background = ColorScheme.ConvertToBrush(ColorScheme.SolarizedLight.BackgroundColor);
						ColorPreviewLabel.Foreground = ColorScheme.ConvertToBrush(ColorScheme.SolarizedLight.TextColor);
						break;
					case "Monokai":
						ColorPreviewLabel.Background = ColorScheme.ConvertToBrush(ColorScheme.Monokai.BackgroundColor);
						ColorPreviewLabel.Foreground = ColorScheme.ConvertToBrush(ColorScheme.Monokai.TextColor);
						break;
					case "Gruvbox Dark":
						ColorPreviewLabel.Background = ColorScheme.ConvertToBrush(ColorScheme.GruvboxDark.BackgroundColor);
						ColorPreviewLabel.Foreground = ColorScheme.ConvertToBrush(ColorScheme.GruvboxDark.TextColor);
						break;
					case "Gruvbox Light":
						ColorPreviewLabel.Background = ColorScheme.ConvertToBrush(ColorScheme.GruvboxLight.BackgroundColor);
						ColorPreviewLabel.Foreground = ColorScheme.ConvertToBrush(ColorScheme.GruvboxLight.TextColor);
						break;
					case "Nord":
						ColorPreviewLabel.Background = ColorScheme.ConvertToBrush(ColorScheme.Nord.BackgroundColor);
						ColorPreviewLabel.Foreground = ColorScheme.ConvertToBrush(ColorScheme.Nord.TextColor);
						break;
					case "Ocean":
						ColorPreviewLabel.Background = ColorScheme.ConvertToBrush(ColorScheme.Ocean.BackgroundColor);
						ColorPreviewLabel.Foreground = ColorScheme.ConvertToBrush(ColorScheme.Ocean.TextColor);
						break;
					case "Desert":
						ColorPreviewLabel.Background = ColorScheme.ConvertToBrush(ColorScheme.Desert.BackgroundColor);
						ColorPreviewLabel.Foreground = ColorScheme.ConvertToBrush(ColorScheme.Desert.TextColor);
						break;
					case "Retro":
						ColorPreviewLabel.Background = ColorScheme.ConvertToBrush(ColorScheme.Retro.BackgroundColor);
						ColorPreviewLabel.Foreground = ColorScheme.ConvertToBrush(ColorScheme.Retro.TextColor);
						break;
					case "Cyber Punk":
						ColorPreviewLabel.Background = ColorScheme.ConvertToBrush(ColorScheme.Cyberpunk.BackgroundColor);
						ColorPreviewLabel.Foreground = ColorScheme.ConvertToBrush(ColorScheme.Cyberpunk.TextColor);
						break;
					case "Twilight":
						ColorPreviewLabel.Background = ColorScheme.ConvertToBrush(ColorScheme.Twilight.BackgroundColor);
						ColorPreviewLabel.Foreground = ColorScheme.ConvertToBrush(ColorScheme.Twilight.TextColor);
						break;
					case "Forest":
						ColorPreviewLabel.Background = ColorScheme.ConvertToBrush(ColorScheme.Forest.BackgroundColor);
						ColorPreviewLabel.Foreground = ColorScheme.ConvertToBrush(ColorScheme.Forest.TextColor);
						break;
					case "Sunset":
						ColorPreviewLabel.Background = ColorScheme.ConvertToBrush(ColorScheme.Sunset.BackgroundColor);
						ColorPreviewLabel.Foreground = ColorScheme.ConvertToBrush(ColorScheme.Sunset.TextColor);
						break;
					default:
						ColorPreviewLabel.Background = ColorScheme.ConvertToBrush(ColorScheme.Default.BackgroundColor);
						ColorPreviewLabel.Foreground = ColorScheme.ConvertToBrush(ColorScheme.Default.TextColor);
						break;
				}
			}
		} // End of ColorSchemeComboBox_SelectionChanged()
	}
}

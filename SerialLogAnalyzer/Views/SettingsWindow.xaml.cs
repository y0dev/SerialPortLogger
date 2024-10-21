using SerialLogAnalyzer.Helpers;
using SerialLogAnalyzer.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

		public ObservableCollection<string> AvailableFonts { get; private set; }
		public ObservableCollection<int> FontSizes { get; set; }
		public string SelectedFont { get; set; }
		public int SelectedFontSize { get; set; }
		private Logger logger;

		public SettingsWindow()
		{
			InitializeComponent();

			AvailableFonts = new ObservableCollection<string>
			{
				"Segoe UI",       // Default font for Windows applications
				"Arial",          // Very common sans-serif font, widely used
				"Tahoma",         // Sans-serif font with good readability
				"Times New Roman",// Common serif font, often used for documents
				"Verdana",        // Another sans-serif font known for readability
				"Calibri"         // Default font for Microsoft Office applications
			};

			// Initialize with common baud rates
			FontSizes = new ObservableCollection<int> { 8, 10, 12, 14, 16, 18, 20, 22, 24, 26 };
			//SelectedBaudRate = 115200; // Default baud rate

			// Set the DataContext to itself for binding
			this.DataContext = this;
			
			// Get the MainViewModel from resources
			var mainViewModel = (MainViewModel)FindResource("MainViewModel");

			SelectedFont = mainViewModel.Config.Settings.Font;
			SelectedFontSize = mainViewModel.Config.Settings.FontSize;

			logger = Logger.GetInstance("slate_app.log", false);
		}

		// Logic to save the settings
		private void SaveButton_Click(object sender, RoutedEventArgs e)
		{

			// Get the MainViewModel from resources
			var mainViewModel = (MainViewModel)FindResource("MainViewModel");

			// Check if fontsComboBox has a selected value and if it has changed
			if (fontsComboBox.SelectedItem != null && !string.IsNullOrEmpty(fontsComboBox.SelectedItem.ToString())
				&& mainViewModel.Config.Settings.Font != fontsComboBox.SelectedItem.ToString())
			{
				string selectedFont = fontsComboBox.SelectedItem.ToString();
				logger.Log($"Changing font from {mainViewModel.Config.Settings.Font} to {selectedFont}.", LogLevel.Info);

				// Update the font setting
				mainViewModel.Config.Settings.Font = selectedFont;
			}

			// Check if fontSizeComboBox has a selected value, convert it to an integer, and check if it has changed
			if (fontSizeComboBox.SelectedItem != null && int.TryParse(fontSizeComboBox.SelectedItem.ToString(), out int selectedFontSize)
				&& mainViewModel.Config.Settings.FontSize != selectedFontSize)
			{
				logger.Log($"Changing font size from {mainViewModel.Config.Settings.FontSize} to {selectedFontSize}.", LogLevel.Info);

				// Update the font size setting
				mainViewModel.Config.Settings.FontSize = selectedFontSize;
			}

			// Save the updated configuration if changes were made
			mainViewModel.SaveConfig();


			this.Close(); // Close the settings window
		} // End of SaveButton_Click()
	}

}

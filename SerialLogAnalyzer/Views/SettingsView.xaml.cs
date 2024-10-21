using SerialLogAnalyzer.Helpers;
using SerialLogAnalyzer.ViewModels;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace SerialLogAnalyzer.Views
{
	public partial class SettingsView : UserControl
	{
		public ObservableCollection<string> AvailableFonts { get; private set; }
		public ObservableCollection<int> FontSizes { get; set; }
		public bool IsDarkTheme { get; set; }
		public string SelectedFont { get; set; }
		public int SelectedFontSize { get; set; }
		private Logger logger;
		MainViewModel mainViewModel;

		public SettingsView(MainViewModel mainViewModel)
		{
			InitializeComponent();

			AvailableFonts = new ObservableCollection<string>
			{
				"Segoe UI",
				"Arial",
				"Tahoma",
				"Times New Roman",
				"Verdana",
				"Calibri"
			};

			FontSizes = new ObservableCollection<int> { 8, 10, 12, 14, 16, 18, 20, 22, 24, 26 };

			this.mainViewModel = mainViewModel;
			
			var theme = mainViewModel.Config?.Settings.Theme ?? "Light";
			IsDarkTheme = theme == "Dark" ? true : false;
			SelectedFont = mainViewModel.Config.Settings.Font;
			SelectedFontSize = mainViewModel.Config.Settings.FontSize;

			logger = Logger.GetInstance("slate_app.log", false);

			// Set the DataContext to itself for binding
			this.DataContext = this;
		}

		private void SaveButton_Click(object sender, RoutedEventArgs e)
		{


			if (fontsComboBox.SelectedItem != null &&
				!string.IsNullOrEmpty(fontsComboBox.SelectedItem.ToString()) &&
				mainViewModel.Config.Settings.Font != fontsComboBox.SelectedItem.ToString())
			{
				string selectedFont = fontsComboBox.SelectedItem.ToString();
				logger.Log($"Changing font from {mainViewModel.Config.Settings.Font} to {selectedFont}.", LogLevel.Info);
				mainViewModel.Config.Settings.Font = selectedFont;
			}

			if (fontSizeComboBox.SelectedItem != null &&
				int.TryParse(fontSizeComboBox.SelectedItem.ToString(), out int selectedFontSize) &&
				mainViewModel.Config.Settings.FontSize != selectedFontSize)
			{
				logger.Log($"Changing font size from {mainViewModel.Config.Settings.FontSize} to {selectedFontSize}.", LogLevel.Info);
				mainViewModel.Config.Settings.FontSize = selectedFontSize;
			}

			if (themeToggleButton.IsChecked.HasValue && 
				themeToggleButton.IsChecked.Value != (mainViewModel.Config.Settings.Theme == "Dark"))
			{
				// Log the theme change
				logger.Log($"Changing theme from {((mainViewModel.Config.Settings.Theme == "Dark") ? "Dark" : "Light")} to {(themeToggleButton.IsChecked.Value ? "Dark" : "Light")}.", LogLevel.Info);

				// Update the settings to the new theme
				mainViewModel.Config.Settings.Theme = themeToggleButton.IsChecked.Value ? "Dark": "Light";
				
			}

			mainViewModel.SaveConfig();
		}

		private void ToggleButton_Click(object sender, RoutedEventArgs e)
		{
			ToggleButton toggleButton = (ToggleButton)sender;
			Console.WriteLine($"Toggle button: {toggleButton.IsChecked}");
			string theme = toggleButton.IsChecked == true ? "Dark" : "Light";
			ChangeTheme(theme);
		}

		private void ChangeTheme(string themeName)
		{
			ResourceDictionary theme = null;
			// Set the theme in the MainViewModel
			logger.Log($"Changing theme from {mainViewModel.Config.Settings.Theme} to {themeName}.", LogLevel.Info);


			// Theme switching logic
			switch (themeName)
			{
				case "Light":
					// Apply light theme resources or styles
					theme = new LightTheme();
					break;
				case "Dark":
					// Apply dark theme resources or styles
					theme = new DarkTheme();
					break;
				default:
					// Handle other themes
					break;
			}


			// Clear existing resources and add the new theme
			Application.Current.Resources.MergedDictionaries.Clear();
			Application.Current.Resources.MergedDictionaries.Add(theme);
			
			logger.Log($"Changed theme", LogLevel.Info);
		}
	}
}

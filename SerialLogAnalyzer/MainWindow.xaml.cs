using Microsoft.Win32;
using SerialLogAnalyzer.Models;
using SerialLogAnalyzer.Views;
using SerialLogAnalyzer.Services;
using System;
using System.Windows;
using System.Xml.Linq;
using System.Windows.Controls;
using SerialLogAnalyzer.ViewModels;
using SerialLogAnalyzer.Helpers;
using System.Windows.Media;

namespace SerialLogAnalyzer
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public string SelectedTheme { get; set; } = "Light"; // Default theme
		private Logger logger;
		private const int MinFontSize = 8;
		private const int MaxFontSize = 28;

		public MainWindow()
		{
			InitializeComponent();
			DataContext = this; // Set the data context for binding
			logger = Logger.GetInstance("slate_app.log", false);

			// Get the MainViewModel from resources
			var mainViewModel = (MainViewModel)FindResource("MainViewModel");

			// Set the DataContext to MainViewModel for data binding
			DataContext = mainViewModel;

			// Set the theme based on the configuration
			SelectedTheme = mainViewModel.Config?.Settings.Theme ?? "Light"; 
			ChangeTheme(Char.ToUpper(SelectedTheme[0]) + SelectedTheme.Substring(1));

			string currentFont = mainViewModel.Config?.Settings.Font ?? "Segoe UI";
			int currentFontSize = mainViewModel.Config?.Settings.FontSize ?? 12;

			ChangeFontAndSize(currentFont, currentFontSize);
		}
		

		// File menu event handlers (existing ones)

		private void NewMenuItem_Click(object sender, RoutedEventArgs e)
		{
			NewConfigWindow newConfigWindow = new NewConfigWindow();
			newConfigWindow.ShowDialog();
		}
	
		private void SaveMenuItem_Click(object sender, RoutedEventArgs e)
		{
			
		}

		private void ExitMenuItem_Click(object sender, RoutedEventArgs e)
		{
			Application.Current.Shutdown();
		}

		private void OpenConfigMenuItem_Click(object sender, RoutedEventArgs e)
		{
			// Use OpenFileDialog to allow the user to select a config.xml file
			OpenFileDialog openFileDialog = new OpenFileDialog
			{
				Filter = "XML files (*.xml)|*.xml|All files (*.*)|*.*",
				Title = "Open Config File"
			};

			if (openFileDialog.ShowDialog() == true)
			{
				string configFilePath = openFileDialog.FileName;

				// Display or process the XML config file
				try
				{
					ConfigurationService configService = new ConfigurationService(Properties.Resources.CONFIG_PATH);
					// Load and parse the XML file
					AppConfiguration customConfig = configService.LoadCustomConfiguration(configFilePath);
					if (customConfig != null)
					{
						MessageBox.Show($"Successfully opened config file: {configFilePath}");
					}
					

					// You can now access specific nodes in the XML and process it as needed
				}
				catch (Exception ex)
				{
					MessageBox.Show($"Error opening config file: {ex.Message}");
				}
			}
		} // End of OpenConfigMenuItem_Click()
				
		// Event handler for Analysis Options menu item
		private void AnalysisOptionsMenuItem_Click(object sender, RoutedEventArgs e)
		{
			// Logic to show analysis options (could be a separate window or dialog)
			MessageBox.Show("Show Analysis Options", "Analysis Options", MessageBoxButton.OK, MessageBoxImage.Information);
		}

		// Event handler for Generate Report menu item
		private void GenerateReportMenuItem_Click(object sender, RoutedEventArgs e)
		{
			// Logic to generate a report of the analysis or logs
			MessageBox.Show("Generating Report...", "Generate Report", MessageBoxButton.OK, MessageBoxImage.Information);
			// Add actual report generation logic here
		}

		// Event handler for Check for Updates menu item
		private void CheckUpdatesMenuItem_Click(object sender, RoutedEventArgs e)
		{
			// Logic to check for application updates
			MessageBox.Show("Checking for Updates...", "Check for Updates", MessageBoxButton.OK, MessageBoxImage.Information);
			// Add actual update-checking logic here
		} // End of CheckUpdatesMenuItem_Click()

		// Event handler for Contact Support menu item
		private void ContactSupportMenuItem_Click(object sender, RoutedEventArgs e)
		{
			// Logic to contact support (could be an email or a dialog box with support contact info)
			MessageBox.Show("Contacting Support...", "Contact Support", MessageBoxButton.OK, MessageBoxImage.Information);
			// Implement actual support contact logic (e.g., open email, show a support form, etc.)
		} // End of ContactSupportMenuItem_Click()

		// Event handler for View Help menu item
		private void ViewHelpMenuItem_Click(object sender, RoutedEventArgs e)
		{
			HelpPage helpPage = new HelpPage();
			helpPage.ShowDialog();
		} // End of ViewHelpMenuItem_Click()

		// Help menu event handler
		private void AboutMenuItem_Click(object sender, RoutedEventArgs e)
		{
			AboutPage aboutPage = new AboutPage();
			aboutPage.ShowDialog(); // Show it as a dialog
		} // End of AboutMenuItem_Click()

		// Event handler for Export Data menu item
		private void ExportDataMenuItem_Click(object sender, RoutedEventArgs e)
		{
			// Logic to export data
			MessageBox.Show("Exporting Data...", "Export Data", MessageBoxButton.OK, MessageBoxImage.Information);
			// Add actual export logic here (e.g., export logs or analysis data to a file)
		} // End of ExportDataMenuItem_Click()

		// Event handler for Import Data menu item
		private void ImportDataMenuItem_Click(object sender, RoutedEventArgs e)
		{
			// Logic to import data
			MessageBox.Show("Importing Data...", "Import Data", MessageBoxButton.OK, MessageBoxImage.Information);
			// Add actual import logic here (e.g., load data from a file for analysis or logging)
		} // End of ImportDataMenuItem_Click()
		
		// Logic to apply the theme (replace with your theme switching logic)
		private void ChangeTheme(string themeName)
		{
			ResourceDictionary theme = null;
			// Set the theme in the MainViewModel
			var mainViewModel = (MainViewModel)DataContext;
			logger.Log($"Changing theme from {mainViewModel.Config.Settings.Theme} to {themeName}.", LogLevel.Info);
			mainViewModel.Config.Settings.Theme = themeName; // Update the configuration
			mainViewModel.SaveConfig(); // Save the updated configuration


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

			// Force re-evaluation of the data bindings to update the check state
			DataContext = null;
			DataContext = mainViewModel; // Reassign the DataContext to refresh bindings
			logger.Log($"Changed theme", LogLevel.Info);
		}

		private void ChangeFontAndSize(string selectedFont, int selectedFontSize)
		{
			// Validate font size
			if (selectedFontSize < MinFontSize || selectedFontSize > MaxFontSize)
			{
				logger.Log($"Attempted to set font size {selectedFontSize}, which is out of range ({MinFontSize}-{MaxFontSize}).", LogLevel.Warning);
				return; // Exit the method if the font size is invalid
			}
			// Set the font in the MainViewModel
			var mainViewModel = (MainViewModel)DataContext;

			// Log the font and font size change
			logger.Log($"Changing font from {mainViewModel.Config.Settings.Font} (size {mainViewModel.Config.Settings.FontSize}) " +
					   $"to {selectedFont} (size {selectedFontSize}).", LogLevel.Info);

			// Update the configuration
			mainViewModel.Config.Settings.Font = selectedFont; // Update font
			mainViewModel.Config.Settings.FontSize = selectedFontSize; // Update font size
			mainViewModel.SaveConfig(); // Save the updated configuration

			// Apply the new font and font size to the application resources
			Application.Current.Resources["ApplicationFontFamily"] = new FontFamily(selectedFont);
			Application.Current.Resources["ApplicationFontSize"] = selectedFontSize;

			// Force re-evaluation of the data bindings to update the font
			DataContext = null;
			DataContext = mainViewModel; // Reassign the DataContext to refresh bindings
			logger.Log($"Changed font to {selectedFont} and size to {selectedFontSize}.", LogLevel.Info);
		}

		private void RadioButton_Checked(object sender, RoutedEventArgs e)
		{
			RadioButton radioButton = (RadioButton)sender;
			Console.WriteLine($"Radio Button Selected: {radioButton.Content}");

		}
	}
}

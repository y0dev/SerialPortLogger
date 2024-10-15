using Microsoft.Win32;
using SerialLogAnalyzer.ViewModels;
using SerialLogAnalyzer.Views;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;

namespace SerialLogAnalyzer
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
		}

		private void ApplySettings(string theme, string commandLineTheme, bool defaultCheckboxConfig)
		{
			// Apply settings like themes or defaults to the UI
			this.Background = (theme == "Dark") ? System.Windows.Media.Brushes.DarkGray : System.Windows.Media.Brushes.White;
		}

		// File menu event handlers (existing ones)

		private void NewMenuItem_Click(object sender, RoutedEventArgs e)
		{
			MessageBox.Show("New File");
			// Implement your new file logic here
		}

		private void OpenMenuItem_Click(object sender, RoutedEventArgs e)
		{
			MessageBox.Show("Open File");
			// Implement your open file logic here
		}

		private void SaveMenuItem_Click(object sender, RoutedEventArgs e)
		{
			MessageBox.Show("Save File");
			// Implement your save file logic here
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
					// Load and parse the XML file
					XDocument configXml = XDocument.Load(configFilePath);
					MessageBox.Show($"Successfully opened config file: {configFilePath}");

					// Process the XML data (example: display root node)
					MessageBox.Show($"Root node: {configXml.Root.Name}");

					// You can now access specific nodes in the XML and process it as needed
				}
				catch (Exception ex)
				{
					MessageBox.Show($"Error opening config file: {ex.Message}");
				}
			}
		} // End of OpenConfigMenuItem_Click()

		// Event handler for Settings menu item
		private void SettingsMenuItem_Click(object sender, RoutedEventArgs e)
		{
			// Show a simple settings window/dialog
			SettingsWindow settingsWindow = new SettingsWindow();
			settingsWindow.ShowDialog();
		} // End of OpenConfigMenuItem_Click()

		// Event handler for Show Analysis menu item
		private void ShowAnalysisMenuItem_Click(object sender, RoutedEventArgs e)
		{
			// Logic to show the analysis tab or analysis-related data
			MainTabControl.SelectedIndex = 1; // Assuming the second tab is the Analysis tab
		}

		// Event handler for Show Log menu item
		private void ShowLogMenuItem_Click(object sender, RoutedEventArgs e)
		{
			// Logic to show the logger tab or log-related data
			MainTabControl.SelectedIndex = 0; // Assuming the first tab is the Logger tab
		}

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

		// Event handler for Change Theme menu item
		private void ChangeThemeMenuItem_Click(object sender, RoutedEventArgs e)
		{
			// Logic to change the theme (e.g., dark/light)
			MessageBox.Show("Changing Theme...", "Change Theme", MessageBoxButton.OK, MessageBoxImage.Information);
			// Implement actual theme-changing logic here
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
			// Logic to show help documentation or help dialog
			MessageBox.Show("Opening Help...", "Help", MessageBoxButton.OK, MessageBoxImage.Information);
			// Add actual help documentation display logic here (e.g., open a PDF, show a help window, etc.)
		} // End of ViewHelpMenuItem_Click()

		// Help menu event handler
		private void AboutMenuItem_Click(object sender, RoutedEventArgs e)
		{
			MessageBox.Show("About this application...");
			// Implement your about dialog logic here
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

	}
}
